using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyVaultHelper.Models;
using KeyVaultHelper.Models.Data;
using KeyVaultHelper.Services;
using KeyVaultHelper.Views;
using Microsoft.Extensions.DependencyInjection;

namespace KeyVaultHelper.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AzureResourceCache _resourceCache;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    public partial ObservableCollection<KeyVaultTabViewModel> OpenTabs { get; set; }

    [ObservableProperty]
    public partial KeyVaultTabViewModel? SelectedTab { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingResources { get; set; }

    [ObservableProperty]
    public partial string? ResourceLoadError { get; set; }

    public IAsyncRelayCommand OpenVaultCommand { get; }

    public IAsyncRelayCommand RetryLoadCommand { get; }

    public MainWindowViewModel(AzureResourceCache resourceCache, IServiceProvider serviceProvider)
    {
        _resourceCache = resourceCache;
        _serviceProvider = serviceProvider;

        OpenTabs = [];
        OpenVaultCommand = new AsyncRelayCommand(OpenVaultAsync);
        RetryLoadCommand = new AsyncRelayCommand(async () => await _resourceCache.ReloadSubscriptionsAsync());
        _ = InitializeAsync();
    }

    /// <summary>
    /// Initializes the cache and starts background loading of subscriptions, RGs, and vaults.
    /// Should be called when the window loads. Does not block the UI.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _resourceCache.ReloadSubscriptionsAsync();
    }

    /// <summary>
    /// Opens the OpenVaultDialog modal and adds the selected vault as a new tab.
    /// </summary>
    private async Task OpenVaultAsync()
    {
        var dialogViewModel = _serviceProvider.GetRequiredService<OpenVaultDialogViewModel>();
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
        // This is sufficient because Azure requires all KeyVaults to have a unique name
        var existingTab = OpenTabs.FirstOrDefault(vm => vm.KeyVaultName == keyVault.Id);
        if (existingTab is not null)
        {
            SelectedTab = existingTab;
            return;
        }

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
