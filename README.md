# QuantCurve

A small, simple .NET 10 library and Excel add-in for quantitative finance,
focused on **Latin American markets** (Brazil, Chile, Mexico).

It started as a weekend project because most libraries out there are heavy,
abstracted away, or simply don't cover these markets. The design is
**inspired by QuantLib** — a well-known, heavily-abstracted C++ reference
implementation — but stripped down: no heavy abstractions, no ceremony, just
the essentials. The goal is to ship: a business-day **calendar** engine today,
with a **curve builder** and a **swap pricer** coming next.

Everything is designed to be easy to read and easy to extend.

---

## Why this exists

- Hard to find simple, easy-to-use implementations online.
- Most finance libraries are huge and built for US/EU markets.
- Latin American derivatives markets (Brazil, Chile, Mexico) deserve first-class
  support, not afterthoughts.

---

## Project structure

```
QuantCurve/
├── QuantCurve.slnx
└── QuantCurve/
    ├── QuantCurve.csproj          # .NET 10 solution project
    ├── Core/                      # Core, testable logic (no Excel dependency)
    │   └── Calendar/              # Business-day calendars
    │       ├── ICountryCalendar.cs
    │       ├── CalendarType.cs
    │       ├── CalendarFactory.cs
    │       ├── ChristianCalendar.cs
    │       └── BrazilCalendar.cs
    └── Excel/                     # Thin Excel add-in wrappers
        └── ExcelCalendar.cs
```

The design follows a simple rule: **Core logic lives in `QuantCurve.Core` and is
free of any Excel dependency**, so it can be unit-tested and reused. The `Excel`
layer is intentionally thin — it only translates Excel arguments and errors.

---

## What's available now

### Calendars

A business-day calendar engine. Each calendar answers three questions:

| Method | Description |
| --- | --- |
| `IsBusinessDay(date)` | Is a given date a business day? |
| `AddBusinessDays(date, days)` | Return the date `days` business days from `date` (supports negative `days`). |
| `CountBusinessDays(start, end)` | Count business days strictly between two dates (sign follows direction). |

**Implemented calendars:**

| Calendar | Status |
| --- | --- |
| Brazil | Implemented (holidays + Christian-based floating holidays). |
| Chile | Planned. |
| Mexico | Planned. |

New countries are added by:

1. Creating a new type that implements `ICountryCalendar`.
2. Registering an instance in `CalendarFactory`.
3. Adding the value to `CalendarType`.

---

## Planned features

The project is a living side project. The next milestones are:

1. **Curve builder** — build/interpolate interest-rate term structures for
   **Brazil, Chile and Mexico**.
2. **Swap pricer** — price interest-rate swaps for **Chile and Mexico**,
   reusing the same calendar engine.

---

## Using it in Excel

The project builds a 32- and 64-bit Excel add-in. Load the compiled add-in in
Excel, then use the exposed functions:

| Function | Arguments |
| --- | --- |
| `QCWorkday` | Start date, number of business days, calendar selector. |
| `QCNetworkdays` | Start date, end date, calendar selector. |

Calendar selector:

| Value | Calendar |
| --- | --- |
| `0` | Brazil |
| *(Chile / Mexico)* | Planned. |

> Example: `=QCWorkday(A1, 5, 0)` returns the date 5 business days after the
> date in `A1`, using the Brazil calendar.

---

## Building

Requires the .NET 10 SDK.

```bash
dotnet build QuantCurve/QuantCurve.csproj
```

---

## Conventions

- Core logic is kept free of ExcelDna so it stays unit-testable.
- New markets follow the `ICountryCalendar` + `CalendarFactory` pattern.
- Prefer clear, maintainable code over clever implementations.
