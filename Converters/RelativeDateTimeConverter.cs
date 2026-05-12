using System.Globalization;
using System.Numerics;
using Avalonia.Data;
using Avalonia.Data.Converters;
using KeyVaultHelper.Extensions;

namespace KeyVaultHelper.Converters;

/// <summary>
/// Converts a DateTimeOffset to a human-readable relative time string (e.g., "in 3 days", "2 months ago").
/// </summary>
public class RelativeDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is null)
        {
            return null;
        }

        if (value is not DateTimeOffset dateTime)
        {
            return new BindingNotification(new NotSupportedException($"Only {typeof(DateTimeOffset)} is supported, but {value.GetType()} was given"), BindingErrorType.Error);
        }

        var now = DateTimeOffset.UtcNow;
        var diff = dateTime - now;
        var isPast = diff < TimeSpan.Zero;

        // See if we're on month-scale or greater. Using GetTotalMonthsFrom is slower, but yields
        // a far more accurate result, than naively scaling e.g. TimeSpan#TotalSeconds.
        var monthsDifference = dateTime.UtcDateTime.GetTotalMonthsFrom(now.UtcDateTime);
        if (monthsDifference >= 12)
        {
            var yearsDifference = monthsDifference / 12;
            return isPast
                ? $"{yearsDifference} {Pluralize(yearsDifference, "year", "years")} ago"
                : $"in {yearsDifference} {Pluralize(yearsDifference, "year", "years")}";
        }
        else if (monthsDifference > 0 && Math.Abs(diff.TotalDays) > 15) // only trigger month rounding if its actually been at least half a month
        {
            return isPast
                ? $"{monthsDifference} {Pluralize(monthsDifference, "month", "months")} ago"
                : $"in {monthsDifference} {Pluralize(monthsDifference, "month", "months")}";
        }

        // Fall back to scaling based on seconds difference. It is fast and accurate enough for our
        // purposes. I don't think users will notice a leap second missing, and due to the above
        // greater-than-month difference handling, the timescale on which such an issue can happen
        // is limited to less than a month - so the error cannot compound to a significant difference.
        // All constants here are in seconds, obviously.
        const int ONE_MINUTE = 60;
        const int ONE_HOUR = ONE_MINUTE * 60;
        const int ONE_DAY = ONE_HOUR * 24;

        // Per isPast above, negative numbers of seconds indicate the past
        return diff.TotalSeconds switch
        {
            <= -ONE_DAY => $"{Math.Abs(diff.Days)} {Pluralize(diff.Days, "day", "days")} ago",
            <= -ONE_HOUR => $"{Math.Abs(diff.Hours)} {Pluralize(diff.Hours, "hour", "hours")} ago",
            <= -ONE_MINUTE => $"{Math.Abs(diff.Minutes)} {Pluralize(diff.Minutes, "minute", "minutes")} ago",
            < 0 => "a few seconds ago",
            0 => "now",
            <= ONE_MINUTE => "in a few seconds",
            <= ONE_HOUR => $"in {Math.Abs(diff.Minutes)} {Pluralize(diff.Minutes, "minute", "minutes")}",
            <= ONE_DAY => $"in {Math.Abs(diff.Hours)} {Pluralize(diff.Hours, "hour", "hours")}",
            _ => $"in {Math.Abs(diff.Days)} {Pluralize(diff.Days, "day", "days")}"
        };
    }

    private static string Pluralize<T>(T quantity, string singular, string plural) where T : IBinaryInteger<T>
        => T.Abs(quantity) == T.One ? singular : plural;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        return new BindingNotification(new NotSupportedException("Converting back from relative time is not supported."), BindingErrorType.Error);
    }
}
