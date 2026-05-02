using CommunityToolkit.Mvvm.ComponentModel;
using KeyVaultHelper.Extensions;
using KeyVaultHelper.Models.Data;

namespace KeyVaultHelper.Services;

/// <summary>
/// Manages a hierarchical cache of subscriptions, resource groups, and key vaults.
/// Supports partial refresh operations that cascade deletion of dependent items.
/// </summary>
public class AzureResourceCache(AzureResourceService _azureService) : ObservableObject
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

    public async Task ReloadSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var subscriptionJobs = _azureService
            .PageSubscriptionsAsync(cancellationToken)
            .Select(page => page.Select(sub => (
                Subscription: sub,
                Vaults: _azureService.PageKeyVaultsAsync(sub, cancellationToken)
            )))
            .Flatten()
            .ConfigureAwait(false);

        SubscriptionCache newSubscriptionCache = new();

        await foreach (var subscriptionJob in subscriptionJobs)
        {
            ResourceGroupCache resourceGroupCache = new();

            await foreach (var vault in subscriptionJob.Vaults.Flatten().ConfigureAwait(false))
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

    public IEnumerable<Subscription> GetSubscriptions()
    {
        if (_subscriptionCache is null)
        {
            return [];
        }

        return _subscriptionCache.Values.Select(item => item.Subscription);
    }

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
