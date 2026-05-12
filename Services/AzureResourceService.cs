using System.Runtime.CompilerServices;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.Resources;
using KeyVaultHelper.Models.Data;

namespace KeyVaultHelper.Services;

/// <summary>
/// Service for interacting with Azure resources (subscriptions, resource groups, key vaults, and secrets).
/// </summary>
/// <remarks>
/// Uses the <see cref="AzureCliCredential" /> to authenticate with Azure.
/// </remarks>
public class AzureResourceService(AzureUserService azureUserService)
{
    private readonly ArmClient _resourceClient = azureUserService.GetAzureResourceClient();

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

    /// <summary>
    /// Lists all secrets in the specified key vault.
    /// </summary>
    public async IAsyncEnumerable<IEnumerable<Secret>> GetSecretsAsync(
        KeyVault keyVault,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var foundKeyVault = await _resourceClient.GetKeyVaultResource(KeyVaultResource.CreateResourceIdentifier(
                subscriptionId: keyVault.ResourceGroup.Subscription.Id,
                resourceGroupName: keyVault.ResourceGroup.Name,
                vaultName: keyVault.Id
            ))
            .GetAsync(cancellationToken)
            .ConfigureAwait(false);

        var secrets = foundKeyVault.Value.GetKeyVaultSecrets().GetAllAsync(cancellationToken: cancellationToken);
        await foreach (var page in secrets.AsPages().ConfigureAwait(false))
        {
            yield return page.Values.Select(secret => new Secret(
                secret.Data.Name,
                secret.Data.Properties.ContentType,
                secret.Data.Properties.Attributes.Updated,
                secret.Data.Properties.Attributes.Expires
            ));
        }
    }
}
