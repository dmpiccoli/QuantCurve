namespace QuantCurve.Core.Calendar;

/// <summary>
/// Abstraction over a country's business-day calendar.
///
/// This is deliberately free of any ExcelDna dependency so that calendars
/// (Brazil, Chile, Mexico, ...) can be unit-tested in isolation and reused
/// both by the Excel add-in and by future swap-calculation code.
/// </summary>
public interface ICountryCalendar
{
    /// <summary>
    /// Returns <c>true</c> if <paramref name="date"/> is a business day.
    /// </summary>
    bool IsBusinessDay(DateTime date);

    /// <summary>
    /// Returns the date that is <paramref name="days"/> business days after
    /// <paramref name="date"/>. Positive <paramref name="days"/> move forward,
    /// negative ones move backward. A value of 0 returns <paramref name="date"/>.
    /// </summary>
    DateTime AddBusinessDays(DateTime date, int days);

    /// <summary>
    /// Counts the business days strictly between <paramref name="start"/> and
    /// <paramref name="end"/> (exclusive on both ends). The sign matches the
    /// direction of the range: positive when <paramref name="start"/> is before
    /// <paramref name="end"/>, negative otherwise. Equal dates return 0.
    /// </summary>
    int CountBusinessDays(DateTime start, DateTime end);
}
