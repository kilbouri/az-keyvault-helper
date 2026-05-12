using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyVaultHelper.Models.Data;
using KeyVaultHelper.Services;
using Microsoft.Extensions.Logging;

namespace KeyVaultHelper.ViewModels;

public partial class KeyVaultTabViewModel(KeyVault _keyVault, AzureResourceService _resourceService, ILogger<KeyVaultTabViewModel> _logger) : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<Secret> Secrets { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoadingSecrets { get; set; }

    [ObservableProperty]
    public partial string? LoadingMessage { get; set; }

    [ObservableProperty]
    public partial string? SecretLoadError { get; set; }

    public string KeyVaultName => _keyVault.Id;
    public string SubscriptionName => _keyVault.ResourceGroup.Subscription.Name;
    public string ResourceGroupName => _keyVault.ResourceGroup.Name;
    public string BreadcrumbPath => $"{SubscriptionName} / {ResourceGroupName} / {KeyVaultName}";

    /// <summary>
    /// Loads secrets for the vault. Called automatically when the tab is created.
    /// </summary>
    public async Task LoadSecretsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("LoadSecretsAsync called for {KeyVaultName}", KeyVaultName);

        if (IsLoadingSecrets)
        {
            _logger.LogDebug("Already loading, returning");
            return;
        }

        IsLoadingSecrets = true;
        LoadingMessage = "Loading secrets...";
        SecretLoadError = null;

        try
        {
            _logger.LogInformation("Fetching secrets from service for {KeyVaultName}", KeyVaultName);
            var secrets = await _resourceService.GetSecretsAsync(_keyVault, cancellationToken);
            _logger.LogInformation("Received {SecretCount} secrets", secrets.Count());

            foreach (var secret in secrets)
            {
                _logger.LogDebug("Adding secret: {SecretName}", secret.Name);
                Secrets.Add(secret);
            }

            _logger.LogInformation("Total secrets in collection: {TotalCount}", Secrets.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Secret loading operation cancelled");
            // Silently ignore cancellation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception loading secrets");
            SecretLoadError = $"Failed to load secrets: {ex.Message}";
        }
        finally
        {
            IsLoadingSecrets = false;
            LoadingMessage = null;
        }
    }
}
