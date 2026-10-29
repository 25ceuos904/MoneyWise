using Microsoft.EntityFrameworkCore;
using MoneyWise.Data;
using MoneyWise.Models;

namespace MoneyWise.Services;

public class LifeEventService
{
    private readonly AppDbContext _db;
    public LifeEventService(AppDbContext db) => _db = db;

    public async Task<List<LifeEvent>> ForAgeAsync(string ageGroup)
    {
        var age = GameRules.AgeFloor(ageGroup);
        return await _db.LifeEvents.Include(e => e.Scenario).Where(e => e.MinAge <= age).OrderBy(e => e.Id).ToListAsync();
    }
}
