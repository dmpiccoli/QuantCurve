using QuantCurve.Core.Calendar;

namespace QuantCurve.Core.Curve;

public class Brazil
{
    public SortedList<DateTime, Pillar> Prepare(DateTime curveDate, Dictionary<string, double> contracts)
    {
        SortedList<DateTime, Pillar> pillars = new SortedList<DateTime, Pillar>();
        var bz = CalendarFactory.Create(CalendarType.Brazil);
        
        foreach (var contract in contracts)
        {
            var pillar = new Pillar
            {
                Price = contract.Value,
                Maturity = new DateTime(2000 + int.Parse(contract.Key.AsSpan(1, 2)), (int)Enum.Parse<FutureCode>(contract.Key.AsSpan(0, 1)), 1)
            };
            pillar.Maturity = bz.AddBusinessDays(pillar.Maturity.AddDays(-1), 1);
            pillar.DayCount =  bz.CountBusinessDays(curveDate, pillar.Maturity);
            pillar.SpotRate = Math.Pow(100000 / pillar.Price, 252.0 / pillar.DayCount) - 1;
            
            pillars.Add(pillar.Maturity, pillar);
        }
        return pillars;
    }
    
    public Dictionary<int, double> Create(SortedList<DateTime, Pillar> pillars)
    {
        var pillar = pillars.Values.ElementAt(0);
        pillar.ForwardRate = pillar.SpotRate;
        
        const int horizon = 252 * 15;
        var dfs = new Dictionary<int, double>(horizon);
        var lastDf = 1.0;
        var dailyFactor = Math.Pow(1 + pillar.ForwardRate, 1.0 / 252.0);
        

        for (int i = 1; i <= horizon; i++)
        {
            if (i > pillar.DayCount)
            {
                if (pillars.IndexOfKey(pillar.Maturity) < pillars.Count - 1)
                {
                    var previous = pillar;
                    pillar = pillars.Values.ElementAt(pillars.IndexOfKey(pillar.Maturity) + 1);
                    pillar.ForwardRate = Math.Pow(previous.Price / pillar.Price, 252.0 / (pillar.DayCount - previous.DayCount)) - 1;
                    dailyFactor = Math.Pow(1 + pillar.ForwardRate, 1.0 / 252.0);
                }
                else
                    break;
            }
            lastDf /= dailyFactor;
            dfs.Add(i, lastDf);
        }

        return dfs;
    }
}