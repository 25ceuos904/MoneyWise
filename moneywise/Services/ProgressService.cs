using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Models;

namespace MoneyWise.Services;

public class ProgressService
{
    private readonly AppDbContext _db;
    private readonly BadgeService _badges;

    public ProgressService(AppDbContext db, BadgeService badges)
    {
        _db = db;
        _badges = badges;
    }

    public async Task TouchStreakAsync(ApplicationUser user)
    {
        var progress = await _db.UserProgresses.FirstAsync(p => p.UserId == user.Id);
        var today = DateTime.Today;
        if (progress.LastActiveDate.Date == today) return;
        progress.Streak = progress.LastActiveDate.Date == today.AddDays(-1) ? progress.Streak + 1 : 1;
        progress.LastActiveDate = today;
        await _db.SaveChangesAsync();
        await _badges.EvaluateAsync(user.Id);
    }

    public async Task<int> AddXpAsync(string userId, int xp, string category, int score)
    {
        var progress = await _db.UserProgresses.FirstAsync(p => p.UserId == userId);
        if (xp > 0) progress.Xp += xp;
        Nudge(progress, category, score);
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == userId);
        journey.FinancialScore = progress.Overall;
        await _db.SaveChangesAsync();
        await CheckChallengesAsync(userId);
        await _badges.EvaluateAsync(userId);
        return xp;
    }

    public async Task CheckChallengesAsync(string userId)
    {
        var progress = await _db.UserProgresses.FirstAsync(p => p.UserId == userId);
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == userId);
        var done = await _db.UserChallenges.Where(c => c.UserId == userId).Select(c => c.ChallengeId).ToListAsync();
        var challenges = await _db.Challenges.ToListAsync();
        var lessons = await _db.QuizResults.CountAsync(r => r.UserId == userId);
        var sims = await _db.SimulationResults.CountAsync(r => r.UserId == userId);
        var total = journey.VirtualBalance + journey.Savings + journey.EmergencyFund + journey.InvestmentValue;
        var saveRate = total == 0 ? 0 : (journey.Savings + journey.EmergencyFund) * 100 / total;

        foreach (var challenge in challenges)
        {
            if (done.Contains(challenge.Id)) continue;
            var met = challenge.RuleKey switch
            {
                "save-30" => saveRate >= 30,
                "needs" => journey.PlayedNeeds,
                "budget" => journey.PlayedBudget,
                "invest" => journey.PlayedInvestSim,
                "portfolio" => journey.PlayedPortfolio,
                "survive-3" => journey.EventsSurvived >= 3,
                "lesson" => lessons > 0,
                "sim" => sims > 0,
                "emergency" => journey.EmergencyFund >= 1000,
                "goal" => journey.GoalProgress >= journey.GoalAmount && journey.GoalAmount > 0,
                "stage-3" => journey.CurrentStage >= 3,
                "month-2" => journey.CurrentMonth >= 2,
                "debt-free" => journey.Debt == 0 && sims > 0,
                "risk" => progress.RiskScore >= 40,
                "plan" => progress.PlanningScore >= 40,
                _ => false
            };
            if (!met) continue;
            _db.UserChallenges.Add(new UserChallenge { UserId = userId, ChallengeId = challenge.Id });
            progress.Xp += challenge.Xp;
            done.Add(challenge.Id);
        }
        await _db.SaveChangesAsync();
    }

    private static void Nudge(UserProgress progress, string category, int score)
    {
        int Mix(int current) => GameRules.ClampScore((current * 3 + score) / 4);
        switch (category)
        {
            case "Saving": progress.SavingScore = Mix(progress.SavingScore); break;
            case "Budgeting": progress.BudgetingScore = Mix(progress.BudgetingScore); break;
            case "Spending": progress.SpendingScore = Mix(progress.SpendingScore); break;
            case "Investing": progress.InvestingScore = Mix(progress.InvestingScore); break;
            case "Risk": progress.RiskScore = Mix(progress.RiskScore); break;
            default: progress.PlanningScore = Mix(progress.PlanningScore); break;
        }
    }
}
