namespace QuantCurve.Core.Curve;

public enum FutureCode
{
    F = 1,
    G = 2,
    H = 3,
    J = 4,
    K = 5,
    M = 6,
    N = 7,
    Q = 8,
    U = 9,
    V = 10,
    X = 11,
    Z = 12,
}

public class Pillar
{
    public string Tenor { get; set; }
    public DateTime Maturity { get; set; }
    public int DayCount { get; set; }
    public double SpotRate { get; set; }
    public double ForwardRate { get; set; }
    public double Price { get; set; }
}