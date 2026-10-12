namespace QuantCurve.Core.Calendar;

/// <summary>
/// Maps a <see cref="CalendarType"/> to its <see cref="ICountryCalendar"/>
/// implementation. New countries are registered here.
/// </summary>
public static class CalendarFactory
{
    private static readonly DateTime BrazilCalendarChangeDate = new(2023, 12, 21);

    private static readonly Dictionary<CalendarType, ICountryCalendar> Calendars = new()
    {
        [CalendarType.Brazil] = new BrazilCalendar()
    };

    public static ICountryCalendar Create(CalendarType calendar, DateTime? calendarDate = null)
    {
        
        
        if (!Calendars.TryGetValue(calendar, out ICountryCalendar? calendarInstance))
            throw new ArgumentException($"Unsupported calendar '{calendar}'. Supported calendars: " + string.Join(", ", Calendars.Keys), nameof(calendar));

        if (calendar == CalendarType.Brazil && calendarDate < BrazilCalendarChangeDate)
            return new BrazilCalendarWithoutBlackConsciousnessDay();

        return calendarInstance;
    }
}