namespace QuantCurve.Core.Calendar;

/// <summary>
/// Maps a <see cref="CalendarType"/> to its <see cref="ICountryCalendar"/>
/// implementation. New countries are registered here.
/// </summary>
public static class CalendarFactory
{
    private static readonly Dictionary<CalendarType, ICountryCalendar> Calendars = new()
    {
        [CalendarType.Brazil] = new BrazilCalendar()
    };

    public static ICountryCalendar Create(CalendarType calendar)
    {
        if (!Calendars.TryGetValue(calendar, out ICountryCalendar? calendarInstance))
            throw new ArgumentException(
                $"Unsupported calendar '{calendar}'. Supported calendars: " +
                string.Join(", ", Calendars.Keys),
                nameof(calendar));

        return calendarInstance;
    }
}
