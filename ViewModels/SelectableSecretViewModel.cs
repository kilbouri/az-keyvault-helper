using CommunityToolkit.Mvvm.ComponentModel;
using KeyVaultHelper.Models.Data;

namespace KeyVaultHelper.ViewModels;

public partial class SelectableSecretViewModel(KeyVault.Secret secret) : ObservableObject
{
    private readonly KeyVault.Secret _secret = secret;

    [ObservableProperty]
    public partial bool IsSelected { get; set; } = false;

    public string SecretName => _secret.Name;

    public KeyVault.Secret Secret => _secret;
}
