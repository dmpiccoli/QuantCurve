namespace QuantCurve.Core.Calendar;

/// <summary>
/// Supported business-day calendars. Only <see cref="CalendarType.Brazil"/> is
/// implemented for now; new countries are added by creating a new type that
/// implements <see cref="ICountryCalendar"/> and registering it in
/// <see cref="CalendarFactory"/>.
/// </summary>
public enum CalendarType
{
    Brazil,

    /// <summary>Planned. Chile calendar.</summary>
    Chile,

    /// <summary>Planned. Mexico calendar.</summary>
    Mexico,
}
