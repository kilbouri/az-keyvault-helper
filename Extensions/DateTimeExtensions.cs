namespace KeyVaultHelper.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Returns the absolute number of months difference between <paramref name="dt1"/> and
    /// <paramref name="dt2"/>. Last day of month N and first day of month N+1 are considered 1
    /// month apart.
    /// </summary>
    /// <param name="dt1">The first date</param>
    /// <param name="dt2">The second date</param>
    /// <returns>The absolute number of months difference between the two dates.</returns>
    public static int GetTotalMonthsFrom(this DateTime dt1, DateTime dt2)
    {
        DateTime earlyDate = (dt1 > dt2) ? dt2.Date : dt1.Date;
        DateTime lateDate = (dt1 > dt2) ? dt1.Date : dt2.Date;

        // Start with 1 month's difference and keep incrementing
        // until we overshoot the late date
        int monthsDiff = 1;
        while (earlyDate.AddMonths(monthsDiff) <= lateDate)
        {
            monthsDiff++;
        }

        return monthsDiff - 1; // -1 because we overshot with the last increment
    }
}
