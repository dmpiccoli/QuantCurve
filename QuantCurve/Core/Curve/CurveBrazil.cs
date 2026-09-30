using QuantCurve.Core.Calendar;

namespace QuantCurve.Core.Curve;

public class CurveBrazil
{
    public Dictionary<DateTime, CurvePillar> Prepare(DateTime curveDate, Dictionary<string, double> contracts)
    {
        Dictionary<DateTime, CurvePillar> pillars = new Dictionary<DateTime, CurvePillar>();
        var bz = CalendarFactory.Create(CalendarType.Brazil);
        CurvePillar? previous = null;
        foreach (var contract in contracts)
        {
            var pillar = new CurvePillar
            {
                Price = contract.Value,
                Maturity = new DateTime(2000 + int.Parse(contract.Key.AsSpan(1, 2)), 1, 1)
            };
            pillar.Maturity = bz.AddBusinessDays(pillar.Maturity.AddDays(-1), 1);
            pillar.DayCount =  bz.CountBusinessDays(curveDate, pillar.Maturity);
            pillar.SpotRate = Math.Pow(100000 / pillar.Price, 252.0 / pillar.DayCount) - 1;

            if (previous is null)
                pillar.ForwardRate = pillar.SpotRate;
            else
                pillar.ForwardRate = Math.Pow(previous.Price / pillar.Price, 252.0 / (pillar.DayCount - previous.DayCount)) - 1;
            pillar.Before = previous;
            if (previous is not null)
                previous.Next = pillar;
            pillars.Add(pillar.Maturity, pillar);
            previous = pillar;
        }
        return pillars;
    }
    
    public Dictionary<int, double> Create(Dictionary<DateTime, CurvePillar> pillars)
    {
        var pillar = pillars.Values.ElementAt(0);
        const int horizon = 252 * 15;
        var dfs = new Dictionary<int, double>(horizon);
        var lastDf = 1.0;
        var dailyFactor = Math.Pow(1 + pillar.ForwardRate, 1.0 / 252.0);

        for (int i = 1; i <= horizon; i++)
        {
            if (i > pillar.DayCount)
            {
                if (pillar.Next is not null)
                {
                    pillar = pillar.Next;
                    dailyFactor = Math.Pow(1 + pillar.ForwardRate, 1.0 / 252.0);
                }
                else
                    dailyFactor = 1.0;
            }

            lastDf /= dailyFactor;
            dfs.Add(i, lastDf);
        }

        return dfs;
    }
}