namespace MoneyWise.Services;

public static class GameRules
{
    public static readonly string[] Stages =
    [
        "Pocket Money", "Smart Saver", "Budget Builder", "First Income",
        "Financial Planner", "Investment Explorer", "Money Master"
    ];

    public static int AgeFloor(string ageGroup) => ageGroup switch
    {
        "16-18" => 16,
        "13-15" => 13,
        _ => 10
    };

    public static int MaxStage(string ageGroup) => ageGroup switch
    {
        "16-18" => 7,
        "13-15" => 5,
        _ => 3
    };

    public static string LevelTitle(int xp)
    {
        var level = Level(xp);
        string[] names = ["Money Beginner", "Smart Saver", "Budget Builder", "Money Planner", "Investment Explorer", "Risk Manager", "Finance Master"];
        return names[Math.Clamp(level - 1, 0, names.Length - 1)];
    }

    public static int Level(int xp) => xp switch
    {
        < 80 => 1,
        < 180 => 2,
        < 320 => 3,
        < 500 => 4,
        < 720 => 5,
        < 980 => 6,
        _ => 7
    };

    public static (int Into, int Span) LevelProgress(int xp)
    {
        int[] starts = [0, 80, 180, 320, 500, 720, 980];
        var level = Level(xp);
        var start = starts[level - 1];
        var next = level == 7 ? start + 300 : starts[level];
        var span = next - start;
        var into = Math.Clamp(xp - start, 0, span);
        return (into, span);
    }

    public static string Inr(int amount)
    {
        var negative = amount < 0;
        var digits = Math.Abs(amount).ToString();
        if (digits.Length <= 3) return $"{(negative ? "-" : "")}₹{digits}";
        var last3 = digits[^3..];
        var rest = digits[..^3];
        var parts = new List<string>();
        while (rest.Length > 2)
        {
            parts.Insert(0, rest[^2..]);
            rest = rest[..^2];
        }
        if (rest.Length > 0) parts.Insert(0, rest);
        return $"{(negative ? "-" : "")}₹{string.Join(",", parts)},{last3}";
    }

    public static int ClampScore(int value) => Math.Clamp(value, 0, 100);

    public static double Compound(double principal, double annualPercent, int years)
    {
        if (years <= 0) return principal;
        var rate = annualPercent / 100d;
        return principal * Math.Pow(1 + rate, years);
    }
}
