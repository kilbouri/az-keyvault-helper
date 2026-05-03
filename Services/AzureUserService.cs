using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;

namespace KeyVaultHelper.Services;

public sealed class AzureUserService
{
    private readonly TokenCredential _userCredential;
    private readonly ArmClient _azureClient;

    public AzureUserService()
    {
        // TODO: we can disk-cache both this credential and the interactive browser one, but we have to do so in order
        _userCredential = new AzureCliCredential();
        _azureClient = new(_userCredential);
    }

    public ArmClient GetAzureResourceClient()
    {
        return _azureClient;
    }

    public string GetDefaultSubscriptionId()
    {
        return _azureClient.GetDefaultSubscription().Id.SubscriptionId!;
    }
}
