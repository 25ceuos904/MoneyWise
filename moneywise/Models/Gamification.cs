namespace MoneyWise.Models;

public class UserProgress
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public int Xp { get; set; }
    public int Streak { get; set; } = 1;
    public DateTime LastActiveDate { get; set; } = DateTime.Today;
    public int SavingScore { get; set; } = 20;
    public int BudgetingScore { get; set; } = 20;
    public int SpendingScore { get; set; } = 20;
    public int InvestingScore { get; set; } = 20;
    public int RiskScore { get; set; } = 20;
    public int PlanningScore { get; set; } = 20;

    public int Overall => (SavingScore + BudgetingScore + SpendingScore + InvestingScore + RiskScore + PlanningScore) / 6;
}

public class Challenge
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string RuleKey { get; set; } = "";
    public int Xp { get; set; } = 20;
    public int MinAge { get; set; } = 10;
}

public class UserChallenge
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public int ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public class Badge
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "🏅";
}

public class UserBadge
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public int BadgeId { get; set; }
    public Badge? Badge { get; set; }
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}

public class InvestmentConcept
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Situation { get; set; } = "";
    public string Explanation { get; set; } = "";
    public string RiskLevel { get; set; } = "";
    public string ReturnCategory { get; set; } = "";
    public int MinAge { get; set; } = 13;
}
