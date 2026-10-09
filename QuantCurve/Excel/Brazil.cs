using ExcelDna.Integration;
using System.Globalization;

using QuantCurve.Core.Calendar;
using QuantCurve.Core.Curve;

namespace QuantCurve.Excel;

/// <summary>
/// This layer intentionally stays thin: it only translates Excel arguments and
/// errors. All business logic lives in <c>QuantCurve.Core</c> so it stays
/// unit-testable and reusable outside of Excel.
/// </summary>
public static class Brazil
{
    /// <summary>Excel date serial 0 (empty date cell).</summary>
    public static readonly DateTime ExcelEpoch = new(1899, 12, 30);

    [ExcelFunction(Description = "Brazil curve as known as pré", Category = "QuantCurve")]
    public static object QCBrazilFixedCurve([ExcelArgument("Curve date")] DateTime date,
        [ExcelArgument("Contract information (contract code and price in BRL)")] object[,] contracts)
    {
        if (date == ExcelEpoch)
            return ExcelError.ExcelErrorValue;

        try
        {
            if (contracts.GetLength(1) != 2 || contracts.GetLength(0) == 0)
                return ExcelError.ExcelErrorValue;

            var contractPrices = new Dictionary<string, double>();
            int rowStart = 0;
            // if second column is not a double than the range has headers
            if (contracts[0, 1] is not double)
                rowStart = 1;

            for (int i = rowStart; i <= contracts.GetUpperBound(0); i++)
            {
                if (string.IsNullOrEmpty(contracts[i, 0].ToString()))
                    return ExcelError.ExcelErrorValue;
                else
                    contractPrices.Add(contracts[i, 0].ToString().Replace("DI1", ""), (double)contracts[i, 1]);
                
            }

            if (contractPrices.Count == 0)
                return ExcelError.ExcelErrorValue;

            var curve = new Core.Curve.Brazil();
            var pillars = curve.Prepare(date, contractPrices);
            var discountFactors = curve.Create(pillars);
            var result = new object[discountFactors.Count, 2];
            int resultRow = 0;

            foreach (var discountFactor in discountFactors)
            {
                result[resultRow, 0] = discountFactor.Key;
                result[resultRow, 1] = discountFactor.Value;
                resultRow++;
            }

            return result;
        }
        catch (Exception ex)
        {
            return ExcelError.ExcelErrorValue;
        }
    }
}
