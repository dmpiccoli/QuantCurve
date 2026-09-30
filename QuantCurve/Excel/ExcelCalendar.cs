using ExcelDna.Integration;

using QuantCurve.Core.Calendar;

namespace QuantCurve.Excel;

/// <summary>
/// Excel-facing wrappers around <see cref="QuantCurve.Core.Calendar.ICountryCalendar"/>.
///
/// This layer intentionally stays thin: it only translates Excel arguments and
/// errors. All business logic lives in <c>QuantCurve.Core</c> so it stays
/// unit-testable and reusable outside of Excel.
/// </summary>
public static class ExcelCalendar
{
    /// <summary>Excel date serial 0 (empty date cell).</summary>
    public static readonly DateTime ExcelEpoch = new(1899, 12, 30);

    [ExcelFunction(Description = "Workday with QuantCurve calendar")]
    public static object QCWorkday([ExcelArgument("Start date")] DateTime date,
        [ExcelArgument("Number of business days")] int days,
        [ExcelArgument("0 -> Brazil")] int calendar)
    {
        if (date == ExcelEpoch)
            return ExcelError.ExcelErrorValue;

        try
        {
            if (days == 0)
                return date;

            ICountryCalendar calendarInstance = CalendarFactory.Create((CalendarType)calendar);
            return calendarInstance.AddBusinessDays(date, days);
        }
        catch (Exception ex)
        {
            return ExcelError.ExcelErrorValue;
        }
    }

    [ExcelFunction(Description = "Networkdays with QuantCurve calendar")]
    public static object QCNetworkdays([ExcelArgument("Start date")] DateTime start_date,
        [ExcelArgument("End date")] DateTime end_date,
        [ExcelArgument("0 -> Brazil")] int calendar)
    {
        if (start_date == ExcelEpoch || end_date == ExcelEpoch)
            return ExcelError.ExcelErrorValue;

        try
        {
            ICountryCalendar calendarInstance = CalendarFactory.Create((CalendarType)calendar);
            return calendarInstance.CountBusinessDays(start_date, end_date);
        }
        catch (Exception ex)
        {
            return ExcelError.ExcelErrorValue;
        }
    }
}
