using System.Runtime.CompilerServices;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.Resources;
using KeyVaultHelper.Models.Data;

namespace KeyVaultHelper.Services;

/// <summary>
/// Service for interacting with Azure resources (subscriptions, resource groups, key vaults, and secrets).
/// Handles authentication and permission errors gracefully.
/// </summary>
public class AzureResourceService
{
    private readonly ArmClient _resourceClient;
    private readonly TokenCredential _azureCredential;


    /// <summary>
    /// Uses the <see cref="DefaultAzureCredential" /> with interactive authentication enabled.
    /// </summary>
    public AzureResourceService()
    {
        try
        {
            _azureCredential = new DefaultAzureCredential(includeInteractiveCredentials: true);
            _resourceClient = new ArmClient(_azureCredential);
        }
        catch (Exception ex)
        {
            throw new AzureAuthenticationException("Failed to initialize Azure authentication. Ensure you are logged in via Azure CLI or have appropriate environment credentials.", ex);
        }
    }

    /// <summary>
    /// Lists all subscriptions the authenticated user has access to.
    /// </summary>
    public async IAsyncEnumerable<IEnumerable<Subscription>> PageSubscriptionsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var subscriptions = _resourceClient.GetSubscriptions().GetAllAsync(cancellationToken);
        await foreach (var page in subscriptions.AsPages().ConfigureAwait(false))
        {
            yield return page.Values.Select(sub => new Subscription(sub.Id.Name, sub.Data.DisplayName));
        }
    }

    /// <summary>
    /// Lists all resource groups in the specified subscription.
    /// </summary>
    public async IAsyncEnumerable<IEnumerable<ResourceGroup>> PageResourceGroupsAsync(
        Subscription subscription,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var foundSubscription = await _resourceClient
            .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription.Id))
            .GetAsync(cancellationToken)
            .ConfigureAwait(false);

        var resourceGroups = foundSubscription.Value.GetResourceGroups().GetAllAsync(cancellationToken: cancellationToken);
        await foreach (var page in resourceGroups.AsPages().ConfigureAwait(false))
        {
            yield return page.Values.Select(rg => new ResourceGroup(subscription, rg.Id.Name));
        }
    }

    /// <summary>
    /// Lists all key vaults in the specified subscription.
    /// </summary>
    public async IAsyncEnumerable<IEnumerable<KeyVault>> PageKeyVaultsAsync(
        Subscription subscription,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var foundSubscription = await _resourceClient
            .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription.Id))
            .GetAsync(cancellationToken)
            .ConfigureAwait(false);

        var keyVaults = foundSubscription.Value.GetKeyVaultsAsync(cancellationToken: cancellationToken);
        await foreach (var page in keyVaults.AsPages().ConfigureAwait(false))
        {
            yield return page.Values.Select(kv => new KeyVault(
                new ResourceGroup(
                    subscription,
                    kv.Id.ResourceGroupName ?? throw new NullReferenceException($"Identifier for {kv.Id.Name} has no resource group name")
                ),
                kv.Id.Name
            ));
        }
    }

    /// <summary>
    /// Lists all key vaults in the specified resource group.
    /// </summary>
    public async IAsyncEnumerable<IEnumerable<KeyVault>> PageKeyVaultsAsync(
        ResourceGroup resourceGroup,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var foundResourceGroup = await _resourceClient
            .GetResourceGroupResource(ResourceGroupResource.CreateResourceIdentifier(
                subscriptionId: resourceGroup.Subscription.Id,
                resourceGroupName: resourceGroup.Name
            ))
            .GetAsync(cancellationToken)
            .ConfigureAwait(false);

        var vaults = foundResourceGroup.Value.GetKeyVaults().GetAllAsync(cancellationToken: cancellationToken);
        await foreach (var page in vaults.AsPages().ConfigureAwait(false))
        {
            yield return page.Values.Select(vault => new KeyVault(resourceGroup, vault.Id.Name));
        }
    }
}

/// <summary>
/// Exception thrown when Azure authentication fails (invalid credentials, not logged in, etc.).
/// </summary>
public class AzureAuthenticationException(string message, Exception? innerException = null) : Exception(message, innerException);
