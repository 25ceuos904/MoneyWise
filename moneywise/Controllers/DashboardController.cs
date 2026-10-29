using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Services;

namespace MoneyWise.Controllers;

public class DashboardController : GameController
{
    private readonly AppDbContext _db;
    private readonly MoneyJourneyService _journey;
    private readonly ProgressService _progress;

    public DashboardController(AppDbContext db, MoneyJourneyService journey, ProgressService progress)
    {
        _db = db;
        _journey = journey;
        _progress = progress;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        await _progress.TouchStreakAsync(user);
        var journey = await _journey.GetAsync(UserId);
        await _journey.EnsureScenarioAsync(journey, user.AgeGroup);
        journey = await _journey.GetAsync(UserId);
        var progress = await _db.UserProgresses.FirstAsync(p => p.UserId == UserId);
        var badge = await _db.UserBadges.Include(b => b.Badge).Where(b => b.UserId == UserId).OrderByDescending(b => b.EarnedAt).FirstOrDefaultAsync();
        var done = await _db.UserChallenges.Where(u => u.UserId == UserId).Select(u => u.ChallengeId).ToListAsync();
        var challenge = await _db.Challenges.Where(c => c.MinAge <= GameRules.AgeFloor(user.AgeGroup) && !done.Contains(c.Id)).FirstOrDefaultAsync();
        ViewBag.User = user;
        ViewBag.Journey = journey;
        ViewBag.Progress = progress;
        ViewBag.Badge = badge?.Badge;
        ViewBag.Challenge = challenge;
        return View();
    }
}
