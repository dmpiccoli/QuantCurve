
namespace QuantCurve.Core.Calendar;

/// <summary>
/// Business-day calendar for Brazil.
///
/// The holiday set below is intentionally kept identical to the original
/// implementation: only the surrounding structure was refactored so the logic
/// can be tested and extended without Excel.
/// </summary>
public sealed class BrazilCalendar : ChristianCalendar, ICountryCalendar
{
    /// <summary>Excel date serial 0, used to detect an empty Excel date cell.</summary>
    public static readonly DateTime ExcelEpoch = new(1899, 12, 30);

    public bool IsBusinessDay(DateTime date)
    {
        return !IsHoliday(date);
    }

    public DateTime AddBusinessDays(DateTime date, int days)
    {
        if (days == 0)
            return date;

        int step = days < 0 ? -1 : 1;
        while (days != 0)
        {
            date = date.AddDays(step);
            if (IsBusinessDay(date))
                days -= step;
        }
        return date;
    }

    public int CountBusinessDays(DateTime start, DateTime end)
    {
        if (start == end)
            return 0;

        int step = start > end ? -1 : 1;
        DateTime lo = start < end ? start : end;
        DateTime hi = start > end ? start : end;
        int numDays = 0;

        while (lo != hi)
        {
            lo = lo.AddDays(1);
            if (IsBusinessDay(lo))
                numDays += 1;
        }

        return step > 0 ? numDays : -numDays;
    }

    private bool IsHoliday(DateTime date)
    {
        DayOfWeek w = date.DayOfWeek;
        int d = date.Day;
        int m = date.Month;
        int y = date.Year;
        int dd = date.DayOfYear;
        int emDoy = EasterMonday(y);

        return w == DayOfWeek.Saturday
            || w == DayOfWeek.Sunday
            // New Year's Day
            || (d == 1 && m == 1)
            // Tiradentes Day
            || (d == 21 && m == 4)
            // Labor Day
            || (d == 1 && m == 5)
            // Independence Day
            || (d == 7 && m == 9)
            // Nossa Sra. Aparecida Day
            || (d == 12 && m == 10)
            // All Souls Day
            || (d == 2 && m == 11)
            // Republic Day
            || (d == 15 && m == 11)
            // Black Consciousness Day
            || (d == 20 && m == 11 && y >= 2024) //declared on 21/12/2023
            // Christmas
            || (d == 25 && m == 12)
            // Passion of Christ
            || dd == emDoy - 3
            // Carnival
            || (dd == emDoy - 49 || dd == emDoy - 48)
            // Corpus Christi
            || dd == emDoy + 59;
    }
}
