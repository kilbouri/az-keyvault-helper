using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KeyVaultHelper.Services;
using KeyVaultHelper.ViewModels;
using KeyVaultHelper.Views;
using Microsoft.Extensions.DependencyInjection;

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

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<AzureUserService>();
        services.AddSingleton<AzureResourceService>();
        services.AddSingleton<AzureResourceCache>();

        // View models probably need to be transient.
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<KeyVaultTabViewModel>();
        services.AddTransient<OpenVaultDialogViewModel>();
    }
}
