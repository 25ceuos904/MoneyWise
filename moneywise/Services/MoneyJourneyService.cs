using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Models;

namespace MoneyWise.Services;

public class MoneyJourneyService
{
    private readonly AppDbContext _db;
    private readonly SimulationService _sim;
    private readonly ProgressService _progress;

    public MoneyJourneyService(AppDbContext db, SimulationService sim, ProgressService progress)
    {
        _db = db;
        _sim = sim;
        _progress = progress;
    }

    public async Task<MoneyJourney> GetAsync(string userId) =>
        await _db.MoneyJourneys
            .Include(j => j.CurrentScenario)!.ThenInclude(s => s!.Options)
            .Include(j => j.PendingLifeEvent)!.ThenInclude(e => e!.Scenario)!.ThenInclude(s => s!.Options)
            .FirstAsync(j => j.UserId == userId);

    public void StartFor(ApplicationUser user)
    {
        var age = GameRules.AgeFloor(user.AgeGroup);
        var (income, goalName, goal) = age switch
        {
            >= 16 => (20000, "Emergency cushion", 30000),
            >= 13 => (4000, "A phone you chose", 15000),
            _ => (2000, "A bicycle", 8000)
        };
        user.Journey = new MoneyJourney
        {
            MonthlyIncome = income,
            VirtualBalance = income,
            GoalName = goalName,
            GoalAmount = goal,
            FinancialScore = 40
        };
        user.Progress = new UserProgress();
    }

    public async Task EnsureScenarioAsync(MoneyJourney journey, string ageGroup)
    {
        if (journey.CurrentScenarioId != null || journey.PendingLifeEventId != null || journey.AwaitingMonthEnd) return;
        var age = GameRules.AgeFloor(ageGroup);
        var scenario = await _db.Scenarios
            .Include(s => s.Options)
            .Where(s => !s.IsLifeEvent && s.Stage == journey.CurrentStage && s.MinAge <= age)
            .OrderBy(s => s.Id)
            .FirstOrDefaultAsync()
            ?? await _db.Scenarios.Include(s => s.Options).Where(s => !s.IsLifeEvent && s.MinAge <= age).OrderBy(s => s.Stage).FirstAsync();
        journey.CurrentScenarioId = scenario.Id;
        journey.CurrentScenario = scenario;
        await _db.SaveChangesAsync();
    }

    public async Task<DecisionOutcome> ChooseAsync(string userId, int optionId, bool fromEvent)
    {
        var user = await _db.Users.FirstAsync(u => u.Id == userId);
        var journey = await GetAsync(userId);
        var option = await _db.ScenarioOptions.Include(o => o.Scenario).FirstAsync(o => o.Id == optionId);
        var outcome = _sim.Apply(journey, option);
        _db.MoneyTransactions.Add(new MoneyTransaction
        {
            UserId = userId,
            Amount = option.BalanceChange + option.SavingsChange + option.EmergencyChange,
            Type = option.SavingsChange > 0 ? "Saving" : option.InvestmentChange != 0 ? "Investment" : "Expense",
            Note = option.Label
        });
        _db.SimulationResults.Add(new SimulationResult
        {
            UserId = userId,
            ScenarioId = option.ScenarioId,
            ScenarioOptionId = option.Id,
            Score = option.Score,
            XpEarned = option.Xp
        });
        if (fromEvent)
        {
            journey.EventsSurvived += 1;
            journey.PendingLifeEventId = null;
            journey.AwaitingMonthEnd = true;
        }
        else
        {
            journey.CurrentScenarioId = null;
            var age = GameRules.AgeFloor(user.AgeGroup);
            var events = await _db.LifeEvents.Where(e => e.MinAge <= age).ToListAsync();
            if (events.Count > 0)
            {
                var pick = events[(journey.CurrentMonth + journey.CurrentStage) % events.Count];
                journey.PendingLifeEventId = pick.Id;
            }
            else journey.AwaitingMonthEnd = true;
        }
        await _db.SaveChangesAsync();
        await _progress.AddXpAsync(userId, outcome.Xp, option.Scenario?.Category ?? "Planning", option.Score);
        return outcome;
    }

    public async Task AdvanceMonthAsync(string userId)
    {
        var user = await _db.Users.FirstAsync(u => u.Id == userId);
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == userId);
        var pay = Math.Max(0, journey.MonthlyIncome - journey.MonthlyObligation);
        journey.VirtualBalance += pay;
        if (journey.Debt > 0 && journey.MonthlyObligation > 0)
        {
            var paid = Math.Min(journey.Debt, journey.MonthlyObligation);
            journey.Debt -= paid;
        }
        journey.CurrentMonth += 1;
        journey.AwaitingMonthEnd = false;
        journey.PendingLifeEventId = null;
        journey.CurrentScenarioId = null;
        var max = GameRules.MaxStage(user.AgeGroup);
        if (journey.CurrentMonth % 2 == 1 && journey.CurrentStage < max)
        {
            journey.CurrentStage += 1;
            ApplyStageGoal(journey, user.AgeGroup);
        }
        _db.MoneyTransactions.Add(new MoneyTransaction
        {
            UserId = userId,
            Amount = pay,
            Type = "Income",
            Note = $"Month {journey.CurrentMonth} income"
        });
        await _db.SaveChangesAsync();
        await EnsureScenarioAsync(journey, user.AgeGroup);
        await _progress.CheckChallengesAsync(userId);
    }

    public static void ApplyStageGoal(MoneyJourney journey, string ageGroup)
    {
        var age = GameRules.AgeFloor(ageGroup);
        switch (journey.CurrentStage)
        {
            case 2:
                journey.GoalName = "A clearer savings jar";
                journey.GoalAmount = age >= 16 ? 15000 : 5000;
                break;
            case 3:
                journey.GoalName = "One calm month";
                journey.GoalAmount = age >= 16 ? 20000 : 6000;
                break;
            case 4:
                journey.MonthlyIncome = age >= 16 ? 35000 : journey.MonthlyIncome;
                journey.GoalName = age >= 16 ? "First salary buffer" : "Festival fund";
                journey.GoalAmount = age >= 16 ? 25000 : 8000;
                break;
            case 5:
                journey.GoalName = "Emergency fund";
                journey.GoalAmount = age >= 16 ? 40000 : 10000;
                break;
            case 6:
                journey.GoalName = "A patient investing habit";
                journey.GoalAmount = 15000;
                break;
            case 7:
                journey.GoalName = "A plan you can keep";
                journey.GoalAmount = 20000;
                break;
        }
        journey.GoalProgress = Math.Min(journey.GoalProgress, journey.GoalAmount);
    }
}
