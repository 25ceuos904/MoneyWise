using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;

namespace MoneyWise.Controllers;

public class ProgressController : GameController
{
    private readonly AppDbContext _db;
    public ProgressController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var progress = await _db.UserProgresses.FirstAsync(p => p.UserId == UserId);
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == UserId);
        ViewBag.Lessons = await _db.QuizResults.CountAsync(r => r.UserId == UserId);
        ViewBag.Challenges = await _db.UserChallenges.CountAsync(c => c.UserId == UserId);
        ViewBag.Sims = await _db.SimulationResults.CountAsync(s => s.UserId == UserId);
        ViewBag.Badges = await _db.UserBadges.Include(b => b.Badge).Where(b => b.UserId == UserId).ToListAsync();
        ViewBag.AllBadges = await _db.Badges.ToListAsync();
        ViewBag.Journey = journey;
        return View(progress);
    }
}
