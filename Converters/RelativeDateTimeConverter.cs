using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace KeyVaultHelper.Converters;

/// <summary>
/// Converts a DateTimeOffset to a human-readable relative time string (e.g., "in 3 days", "2 months ago").
/// </summary>
public class RelativeDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is not DateTimeOffset dateTime)
            return null;

        var now = DateTimeOffset.UtcNow;
        var diff = dateTime - now;

        return diff.TotalSeconds switch
        {
            // Past
            < -86400 * 365 => $"{Math.Abs(diff.Days / 365)} year{(Math.Abs(diff.Days / 365) > 1 ? "s" : "")} ago",
            < -86400 * 30 => $"{Math.Abs(diff.Days / 30)} month{(Math.Abs(diff.Days / 30) > 1 ? "s" : "")} ago",
            < -86400 => $"{Math.Abs(diff.Days)} day{(Math.Abs(diff.Days) > 1 ? "s" : "")} ago",
            < -3600 => $"{Math.Abs((int)diff.TotalHours)} hour{(Math.Abs((int)diff.TotalHours) > 1 ? "s" : "")} ago",
            < 0 => $"{Math.Abs((int)diff.TotalMinutes)} minute{(Math.Abs((int)diff.TotalMinutes) > 1 ? "s" : "")} ago",

            // Future
            < 60 => "in a few seconds",
            < 3600 => $"in {(int)diff.TotalMinutes} minute{((int)diff.TotalMinutes > 1 ? "s" : "")}",
            < 86400 => $"in {(int)diff.TotalHours} hour{((int)diff.TotalHours > 1 ? "s" : "")}",
            < 86400 * 30 => $"in {diff.Days} day{(diff.Days > 1 ? "s" : "")}",
            < 86400 * 365 => $"in {diff.Days / 30} month{(diff.Days / 30 > 1 ? "s" : "")}",
            _ => $"in {diff.Days / 365} year{(diff.Days / 365 > 1 ? "s" : "")}",
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        return new BindingNotification(new NotSupportedException("Converting back from relative time is not supported."), BindingErrorType.Error);
    }
}
