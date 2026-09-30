namespace QuantCurve.Core.Curve;

public class CurvePillar
{
    public string Tenor { get; set; }
    public DateTime Maturity { get; set; }
    public int DayCount { get; set; }
    public double SpotRate { get; set; }
    public double ForwardRate { get; set; }
    public double Price { get; set; }
    
    public CurvePillar? Before { get; set; }
    public CurvePillar? Next { get; set; }
}