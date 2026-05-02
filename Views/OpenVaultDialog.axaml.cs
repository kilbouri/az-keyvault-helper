using Avalonia.Controls;
using KeyVaultHelper.ViewModels;

namespace KeyVaultHelper.Views;

public partial class OpenVaultDialog : Window
{
    public OpenVaultDialog()
    {
        InitializeComponent();
    }

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(null);
    }

    private void OnOpenClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is OpenVaultDialogViewModel vm && vm.SelectedVault != null)
        {
            Close(vm.SelectedVault);
        }
    }
}
