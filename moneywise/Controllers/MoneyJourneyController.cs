using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Services;

namespace MoneyWise.Controllers;

public class MoneyJourneyController : GameController
{
    private readonly AppDbContext _db;
    private readonly MoneyJourneyService _journey;

    public MoneyJourneyController(AppDbContext db, MoneyJourneyService journey)
    {
        _db = db;
        _journey = journey;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        var journey = await _journey.GetAsync(UserId);
        await _journey.EnsureScenarioAsync(journey, user.AgeGroup);
        journey = await _journey.GetAsync(UserId);
        ViewBag.StageName = GameRules.Stages[Math.Clamp(journey.CurrentStage - 1, 0, 6)];
        return View(journey);
    }

    [HttpPost]
    public async Task<IActionResult> Choose(int optionId, bool fromEvent)
    {
        var outcome = await _journey.ChooseAsync(UserId, optionId, fromEvent);
        TempData["Headline"] = outcome.Headline;
        TempData["Why"] = outcome.Why;
        TempData["Immediate"] = outcome.Immediate;
        TempData["Future"] = outcome.Future;
        TempData["Score"] = outcome.Score.ToString();
        TempData["Xp"] = outcome.Xp.ToString();
        return RedirectToAction(nameof(Result));
    }

    public async Task<IActionResult> Result()
    {
        ViewBag.Headline = TempData["Headline"] as string ?? "Choice saved";
        ViewBag.Why = TempData["Why"] as string ?? "Every choice changes what next month can do.";
        ViewBag.Immediate = TempData["Immediate"] as string ?? "";
        ViewBag.Future = TempData["Future"] as string ?? "";
        ViewBag.Score = TempData["Score"] as string ?? "0";
        ViewBag.Xp = TempData["Xp"] as string ?? "0";
        var journey = await _journey.GetAsync(UserId);
        return View(journey);
    }

    [HttpPost]
    public async Task<IActionResult> NextMonth()
    {
        await _journey.AdvanceMonthAsync(UserId);
        return RedirectToAction(nameof(Index));
    }
}
