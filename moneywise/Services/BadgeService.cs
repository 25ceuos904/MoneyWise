using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Models;

namespace MoneyWise.Services;

public class BadgeService
{
    private readonly AppDbContext _db;
    public BadgeService(AppDbContext db) => _db = db;

    public async Task<List<Badge>> EvaluateAsync(string userId)
    {
        var progress = await _db.UserProgresses.FirstAsync(p => p.UserId == userId);
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == userId);
        var earned = await _db.UserBadges.Where(b => b.UserId == userId).Select(b => b.BadgeId).ToListAsync();
        var badges = await _db.Badges.ToListAsync();
        var goodDecision = await _db.SimulationResults.AnyAsync(r => r.UserId == userId && r.Score >= 75);
        var unlocked = new List<Badge>();

        foreach (var badge in badges)
        {
            if (earned.Contains(badge.Id)) continue;
            var met = badge.Code switch
            {
                "first_saver" => journey.Savings > 0 || journey.GoalProgress > 0,
                "smart_decision" => goodDecision,
                "budget_builder" => journey.PlayedBudget,
                "emergency_ready" => journey.EmergencyFund >= 1000,
                "investment_explorer" => journey.PlayedPortfolio || journey.PlayedInvestSim || journey.InvestmentValue > 0,
                "risk_balancer" => journey.PlayedPortfolio && journey.InvestmentValue > 0 && journey.Savings > 0,
                "goal_crusher" => journey.GoalAmount > 0 && journey.GoalProgress >= journey.GoalAmount,
                "streak_7" => progress.Streak >= 7,
                "money_master" => journey.CurrentStage >= GameRules.MaxStage(await AgeOf(userId)) || progress.Xp >= 900,
                "journey_starter" => journey.CurrentMonth > 1 || await _db.SimulationResults.AnyAsync(r => r.UserId == userId),
                _ => false
            };
            if (!met) continue;
            _db.UserBadges.Add(new UserBadge { UserId = userId, BadgeId = badge.Id });
            unlocked.Add(badge);
        }
        if (unlocked.Count > 0) await _db.SaveChangesAsync();
        return unlocked;
    }

    private async Task<string> AgeOf(string userId) =>
        await _db.Users.Where(u => u.Id == userId).Select(u => u.AgeGroup).FirstAsync();
}
