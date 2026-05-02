using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KeyVaultHelper.Services;

/// <summary>
/// Represents a single notification/toast message to display to the user.
/// </summary>
public class NotificationViewModel(string message, NotificationSeverity severity, int durationSeconds = 5) : ObservableObject
{
    private string _message = message;
    private NotificationSeverity _severity = severity;
    private int _durationSeconds = durationSeconds;

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public NotificationSeverity Severity
    {
        get => _severity;
        set => SetProperty(ref _severity, value);
    }

    /// <summary>
    /// How long (in seconds) before the toast auto-dismisses. 0 = don't auto-dismiss.
    /// </summary>
    public int DurationSeconds
    {
        get => _durationSeconds;
        set => SetProperty(ref _durationSeconds, value);
    }
}

/// <summary>
/// Severity level for notifications.
/// </summary>
public enum NotificationSeverity
{
    Information,
    Warning,
    Error,
    Success
}

/// <summary>
/// Service for managing user notifications (toasts).
/// Provides methods to show notifications and exposes an observable collection for UI binding.
/// </summary>
public class NotificationService
{
    private readonly ObservableCollection<NotificationViewModel> _notifications;

    public IReadOnlyList<NotificationViewModel> Notifications => _notifications;

    public NotificationService()
    {
        _notifications = [];
    }

    /// <summary>
    /// Shows an error notification.
    /// </summary>
    public void ShowError(string message)
    {
        var notification = new NotificationViewModel(message, NotificationSeverity.Error, durationSeconds: 7);
        AddNotification(notification);
    }

    /// <summary>
    /// Shows a warning notification.
    /// </summary>
    public void ShowWarning(string message)
    {
        var notification = new NotificationViewModel(message, NotificationSeverity.Warning, durationSeconds: 5);
        AddNotification(notification);
    }

    /// <summary>
    /// Shows a success notification.
    /// </summary>
    public void ShowSuccess(string message)
    {
        var notification = new NotificationViewModel(message, NotificationSeverity.Success, durationSeconds: 3);
        AddNotification(notification);
    }

    /// <summary>
    /// Shows an informational notification.
    /// </summary>
    public void ShowInfo(string message)
    {
        var notification = new NotificationViewModel(message, NotificationSeverity.Information, durationSeconds: 5);
        AddNotification(notification);
    }

    private void AddNotification(NotificationViewModel notification)
    {
        _notifications.Add(notification);

        // Auto-dismiss after the specified duration
        if (notification.DurationSeconds > 0)
        {
            _ = Task.Delay(notification.DurationSeconds * 1000).ContinueWith(_ =>
            {
                // Remove from UI thread
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _notifications.Remove(notification);
                });
            });
        }
    }

    /// <summary>
    /// Removes a specific notification from the collection (called when user dismisses it).
    /// </summary>
    public void DismissNotification(NotificationViewModel notification)
    {
        _notifications.Remove(notification);
    }

    /// <summary>
    /// Clears all notifications.
    /// </summary>
    public void ClearAll()
    {
        _notifications.Clear();
    }
}
