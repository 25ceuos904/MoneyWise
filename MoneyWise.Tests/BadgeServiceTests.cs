using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Models;
using MoneyWise.Services;
using Xunit;

namespace MoneyWise.Tests;

public class BadgeServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly BadgeService _service;

    public BadgeServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new BadgeService(_db);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task EvaluateAsync_AwardsFirstSaverAndEmergencyReady()
    {
        await DbSeeder.SeedAsync(_db);
        var user = new ApplicationUser { Id = "badge-user", AgeGroup = "16-18" };
        user.Journey = new MoneyJourney { UserId = "badge-user", Savings = 1500, EmergencyFund = 1200 };
        user.Progress = new UserProgress { UserId = "badge-user", Streak = 7, Xp = 200 };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var unlocked = await _service.EvaluateAsync("badge-user");

        Assert.Contains(unlocked, b => b.Code == "first_saver");
        Assert.Contains(unlocked, b => b.Code == "emergency_ready");
        Assert.Contains(unlocked, b => b.Code == "streak_7");
    }
}
