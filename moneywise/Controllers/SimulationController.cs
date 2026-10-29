using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Services;

namespace MoneyWise.Controllers;

public class SimulationController : GameController
{
    private readonly AppDbContext _db;
    private readonly ProgressService _progress;
    public SimulationController(AppDbContext db, ProgressService progress)
    {
        _db = db;
        _progress = progress;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        var age = GameRules.AgeFloor(user.AgeGroup);
        var items = await _db.Scenarios.Where(s => s.MinAge <= age).OrderBy(s => s.Stage).ToListAsync();
        return View(items);
    }

    public IActionResult Needs() => View();

    [HttpPost]
    public async Task<IActionResult> Needs(IFormCollection form)
    {
        var keys = new[] { "groceries", "game", "rent", "headphones", "books", "phone", "medicine", "restaurant" };
        var needs = new HashSet<string> { "groceries", "rent", "books", "medicine" };
        var correct = 0;
        foreach (var key in keys)
            if ((form[key] == "need") == needs.Contains(key)) correct++;
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == UserId);
        journey.PlayedNeeds = true;
        await _db.SaveChangesAsync();
        await _progress.AddXpAsync(UserId, correct * 4, "Spending", 40 + correct * 7);
        TempData["Msg"] = $"You sorted {correct} of {keys.Length}. Books for school are usually a need. A new phone or a restaurant meal is usually a want.";
        return RedirectToAction(nameof(Feedback));
    }

    public IActionResult Budget() => View();

    [HttpPost]
    public async Task<IActionResult> Budget(int needs, int wants, int savings, int emergency, int invest)
    {
        if (needs < 0 || wants < 0 || savings < 0 || emergency < 0 || invest < 0)
        {
            TempData["Msg"] = "Budget amounts cannot be negative.";
            return RedirectToAction(nameof(Budget));
        }
        var sum = needs + wants + savings + emergency + invest;
        if (sum != 30000)
        {
            TempData["Msg"] = $"Those amounts add up to {GameRules.Inr(sum)}. They need to add up to ₹30,000.";
            return RedirectToAction(nameof(Budget));
        }
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == UserId);
        journey.PlayedBudget = true;
        journey.Savings += savings / 10;
        journey.EmergencyFund += emergency / 10;
        journey.InvestmentValue += invest / 10;
        await _db.SaveChangesAsync();
        var score = needs >= 12000 && savings + emergency >= 6000 ? 86 : 62;
        await _progress.AddXpAsync(UserId, 25, "Budgeting", score);
        TempData["Msg"] = $"Needs {Pct(needs)}, wants {Pct(wants)}, savings {Pct(savings)}, emergency {Pct(emergency)}, investing {Pct(invest)}. There is no single perfect split. If needs are uncovered, shrink wants before shrinking the buffer.";
        return RedirectToAction(nameof(Feedback));
    }

    public IActionResult Inflation() => View();
    public IActionResult Compound() => View();

    public IActionResult Emi() => View();

    [HttpPost]
    public async Task<IActionResult> Emi(string choice)
    {
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == UserId);
        var msg = choice switch
        {
            "now" => "Paying now uses today's money and avoids interest. Only do it if needs and a buffer still fit.",
            "save" => "Waiting turns the phone into a goal instead of a debt. The current phone still works in this story.",
            _ => "An EMI is not free. Interest makes the total higher than ₹40,000, and the payment returns every month."
        };
        if (choice == "emi")
        {
            journey.Debt += 40000;
            journey.MonthlyObligation += 2500;
        }
        else if (choice == "now") journey.VirtualBalance = Math.Max(0, journey.VirtualBalance - 5000);
        await _db.SaveChangesAsync();
        await _progress.AddXpAsync(UserId, 20, "Planning", choice == "emi" ? 48 : 82);
        TempData["Msg"] = msg + " All of this money is simulated.";
        return RedirectToAction(nameof(Feedback));
    }

    public async Task<IActionResult> Credit()
    {
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        if (GameRules.AgeFloor(user.AgeGroup) < 16) return View("Locked");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Credit(string choice)
    {
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == UserId);
        if (choice == "minimum") journey.Debt += 8000;
        if (choice == "full") journey.VirtualBalance = Math.Max(0, journey.VirtualBalance - 3000);
        await _db.SaveChangesAsync();
        await _progress.AddXpAsync(UserId, 20, "Risk", choice == "minimum" ? 40 : 80);
        TempData["Msg"] = choice switch
        {
            "minimum" => "Paying only the minimum can leave a balance that grows with interest. The limit was never your money.",
            "skip" => "Skipping a want keeps the limit unused. A credit limit is borrowing, not income.",
            _ => "Paying the full bill avoids interest. The purchase still has to fit the month."
        };
        return RedirectToAction(nameof(Feedback));
    }

    public async Task<IActionResult> Insurance()
    {
        var user = await _db.Users.FirstAsync(u => u.Id == UserId);
        if (GameRules.AgeFloor(user.AgeGroup) < 16) return View("Locked");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Insurance(string choice)
    {
        var journey = await _db.MoneyJourneys.FirstAsync(j => j.UserId == UserId);
        if (choice == "none")
        {
            var fromEmergency = Math.Min(journey.EmergencyFund, 20000);
            journey.EmergencyFund -= fromEmergency;
            var left = 20000 - fromEmergency;
            var fromSavings = Math.Min(journey.Savings, left);
            journey.Savings -= fromSavings;
            left -= fromSavings;
            journey.VirtualBalance = Math.Max(0, journey.VirtualBalance - left);
        }
        else journey.EmergencyFund = Math.Max(0, journey.EmergencyFund - 2000);
        await _db.SaveChangesAsync();
        await _progress.AddXpAsync(UserId, 20, "Risk", choice == "cover" ? 84 : 45);
        TempData["Msg"] = choice == "cover"
            ? "A premium is the price of protection. This is not a recommendation to buy any real policy. The big bill is partly absorbed in this story."
            : "Without protection, the ₹20,000 bill comes straight out of savings. Insurance is a concept here, not a product offer.";
        return RedirectToAction(nameof(Feedback));
    }

    public IActionResult Feedback()
    {
        ViewBag.Msg = TempData["Msg"] as string ?? "Choice saved.";
        return View();
    }

    private static string Pct(int part) => $"{(int)Math.Round(part * 100d / 30000)}%";
}
