using Avalonia.Controls;
using KeyVaultHelper.ViewModels;

namespace KeyVaultHelper.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new MainWindowViewModel();
        InitializeComponent();
    }
}
