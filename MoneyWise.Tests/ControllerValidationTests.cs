using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using MoneyWise.Controllers;
using MoneyWise.Data;
using MoneyWise.Models;
using MoneyWise.Services;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MoneyWise.Tests;

public class ControllerValidationTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ProgressService _progress;

    public ControllerValidationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _progress = new ProgressService(_db, new BadgeService(_db));
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    private void SetupUser(Controller controller, string userId, string ageGroup = "16-18")
    {
        var user = new ApplicationUser { Id = userId, AgeGroup = ageGroup, DisplayName = "Test User" };
        user.Journey = new MoneyJourney { UserId = userId, VirtualBalance = 20000, MonthlyIncome = 20000 };
        user.Progress = new UserProgress { UserId = userId, Streak = 5, Xp = 100 };
        _db.Users.Add(user);
        _db.SaveChanges();

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = tempData;
    }

    [Fact]
    public async Task SimulationController_Budget_RejectsNegativeInputs()
    {
        var controller = new SimulationController(_db, _progress);
        SetupUser(controller, "user-budget");

        var result = await controller.Budget(-5000, 35000, 0, 0, 0) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Budget", result.ActionName);
        Assert.Equal("Budget amounts cannot be negative.", controller.TempData["Msg"]);
    }

    [Fact]
    public async Task InvestmentController_Portfolio_RejectsNegativeInputs()
    {
        var controller = new InvestmentController(_db, _progress);
        SetupUser(controller, "user-portfolio");

        var result = await controller.Portfolio(-2000, 12000, 0, 0) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Portfolio", result.ActionName);
        Assert.Equal("Investment amounts cannot be negative.", controller.TempData["Msg"]);
    }

    [Fact]
    public async Task InvestmentController_Timeline_RejectsNegativeInputs()
    {
        var controller = new InvestmentController(_db, _progress);
        SetupUser(controller, "user-timeline");

        var result = await controller.Timeline(-1000, 11000, 0, 0) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Timeline", result.ActionName);
        Assert.Equal("Investment amounts cannot be negative.", controller.TempData["Msg"]);
    }

    [Fact]
    public async Task ProfileController_Reset_ResetsStreakAndLastActiveDate()
    {
        var controller = new ProfileController(_db);
        SetupUser(controller, "user-reset");

        var result = await controller.Reset() as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Dashboard", result.ControllerName);

        var progress = await _db.UserProgresses.FirstAsync(p => p.UserId == "user-reset");
        Assert.Equal(0, progress.Xp);
        Assert.Equal(1, progress.Streak);
        Assert.True((DateTime.UtcNow - progress.LastActiveDate).TotalMinutes < 1);
    }
}
