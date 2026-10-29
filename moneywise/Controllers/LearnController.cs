using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Services;

namespace MoneyWise.Controllers;

public class LearnController : GameController
{
    private readonly AppDbContext _db;
    private readonly ProgressService _progress;

    public LearnController(AppDbContext db, ProgressService progress)
    {
        _db = db;
        _progress = progress;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        var age = GameRules.AgeFloor(user.AgeGroup);
        var lessons = await _db.Lessons.Where(l => l.MinAge <= age).OrderBy(l => l.Stage).ThenBy(l => l.Id).ToListAsync();
        var done = await _db.QuizResults.Where(r => r.UserId == UserId).Select(r => r.LessonId).ToListAsync();
        ViewBag.Done = done;
        return View(lessons);
    }

    public async Task<IActionResult> Play(int id)
    {
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        var lesson = await _db.Lessons.Include(l => l.Questions).ThenInclude(q => q.Options).FirstOrDefaultAsync(l => l.Id == id);
        if (lesson == null || lesson.MinAge > GameRules.AgeFloor(user.AgeGroup)) return RedirectToAction(nameof(Index));
        return View(lesson);
    }

    [HttpPost]
    public async Task<IActionResult> Play(int id, IFormCollection form)
    {
        var lesson = await _db.Lessons.Include(l => l.Questions).ThenInclude(q => q.Options).FirstAsync(l => l.Id == id);
        var correct = 0;
        foreach (var question in lesson.Questions)
        {
            if (int.TryParse(form[$"q{question.Id}"], out var optionId))
            {
                var picked = question.Options.FirstOrDefault(o => o.Id == optionId);
                if (picked?.IsCorrect == true) correct++;
            }
        }
        var already = await _db.QuizResults.AnyAsync(r => r.UserId == UserId && r.LessonId == id);
        var xp = already ? 0 : correct * 10 + (correct == lesson.Questions.Count ? lesson.XpReward : 0);
        if (!already)
        {
            _db.QuizResults.Add(new Models.QuizResult
            {
                UserId = UserId,
                LessonId = id,
                CorrectCount = correct,
                Total = lesson.Questions.Count,
                XpEarned = xp
            });
            await _db.SaveChangesAsync();
            await _progress.AddXpAsync(UserId, xp, lesson.Category, 50 + correct * 20);
        }
        TempData["Correct"] = correct.ToString();
        TempData["Total"] = lesson.Questions.Count.ToString();
        TempData["Xp"] = xp.ToString();
        TempData["Explain"] = lesson.Explanation;
        return RedirectToAction(nameof(Result), new { id });
    }

    public async Task<IActionResult> Result(int id)
    {
        var lesson = await _db.Lessons.FirstAsync(l => l.Id == id);
        ViewBag.Correct = TempData["Correct"] ?? "0";
        ViewBag.Total = TempData["Total"] ?? "0";
        ViewBag.Xp = TempData["Xp"] ?? "0";
        ViewBag.Explain = TempData["Explain"] ?? lesson.Explanation;
        return View(lesson);
    }
}
