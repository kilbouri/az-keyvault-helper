using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KeyVaultHelper.Models.Data;

namespace KeyVaultHelper.ViewModels;

public partial class KeyVaultTabViewModel : ObservableObject
{
    private readonly KeyVault _keyVault;

    [ObservableProperty]
    public partial ObservableCollection<SelectableSecretViewModel> Secrets { get; set; }

    public string KeyVaultName => _keyVault.Name;
    public string SubscriptionName => _keyVault.ResourceGroup.Subscription.Name;
    public string ResourceGroupName => _keyVault.ResourceGroup.Name;
    public string BreadcrumbPath => $"{SubscriptionName} / {ResourceGroupName} / {KeyVaultName}";

    public KeyVaultTabViewModel(KeyVault keyVault)
    {
        _keyVault = keyVault;
        Secrets = new ObservableCollection<SelectableSecretViewModel>(
            _keyVault.Secrets.Select(s => new SelectableSecretViewModel(s))
        );
    }
}
