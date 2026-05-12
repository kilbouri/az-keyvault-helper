using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyVaultHelper.Models.Data;
using KeyVaultHelper.Services;
using KeyVaultHelper.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KeyVaultHelper.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    public partial ObservableCollection<KeyVaultTabViewModel> OpenTabs { get; set; }

    [ObservableProperty]
    public partial KeyVaultTabViewModel? SelectedTab { get; set; }

    public IAsyncRelayCommand OpenVaultCommand { get; }

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        OpenTabs = [];
        OpenVaultCommand = new AsyncRelayCommand(ShowOpenVaultDialogAsync);
    }

    /// <summary>
    /// Opens the OpenVaultDialog modal and adds the selected vault as a new tab.
    /// </summary>
    private async Task ShowOpenVaultDialogAsync()
    {
        var dialogViewModel = _serviceProvider.GetRequiredService<OpenVaultDialogViewModel>();
        _ = dialogViewModel.InitializeAsync();

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

        var resourceService = _serviceProvider.GetRequiredService<AzureResourceService>();
        var logger = _serviceProvider.GetRequiredService<ILogger<KeyVaultTabViewModel>>();
        var tabViewModel = new KeyVaultTabViewModel(keyVault, resourceService, logger);
        OpenTabs.Add(tabViewModel);
        SelectedTab = tabViewModel;

        // Load secrets asynchronously without blocking
        _ = tabViewModel.LoadSecretsAsync();
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
