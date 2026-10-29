using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Models;
using MoneyWise.Services;
using Xunit;

namespace MoneyWise.Tests;

public class MoneyJourneyServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SimulationService _sim;
    private readonly ProgressService _progress;
    private readonly MoneyJourneyService _service;

    public MoneyJourneyServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _sim = new SimulationService();
        _progress = new ProgressService(_db, new BadgeService(_db));
        _service = new MoneyJourneyService(_db, _sim, _progress);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Theory]
    [InlineData("10-12", 2000, "A bicycle", 8000)]
    [InlineData("13-15", 4000, "A phone you chose", 15000)]
    [InlineData("16-18", 20000, "Emergency cushion", 30000)]
    public void StartFor_InitializesJourneyForAgeGroup(string ageGroup, int expectedIncome, string expectedGoal, int expectedGoalAmount)
    {
        var user = new ApplicationUser { AgeGroup = ageGroup, Email = "test@user.com" };
        _service.StartFor(user);

        Assert.NotNull(user.Journey);
        Assert.Equal(expectedIncome, user.Journey.MonthlyIncome);
        Assert.Equal(expectedIncome, user.Journey.VirtualBalance);
        Assert.Equal(expectedGoal, user.Journey.GoalName);
        Assert.Equal(expectedGoalAmount, user.Journey.GoalAmount);
        Assert.NotNull(user.Progress);
    }

    [Fact]
    public async Task EnsureScenarioAsync_FindsScenarioForAge13Stage4And5()
    {
        await DbSeeder.SeedAsync(_db);
        var user = new ApplicationUser { Id = "user-13", AgeGroup = "13-15" };
        _service.StartFor(user);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var journey = user.Journey!;
        journey.CurrentStage = 4;
        journey.CurrentScenarioId = null;

        await _service.EnsureScenarioAsync(journey, "13-15");

        Assert.NotNull(journey.CurrentScenarioId);
        Assert.NotNull(journey.CurrentScenario);
        Assert.True(journey.CurrentScenario.MinAge <= 13);
        Assert.Equal(4, journey.CurrentScenario.Stage);

        // Advance to Stage 5
        journey.CurrentStage = 5;
        journey.CurrentScenarioId = null;
        await _service.EnsureScenarioAsync(journey, "13-15");

        Assert.NotNull(journey.CurrentScenarioId);
        Assert.NotNull(journey.CurrentScenario);
        Assert.True(journey.CurrentScenario.MinAge <= 13);
        Assert.Equal(5, journey.CurrentScenario.Stage);
    }

    [Fact]
    public async Task AdvanceMonthAsync_UpdatesBalanceAndIncome()
    {
        await DbSeeder.SeedAsync(_db);
        var user = new ApplicationUser { Id = "user-month", AgeGroup = "16-18" };
        _service.StartFor(user);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var initialBalance = user.Journey!.VirtualBalance;
        await _service.AdvanceMonthAsync("user-month");

        var updatedJourney = await _db.MoneyJourneys.FirstAsync(j => j.UserId == "user-month");
        Assert.Equal(2, updatedJourney.CurrentMonth);
        Assert.Equal(initialBalance + updatedJourney.MonthlyIncome, updatedJourney.VirtualBalance);
    }
}
