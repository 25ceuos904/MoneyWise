using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Services;

namespace MoneyWise.Controllers;

public class ChallengeController : GameController
{
    private readonly AppDbContext _db;
    private readonly ProgressService _progress;
    public ChallengeController(AppDbContext db, ProgressService progress)
    {
        _db = db;
        _progress = progress;
    }

    public async Task<IActionResult> Index()
    {
        await _progress.CheckChallengesAsync(UserId);
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        var age = GameRules.AgeFloor(user.AgeGroup);
        var items = await _db.Challenges.Where(c => c.MinAge <= age).ToListAsync();
        var done = await _db.UserChallenges.Where(c => c.UserId == UserId).Select(c => c.ChallengeId).ToListAsync();
        ViewBag.Done = done;
        return View(items);
    }
}
