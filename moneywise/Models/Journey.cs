namespace MoneyWise.Models;

public class MoneyJourney
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public int CurrentStage { get; set; } = 1;
    public int CurrentMonth { get; set; } = 1;
    public int? CurrentScenarioId { get; set; }
    public Scenario? CurrentScenario { get; set; }
    public int? PendingLifeEventId { get; set; }
    public LifeEvent? PendingLifeEvent { get; set; }
    public bool AwaitingMonthEnd { get; set; }
    public int VirtualBalance { get; set; }
    public int Savings { get; set; }
    public int EmergencyFund { get; set; }
    public int InvestmentValue { get; set; }
    public int Debt { get; set; }
    public int MonthlyObligation { get; set; }
    public int MonthlyIncome { get; set; }
    public string GoalName { get; set; } = "Bicycle";
    public int GoalAmount { get; set; } = 8000;
    public int GoalProgress { get; set; }
    public int FinancialScore { get; set; } = 40;
    public int EventsSurvived { get; set; }
    public bool PlayedBudget { get; set; }
    public bool PlayedPortfolio { get; set; }
    public bool PlayedNeeds { get; set; }
    public bool PlayedInvestSim { get; set; }
}

public class MoneyTransaction
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public int Amount { get; set; }
    public string Type { get; set; } = "Expense";
    public string Note { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
