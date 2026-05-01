using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyVaultHelper.Models.Data;

namespace KeyVaultHelper.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<KeyVaultTabViewModel> OpenTabs { get; set; }
    [ObservableProperty]
    public partial KeyVaultTabViewModel? SelectedTab { get; set; }
    public ICommand OpenNewVaultCommand { get; }

    public MainWindowViewModel()
    {
        OpenTabs = [];
        OpenNewVaultCommand = new RelayCommand(OpenNewVault);
    }

    private void OpenNewVault()
    {
        // Create fake data for a single vault
        var subscription = new Subscription("Production Subscription");
        var resourceGroup = new ResourceGroup(subscription, "prod-rg");
        var secrets = new List<KeyVault.Secret>
        {
            new("database-password", new Optional<string>("fake-db-pwd")),
            new("api-key-external", new Optional<string>("fake-api-key")),
            new("connection-string", new Optional<string>("Server=...;User=...")),
            new("jwt-secret", new Optional<string>("fake-jwt-token")),
            new("stripe-api-key", new Optional<string>("sk_test_xxx"))
        };
        var keyVault = new KeyVault(resourceGroup, "prod-keyvault-001", secrets);
        OpenKeyVault(keyVault);
    }

    public void OpenKeyVault(KeyVault keyVault)
    {
        var tabViewModel = new KeyVaultTabViewModel(keyVault);
        OpenTabs.Add(tabViewModel);
        SelectedTab = tabViewModel;
    }

    public void CloseTab(KeyVaultTabViewModel tab)
    {
        OpenTabs.Remove(tab);
        if (SelectedTab == tab && OpenTabs.Count > 0)
        {
            SelectedTab = OpenTabs[^1];
        }
        else if (OpenTabs.Count == 0)
        {
            SelectedTab = null;
        }
    }
}
