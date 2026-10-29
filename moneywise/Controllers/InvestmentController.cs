using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Services;

namespace MoneyWise.Controllers;

public class InvestmentController : GameController
{
    private readonly AppDbContext _db;
    private readonly ProgressService _progress;
    public InvestmentController(AppDbContext db, ProgressService progress)
    {
        _db = db;
        _progress = progress;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        var age = GameRules.AgeFloor(user.AgeGroup);
        var items = await _db.InvestmentConcepts.Where(c => c.MinAge <= age).ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var item = await _db.InvestmentConcepts.FirstOrDefaultAsync(c => c.Id == id);
        if (item == null) return RedirectToAction(nameof(Index));
        return View(item);
    }

    public IActionResult Portfolio() => View();

    [HttpPost]
    public async Task<IActionResult> Portfolio(int savings, int fd, int fund, int stocks)
    {
        if (savings < 0 || fd < 0 || fund < 0 || stocks < 0)
        {
            TempData["Msg"] = "Investment amounts cannot be negative.";
            return RedirectToAction(nameof(Portfolio));
        }
        if (savings + fd + fund + stocks != 10000)
        {
            TempData["Msg"] = "The four amounts need to add up to ₹10,000 of virtual money.";
            return RedirectToAction(nameof(Portfolio));
        }
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == UserId);
        journey.PlayedPortfolio = true;
        journey.InvestmentValue += fd / 5 + fund / 5 + stocks / 5;
        journey.Savings += savings / 5;
        await _db.SaveChangesAsync();
        var stockShare = stocks * 100 / 10000;
        var buckets = new[] { savings, fd, fund, stocks }.Count(v => v > 0);
        var risk = stockShare >= 50 ? "Higher" : stockShare >= 20 ? "Medium" : "Lower";
        await _progress.AddXpAsync(UserId, 25, "Investing", buckets >= 3 ? 84 : 60);
        TempData["Msg"] = $"Risk level: {risk}. Diversification: {(buckets >= 3 ? "spread across several places" : "concentrated")}. A larger stock slice can move more, including down. This is not a forecast and not a recommendation.";
        return RedirectToAction("Feedback", "Simulation");
    }

    public IActionResult Timeline() => View();

    [HttpPost]
    public async Task<IActionResult> Timeline(int savings, int fd, int fund, int stocks)
    {
        if (savings < 0 || fd < 0 || fund < 0 || stocks < 0)
        {
            TempData["Msg"] = "Investment amounts cannot be negative.";
            return RedirectToAction(nameof(Timeline));
        }
        if (savings + fd + fund + stocks != 10000)
        {
            TempData["Msg"] = "Split exactly ₹10,000 before running the illustration.";
            return RedirectToAction(nameof(Timeline));
        }
        var months = new List<int>();
        var value = 10000d;
        var rng = new Random(savings * 3 + fd * 5 + fund * 7 + stocks);
        for (var m = 1; m <= 12; m++)
        {
            var swing = (rng.NextDouble() - 0.48) * (stocks / 10000d) * 0.08;
            var fundSwing = (rng.NextDouble() - 0.45) * (fund / 10000d) * 0.03;
            var drift = savings * 0.003 + fd * 0.004 + fund * 0.005;
            value = Math.Max(1000, value + drift + value * (swing + fundSwing));
            months.Add((int)Math.Round(value));
        }
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == UserId);
        journey.PlayedInvestSim = true;
        await _db.SaveChangesAsync();
        await _progress.AddXpAsync(UserId, 20, "Risk", 70);
        TempData["Path"] = string.Join(",", months);
        TempData["Msg"] = "These 12 months are a randomized illustration. Stocks can pull the line down. Nothing here is a guaranteed return.";
        return RedirectToAction(nameof(Timeline));
    }
}
