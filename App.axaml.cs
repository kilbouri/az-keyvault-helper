using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KeyVaultHelper.Models.Data;
using KeyVaultHelper.Models.State;
using KeyVaultHelper.Services;
using KeyVaultHelper.ViewModels;
using KeyVaultHelper.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KeyVaultHelper;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Register all the services needed for the application to run
        var collection = new ServiceCollection();
        ConfigureServices(collection);

        // Creates a ServiceProvider containing services from the provided IServiceCollection
        var services = collection.BuildServiceProvider();

        var mainWindowVm = services.GetRequiredService<MainWindowViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow() { DataContext = mainWindowVm };
        }

        // Start background loading of Azure resources
        _ = StartBackgroundResourceLoadAsync(services);

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<AzureUserService>();
        services.AddSingleton<AzureResourceService>();
        services.AddSingleton<AzureResourceCache>();
        services.AddSingleton<ResourceLoadingState>();

        // View models probably need to be transient.
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<KeyVaultTabViewModel>();
        services.AddTransient<OpenVaultDialogViewModel>();

        // Add logging
        services.AddLogging(logging => logging.AddConsole());
    }

    private async Task StartBackgroundResourceLoadAsync(IServiceProvider services)
    {
        var loadingState = services.GetRequiredService<ResourceLoadingState>();
        var cache = services.GetRequiredService<AzureResourceCache>();

        loadingState.IsLoading = true;
        loadingState.LoadError = null;

        try
        {
            var progress = new Progress<Subscription>(sub => loadingState.CurrentSubscriptionName = sub.Name);
            await cache.ReloadSubscriptionsAsync(progress, CancellationToken.None);
            loadingState.IsLoading = false;
            loadingState.LoadError = null;
        }
        catch (Exception ex)
        {
            loadingState.IsLoading = false;
            loadingState.LoadError = $"Failed to load subscriptions: {ex.Message}";
        }
    }
}
