using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace KeyVaultHelper.Converters;

/// <summary>
/// Converts a DateTimeOffset to an absolute date/time string with local timezone and locale formatting.
/// </summary>
public class AbsoluteDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is not DateTimeOffset dateTime)
            return null;

        // Convert to local timezone
        var localTime = dateTime.ToLocalTime();

        // Use the current culture or provided culture
        var cultureToUse = culture ?? CultureInfo.CurrentCulture;

        return localTime.ToString("G", cultureToUse);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        return new BindingNotification(new NotSupportedException("Converting back from absolute date/time is not supported."), BindingErrorType.Error);
    }
}
