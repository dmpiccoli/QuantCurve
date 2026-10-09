namespace QuantCurve.Core.Calendar;

/// <summary>
/// Shared calendar helper for holidays defined relative to Easter Monday.
///
/// Holds the <see cref="EasterMonday"/> day-of-year lookup that is common to
/// several Christian-based calendars (Brazil, ...) so it can be inherited
/// rather than duplicated per calendar.
/// </summary>
public abstract class ChristianCalendar
{
    /// <summary>
    /// Returns the day-of-year of Easter Monday for the given year, using a
    /// precomputed table spanning 1901-2199.
    /// </summary>
    protected static int EasterMonday(int year)
    {
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day   = (h + l - 7 * m + 114) % 31 + 1;
        return new DateTime(year, month, day).DayOfYear;
    }
    
}