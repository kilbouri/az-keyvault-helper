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
