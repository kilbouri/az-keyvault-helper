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
        if (value is null)
        {
            return null;
        }

        if (value is not DateTimeOffset dateTime)
        {
            return new BindingNotification(new NotSupportedException($"Only {typeof(DateTimeOffset)} is supported"), BindingErrorType.Error);
        }

        return dateTime.ToLocalTime().ToString("G", culture ?? CultureInfo.CurrentCulture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        return new BindingNotification(new NotSupportedException("Converting back from absolute date/time is not supported."), BindingErrorType.Error);
    }
}
