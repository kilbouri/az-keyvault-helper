using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyVaultHelper.Models.Data;
using KeyVaultHelper.Services;
using KeyVaultHelper.Views;

namespace KeyVaultHelper.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AzureResourceCache _indexService;
    private readonly NotificationService _notificationService;

    [ObservableProperty]
    public partial ObservableCollection<KeyVaultTabViewModel> OpenTabs { get; set; }

    [ObservableProperty]
    public partial KeyVaultTabViewModel? SelectedTab { get; set; }

    public NotificationService NotificationService => _notificationService;
    public IAsyncRelayCommand OpenVaultCommand { get; }

    public MainWindowViewModel()
    {
        var azureService = new AzureResourceService();
        _notificationService = new NotificationService();
        _indexService = new AzureResourceCache(azureService);

        OpenTabs = [];
        OpenVaultCommand = new AsyncRelayCommand(OpenVaultAsync);
    }

    /// <summary>
    /// Initializes the index service and starts background loading of subscriptions, RGs, and vaults.
    /// Should be called when the window loads. Does not block the UI.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _indexService.ReloadSubscriptionsAsync();
        Console.WriteLine("Initial resource cache has loaded");
    }

    /// <summary>
    /// Opens the OpenVaultDialog modal and adds the selected vault as a new tab.
    /// </summary>
    private async Task OpenVaultAsync()
    {
        var dialogViewModel = new OpenVaultDialogViewModel(_indexService, _notificationService);
        var dialog = new OpenVaultDialog() { DataContext = dialogViewModel };

        var result = await dialog.ShowDialog<KeyVault?>(GetMainWindow()!);
        if (result is not null)
        {
            OpenKeyVault(result);
        }
    }

    /// <summary>
    /// Opens a key vault in a new tab.
    /// </summary>
    public void OpenKeyVault(KeyVault keyVault)
    {
        var tabViewModel = new KeyVaultTabViewModel(keyVault);
        OpenTabs.Add(tabViewModel);
        SelectedTab = tabViewModel;
    }

    /// <summary>
    /// Closes a specific tab.
    /// </summary>
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

    private Window? GetMainWindow()
    {
        // Try to get the main window from the application
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}
