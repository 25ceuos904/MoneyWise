using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Services;
using MoneyWise.ViewModels;

namespace MoneyWise.Controllers;

public class ProfileController : GameController
{
    private readonly AppDbContext _db;
    public ProfileController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        var progress = await _db.UserProgresses.FirstAsync(p => p.UserId == UserId);
        ViewBag.Progress = progress;
        ViewBag.Badges = await _db.UserBadges.CountAsync(b => b.UserId == UserId);
        return View(new ProfileEditViewModel
        {
            Name = user.DisplayName,
            AgeGroup = user.AgeGroup,
            Experience = user.ExperienceLevel
        });
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfileEditViewModel model)
    {
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        if (!ModelState.IsValid)
        {
            ViewBag.Progress = await _db.UserProgresses.FirstAsync(p => p.UserId == UserId);
            ViewBag.Badges = await _db.UserBadges.CountAsync(b => b.UserId == UserId);
            return View(model);
        }

        var ageGroupChanged = user.AgeGroup != model.AgeGroup;
        user.DisplayName = model.Name.Trim();
        user.AgeGroup = model.AgeGroup;
        user.ExperienceLevel = model.Experience;

        if (ageGroupChanged)
        {
            var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == UserId);
            var maxStage = GameRules.MaxStage(model.AgeGroup);
            if (journey.CurrentStage > maxStage)
            {
                journey.CurrentStage = maxStage;
            }
            var age = GameRules.AgeFloor(model.AgeGroup);
            var (income, goalName, goalAmount) = age switch
            {
                >= 16 => (20000, "Emergency cushion", 30000),
                >= 13 => (4000, "A phone you chose", 15000),
                _ => (2000, "A bicycle", 8000)
            };
            journey.MonthlyIncome = income;
            journey.GoalName = goalName;
            journey.GoalAmount = goalAmount;
            journey.GoalProgress = Math.Min(journey.GoalProgress, journey.GoalAmount);
            journey.CurrentScenarioId = null;
            var journeyService = new MoneyJourneyService(_db, new SimulationService(), null!);
            await journeyService.EnsureScenarioAsync(journey, model.AgeGroup);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reset()
    {
        var progress = await _db.UserProgresses.FirstAsync(p => p.UserId == UserId);
        progress.Xp = 0;
        progress.Streak = 1;
        progress.LastActiveDate = DateTime.UtcNow;
        progress.SavingScore = progress.BudgetingScore = progress.SpendingScore = 20;
        progress.InvestingScore = progress.RiskScore = progress.PlanningScore = 20;
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == UserId);
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        var starter = new Models.ApplicationUser { AgeGroup = user.AgeGroup };
        new MoneyJourneyService(_db, new SimulationService(), null!).StartFor(starter);
        journey.CurrentStage = 1;
        journey.CurrentMonth = 1;
        journey.CurrentScenarioId = null;
        journey.PendingLifeEventId = null;
        journey.AwaitingMonthEnd = false;
        journey.VirtualBalance = starter.Journey!.VirtualBalance;
        journey.Savings = 0;
        journey.EmergencyFund = 0;
        journey.InvestmentValue = 0;
        journey.Debt = 0;
        journey.MonthlyObligation = 0;
        journey.MonthlyIncome = starter.Journey.MonthlyIncome;
        journey.GoalName = starter.Journey.GoalName;
        journey.GoalAmount = starter.Journey.GoalAmount;
        journey.GoalProgress = 0;
        journey.EventsSurvived = 0;
        journey.PlayedBudget = journey.PlayedPortfolio = journey.PlayedNeeds = journey.PlayedInvestSim = false;
        _db.QuizResults.RemoveRange(_db.QuizResults.Where(r => r.UserId == UserId));
        _db.SimulationResults.RemoveRange(_db.SimulationResults.Where(r => r.UserId == UserId));
        _db.UserChallenges.RemoveRange(_db.UserChallenges.Where(c => c.UserId == UserId));
        _db.UserBadges.RemoveRange(_db.UserBadges.Where(b => b.UserId == UserId));
        _db.MoneyTransactions.RemoveRange(_db.MoneyTransactions.Where(t => t.UserId == UserId));
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "Dashboard");
    }
}
