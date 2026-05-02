using Avalonia.Controls;
using KeyVaultHelper.ViewModels;

namespace KeyVaultHelper.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var viewModel = new MainWindowViewModel();
        DataContext = viewModel;
        InitializeComponent();

        // Initialize index service in background on window load
        Loaded += async (s, e) =>
        {
            await viewModel.InitializeAsync();
        };
    }
}
