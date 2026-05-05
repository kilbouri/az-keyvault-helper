using CommunityToolkit.Mvvm.ComponentModel;
using KeyVaultHelper.Extensions;
using KeyVaultHelper.Models.Data;

namespace KeyVaultHelper.Services;

/// <summary>
/// Manages a hierarchical cache of subscriptions, resource groups, and key vaults.
/// </summary>
public partial class AzureResourceCache(AzureResourceService _azureService) : ObservableObject
{
    private sealed class SubscriptionCache : Dictionary<string, SubscriptionCache.Item>
    {
        public sealed record Item(Subscription Subscription, ResourceGroupCache ResourceGroups);
    }

    private sealed class ResourceGroupCache : Dictionary<string, ResourceGroupCache.Item>
    {
        public sealed record Item(ResourceGroup ResourceGroup, KeyVaultCache KeyVaults);
    }

    private sealed class KeyVaultCache : Dictionary<string, KeyVaultCache.Item>
    {
        public sealed record Item(KeyVault KeyVault);
    }

    private SubscriptionCache? _subscriptionCache;

    /// <summary><para>
    /// Asynchronously invalidates the currently cached data and fetches a replacement from Azure.
    /// <paramref name="subscriptionProgress"/> reports each <see cref="Subscription"/> when that
    /// subscription begins being processed.
    /// </para><para>
    /// The cached data is not replaced until this job completes. If the job is cancelled via
    /// <paramref name="cancellationToken"/> or an exception is thrown, the original cached data
    /// will remain intact and unchanged.
    /// </para></summary>
    public async Task ReloadSubscriptionsAsync(IProgress<Subscription>? subscriptionProgress = null, CancellationToken cancellationToken = default)
    {
        SubscriptionCache newSubscriptionCache = new();

        var subscriptionJobs = _azureService
            .PageSubscriptionsAsync(cancellationToken)
            .Flatten()
            .Select(sub => (Subscription: sub, Vaults: _azureService.PageKeyVaultsAsync(sub, cancellationToken)))
            .ConfigureAwait(false)
            .WithCancellation(cancellationToken);

        await foreach (var subscriptionJob in subscriptionJobs)
        {
            subscriptionProgress?.Report(subscriptionJob.Subscription);

            ResourceGroupCache resourceGroupCache = new();
            await foreach (var vault in subscriptionJob.Vaults.Flatten().ConfigureAwait(false).WithCancellation(cancellationToken))
            {
                var rgVaultCache = resourceGroupCache.GetOrInsert(vault.ResourceGroup.Name, new(vault.ResourceGroup, new KeyVaultCache()));
                rgVaultCache.KeyVaults.Add(vault.Id, new(vault));
            }

            newSubscriptionCache.Add(subscriptionJob.Subscription.Id, new(subscriptionJob.Subscription, resourceGroupCache));
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            _subscriptionCache = newSubscriptionCache;
        }
    }

    /// <summary>
    /// Enumerates the cached subscriptions.
    /// </summary>
    /// <returns>An iterator over the cached subscriptions.</returns>
    public IEnumerable<Subscription> GetSubscriptions()
    {
        if (_subscriptionCache is null)
        {
            return [];
        }

        return _subscriptionCache.Values.Select(item => item.Subscription);
    }

    /// <summary>
    /// Enumerates the <see cref="ResourceGroup"/>s in the <see cref="Subscription"/> with id
    /// <paramref name="subscriptionId"/>. If no such subscription id is cached, returns an empty
    /// iterator.  
    /// </summary>
    /// <param name="subscriptionId">
    /// Id of the <see cref="Subscription" /> to retrieve resource groups for.
    /// </param>
    /// <returns>
    /// The cached resource groups in the identified subscription, or an empty iterator if no
    /// subscription could be found.
    /// </returns>
    public IEnumerable<ResourceGroup> GetResourceGroups(string subscriptionId)
    {
        if (_subscriptionCache is null)
        {
            return [];
        }

        if (!_subscriptionCache.TryGetValue(subscriptionId, out var subscriptionItem))
        {
            return [];
        }

        return subscriptionItem.ResourceGroups.Values.Select(item => item.ResourceGroup);
    }

    /// <summary>
    /// Enumerates the <see cref="KeyVault"/>s in the given <see cref="Subscription"/> and
    /// <see cref="ResourceGroup"/> identified by <paramref name="subscriptionId"/> and
    /// <paramref name="resourceGroupName"/> respectively. If one or both don't exist in the
    /// cache, an empty iterator is returned.  
    /// </summary>
    /// <param name="subscriptionId">
    /// Id of the <see cref="Subscription"/> to retrieve the KeyVaults within
    /// </param> 
    /// <param name="resourceGroupName">
    /// Name of the <see cref="ResourceGroup"/> to retrieve the KeyVaults within
    /// </param>
    /// <returns>
    /// The cached key vaults within the identified subscription and resource group, or an empty
    /// iterator if one or both is not found.
    /// </returns>  
    public IEnumerable<KeyVault> GetKeyVaults(string subscriptionId, string resourceGroupName)
    {
        if (_subscriptionCache is null)
        {
            return [];
        }

        if (!_subscriptionCache.TryGetValue(subscriptionId, out var subscriptionItem))
        {
            return [];
        }

        if (!subscriptionItem.ResourceGroups.TryGetValue(resourceGroupName, out var keyVaultItem))
        {
            return [];
        }

        return keyVaultItem.KeyVaults.Values.Select(item => item.KeyVault);
    }
}
