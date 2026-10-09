using QuantCurve.Core;
using QuantCurve.Core.Curve;
using QuantCurve.Core.Swap;

namespace QuantCurveTest;

class Program
{
    static void Main(string[] args)
    {
        // Bootstraper b = new Bootstraper();
        // DateTime date = DateTime.Today;
        //
        // Dictionary<string, double> tenors = new Dictionary<string, double>();
        // tenors.Add("1Y", 0.05);
        // tenors.Add("2Y", 0.06);
        // tenors.Add("3Y", 0.07);
        // tenors.Add("4Y", 0.08);
        // tenors.Add("5Y", 0.09);
        //
        // b.Prepare(date, tenors);
        
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