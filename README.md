# QuantCurve

A small, simple .NET 10 library and Excel add-in for quantitative finance,
focused on **Latin American markets** (Brazil, Chile, Mexico).

It started as a weekend project because most libraries out there are heavy,
abstracted away, or simply don't cover these markets. The design is
**inspired by QuantLib** — a well-known, heavily-abstracted C++ reference
implementation — but stripped down: no heavy abstractions, no ceremony, just
the essentials. The library provides: a business-day **calendar** engine,
a **Brazil DI curve builder**, and a **par swap curve bootstrapper**,
with extended multi-currency curves and swap pricing coming next.

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
├── QuantCurve/
│   ├── QuantCurve.csproj          # .NET 10 library & Excel add-in project
│   ├── Core/                      # Core, testable logic (no Excel dependency)
│   │   ├── Calendar/              # Business-day calendars
│   │   │   ├── ICountryCalendar.cs
│   │   │   ├── CalendarType.cs
│   │   │   ├── CalendarFactory.cs
│   │   │   ├── ChristianCalendar.cs
│   │   │   └── BrazilCalendar.cs
│   │   ├── Curve/                 # Term structure and curve construction
│   │   │   ├── Brazil.cs
│   │   │   ├── Pillar.cs
│   │   │   └── Bootstraper.cs
│   │   ├── DayCount/              # Day count conventions
│   │   │   └── Act360DayCount.cs
│   │   └── Swap/                  # Swap bootstrapping and conventions
│   │       ├── BusinessDayConvention.cs
│   │       └── SwapBootstrapper.cs
│   └── Excel/                     # Thin Excel add-in wrappers
│       ├── ExcelCalendar.cs
│       └── Brazil.cs
├── QuantCurveTest/                # Integration tests and DI curve validation
│   ├── Program.cs
│   ├── QC Test.xlsx               # Sample workbook and validation dataset
│   └── QuantCurveTest.csproj
└── TempVerifyApp/                 # Swap bootstrapper theoretical verification
    ├── Program.cs
    └── TempVerifyApp.csproj
```

The design follows a simple rule: **Core logic lives in `QuantCurve.Core` and is
free of any Excel dependency**, so it can be unit-tested and reused. The `Excel`
layer is intentionally thin — it only translates Excel arguments and errors.

---

## Component status

| Module | Component | Status | Description |
| --- | --- | --- | --- |
| **Calendar** | Brazil | Implemented | Fixed holidays + Easter-derived floating holidays (Carnival, Good Friday, Corpus Christi). |
| **Calendar** | Chile & Mexico | Planned | Country holiday schedules. |
| **Curve** | Brazil DI Curve | Implemented | B3 DI futures (`Brazil`), `FutureCode` parsing, 252 business days compounding, sorted pillar schedule up to 15 years. |
| **Excel Add-in** | Calendar UDFs | Implemented | `QCWorkday`, `QCNetworkdays` (Category: QuantCurve). |
| **Excel Add-in** | Brazil Curve UDF | Implemented | `QCBrazilFixedCurve` (Category: QuantCurve, handles 2D ranges, strips DI1 prefixes). |
| **Excel Add-in** | Swap UDFs | Planned | Excel UDF wrappers for swap bootstrapping and valuation. |

---

## Modules & Usage

### 1. Business Day Calendars (`QuantCurve.Core.Calendar`)

The calendar module provides business day calculations, holiday adjustments, and network day counting for Latin American financial markets.

Each calendar implements `ICountryCalendar`:

| Method | Description |
| --- | --- |
| `IsBusinessDay(date)` | Returns `true` if `date` is a valid business day (not a weekend or holiday). |
| `AddBusinessDays(date, days)` | Adds (or subtracts) `days` business days to/from `date`. |
| `CountBusinessDays(start, end)` | Counts business days strictly between `start` and `end` (sign follows chronological order). |

#### Holiday Logic
- **Fixed Holidays**: New Year's Day, Tiradentes, Labor Day, Independence Day, Our Lady of Aparecida, All Souls, Republic Proclamation, Black Awareness Day, Christmas.
- **Floating Christian Holidays**: Handled dynamically using `ChristianCalendar.EasterMonday(year)` (a protected helper inherited by country calendar implementations such as `BrazilCalendar`) using the Meeus/Jones/Butcher algorithm to compute Carnival (Monday & Tuesday, $-48$ and $-47$ days), Good Friday ($-2$ days), and Corpus Christi ($+60$ days).

#### Example Usage
```csharp
using QuantCurve.Core.Calendar;

var calendar = CalendarFactory.Create(CalendarType.Brazil);

