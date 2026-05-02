using CommunityToolkit.Mvvm.ComponentModel;
using KeyVaultHelper.Models.Data;

namespace KeyVaultHelper.ViewModels;

public partial class KeyVaultTabViewModel(KeyVault _keyVault) : ObservableObject
{
    public string KeyVaultName => _keyVault.Id;
    public string SubscriptionName => _keyVault.ResourceGroup.Subscription.Name;
    public string ResourceGroupName => _keyVault.ResourceGroup.Name;
    public string BreadcrumbPath => $"{SubscriptionName} / {ResourceGroupName} / {KeyVaultName}";
}
