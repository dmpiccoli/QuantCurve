using QuantCurve.Core;
using QuantCurve.Core.Calendar;
using QuantCurve.Core.Curve;
using QuantCurve.Core.Swap;

namespace QuantCurveTest;

class Program
{
    static void Main(string[] args)
    {
        var cbz = new Brazil();
        Dictionary<string, double> contracts = new Dictionary<string, double>();
        contracts.Add("F27", 96873.64);
        contracts.Add("F28", 85386.65);
        contracts.Add("F29", 75067.80);
        contracts.Add("F30", 65858.82);
        contracts.Add("F31", 57725.48);
        contracts.Add("F32", 50559.37);
        var pillars = cbz.Prepare(new DateTime(2026, 9, 30), contracts);
        var dfs = cbz.Create(pillars);
        Console.WriteLine("Test complete");
    }
}