DateTime date = new DateTime(2026, 9, 30);
bool isBusDay = calendar.IsBusinessDay(date); // true
DateTime nextBusDay = calendar.AddBusinessDays(date, 5);
int count = calendar.CountBusinessDays(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
```

---

### 2. Brazil DI Curve Engine (`QuantCurve.Core.Curve`)

The DI curve builder (`Brazil`) models the Brazilian interbank deposit rate term structure using standard B3 DI futures contracts (e.g. `F27`, `F28`, ..., `F32` corresponding to January maturities).

#### Month Codes (`FutureCode`)
Maturity month codes follow the standard B3 futures conventions mapped via the `FutureCode` enum:

| Code | Month | Code | Month | Code | Month | Code | Month |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `F` | January (1) | `J` | April (4) | `N` | July (7) | `V` | October (10) |
| `G` | February (2) | `K` | May (5) | `Q` | August (8) | `X` | November (11) |
| `H` | March (3) | `M` | June (6) | `U` | September (9) | `Z` | December (12) |

#### Mathematics & Compounding
Brazilian fixed-income conventions operate on **252 business days per year** with discrete compounding:
- **Spot Rate**:
  $$\text{SpotRate} = \left(\frac{100{,}000}{\text{Price}}\right)^{\frac{252}{\text{DayCount}}} - 1$$
- **Forward Rate** between contract nodes $i-1$ and $i$:
  $$\text{ForwardRate} = \left(\frac{\text{Price}_{i-1}}{\text{Price}_i}\right)^{\frac{252}{\text{DayCount}_i - \text{DayCount}_{i-1}}} - 1$$
- **Discount Factor Schedule**:
  Computed daily over a 15-year horizon ($252 \times 15 = 3{,}780$ business days) by compounding forward rates:
  $$\text{DF}_t = \frac{\text{DF}_{t-1}}{(1 + \text{ForwardRate})^{1/252}}$$

#### Sorted Pillar Architecture
Contract nodes are represented by `Pillar` models containing maturity, business day count, spot rate, forward rate, and price. 

`Brazil.Prepare` processes input contract dictionaries provided in any arbitrary order and returns a `SortedList<DateTime, Pillar>` chronologically sorted by maturity. `Brazil.Create` then iterates through the sorted pillars to compute forward rates between consecutive nodes and generates the complete daily discount factor schedule.

### 3. Swap Bootstrapping & Day Count (`QuantCurve.Core.Swap` & `QuantCurve.Core.DayCount`)

#### Swap Curve Bootstrapper (`SwapBootstrapper`)
Bootstraps a discount curve from a sequence of par swap rates (ordered by increasing maturity):
- **Spot Offset**: Spot date = $\text{CurveDate} + 2\text{ calendar days}$.
- **Coupon Schedule**: Semiannual fixed payments ($k \times 180$ days from spot).
- **Date Adjustment**: Modified Following / Backward (`BusinessDayConvention.Adjust`) using the specified calendar.
- **Interpolation**: Log-linear interpolation on discount factors between bootstrapped tenors.
- **Bootstrap Formula**:
  $$\text{DF}_n = \frac{1 - S \sum_{j=1}^{n-1} \tau_j \text{DF}_j}{1 + S \tau_n}$$
  where $S$ is the par swap rate and $\tau$ is the accrual fraction.

#### Day Count Convention (`Act360DayCount`)
Computes accrual fractions using the ACT/360 convention:
$$\text{Fraction}(\text{start}, \text{end}) = \frac{\text{Days}(\text{end} - \text{start})}{360.0}$$

#### Business Day Convention (`BusinessDayConvention`)
Implements Modified Following / Backward (`mFb`):
1. Returns the date if it is already a business day.
2. Rolls forward to the next business day.
3. If a holiday was crossed such that the day before the rolled date is a non-business day, rolls backward.

## Testing & Verification

The solution includes dedicated validation and verification applications:

### 1. `QuantCurveTest`
An end-to-end integration harness testing the Brazil DI curve engine with real contract data.
- Includes `QC Test.xlsx` containing sample B3 DI contract data, calculated rates, and reference curve validation.
```bash
dotnet run --project QuantCurveTest/QuantCurveTest.csproj
```

### 2. `TempVerifyApp`
A verification suite that tests `SwapBootstrapper` against closed-form theoretical discount factors:
- **Flat term structure**: Validates bootstrapped discount factors against $\text{DF}(T) = e^{-r T}$ with machine precision ($< 10^{-12}$).
- **Piecewise-flat term structure**: Validates multi-step discount factor curves against exact analytical solutions.
```bash
dotnet run --project TempVerifyApp/TempVerifyApp.csproj
```

---

## Using it in Excel

The project builds a 32- and 64-bit Excel add-in via Excel-DNA. Load the compiled XLL add-in in Excel to use the custom worksheet functions (registered under category `"QuantCurve"`):

| Function | Arguments | Description |
| --- | --- | --- |
| `QCWorkday` | `date`, `days`, `calendar` | Returns the date offset by `days` business days. |
| `QCNetworkdays` | `start_date`, `end_date`, `calendar` | Counts business days between two dates. |
| `QCBrazilFixedCurve` | `date`, `contracts` | Builds the Brazil DI curve discount factors. Accepts contract codes with or without `DI1` prefix (e.g. `DI1F27` or `F27`). Handles 2-column ranges with or without header rows and returns a dynamic 2D array of `[DayIndex, DiscountFactor]`. |

#### Calendar Selector
| Value | Calendar |
| --- | --- |
| `0` | Brazil |
| *(Chile / Mexico)* | Planned |

#### Examples
- **Business Day Calculation**:
  `=QCWorkday(A1, 5, 0)` returns the date 5 business days after `A1` using the Brazil calendar.
- **Brazil Fixed DI Curve**:
  `=QCBrazilFixedCurve(A1, B2:C10)` where `A1` is the curve date and `B2:C10` contains the contract codes and settlement prices.

---

## Planned Features & Roadmap

1. **Country Calendars**: Add Chile and Mexico holiday schedules.
2. **Curve Engines**: Implement Chile ICP and Mexico TIIE term structure construction.
3. **Swap Pricing Engine**: Implement pricing and risk analytics for cross-currency and interest rate swaps.
4. **Excel Add-in Expansion**: Expose swap bootstrapping and valuation functions directly as Excel UDFs.

---

## Building

Requires the .NET 10 SDK.

```bash
# Build entire solution
dotnet build QuantCurve.slnx

# Or build the library project directly
dotnet build QuantCurve/QuantCurve.csproj
```

---

## Conventions

- **Clean Core**: Core logic lives in `QuantCurve.Core` free of ExcelDna or external UI dependencies for full unit-testability.
- **Extensibility**: Calendars implement `ICountryCalendar` and register with `CalendarFactory`.
- **Clarity over Cleverness**: Prefer explicit, transparent financial math implementations.
