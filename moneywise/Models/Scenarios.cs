namespace MoneyWise.Models;

public class Scenario
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public string Story { get; set; } = "";
    public string Emoji { get; set; } = "💡";
    public int MinAge { get; set; } = 10;
    public int Stage { get; set; } = 1;
    public string Category { get; set; } = "Planning";
    public bool IsLifeEvent { get; set; }
    public List<ScenarioOption> Options { get; set; } = new();
}

public class ScenarioOption
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }
    public Scenario? Scenario { get; set; }
    public string Label { get; set; } = "";
    public int BalanceChange { get; set; }
    public int SavingsChange { get; set; }
    public int EmergencyChange { get; set; }
    public int InvestmentChange { get; set; }
    public int DebtChange { get; set; }
    public int GoalChange { get; set; }
    public int Score { get; set; }
    public int Xp { get; set; }
    public string ImmediateResult { get; set; } = "";
    public string FutureEffect { get; set; } = "";
    public string WhyItMatters { get; set; } = "";
    public string Tone { get; set; } = "tradeoff";
}

public class SimulationResult
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public int ScenarioId { get; set; }
    public Scenario? Scenario { get; set; }
    public int ScenarioOptionId { get; set; }
    public int Score { get; set; }
    public int XpEarned { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}

public class LifeEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Story { get; set; } = "";
    public string Emoji { get; set; } = "⚡";
    public int MinAge { get; set; } = 10;
    public int ScenarioId { get; set; }
    public Scenario? Scenario { get; set; }
}
