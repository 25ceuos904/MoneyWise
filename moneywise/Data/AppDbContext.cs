using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoneyWise.Models;

namespace MoneyWise.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<QuizResult> QuizResults => Set<QuizResult>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<ScenarioOption> ScenarioOptions => Set<ScenarioOption>();
    public DbSet<SimulationResult> SimulationResults => Set<SimulationResult>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<UserChallenge> UserChallenges => Set<UserChallenge>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();
    public DbSet<InvestmentConcept> InvestmentConcepts => Set<InvestmentConcept>();
    public DbSet<UserProgress> UserProgresses => Set<UserProgress>();
    public DbSet<MoneyJourney> MoneyJourneys => Set<MoneyJourney>();
    public DbSet<MoneyTransaction> MoneyTransactions => Set<MoneyTransaction>();
    public DbSet<LifeEvent> LifeEvents => Set<LifeEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().HasOne(u => u.Progress).WithOne(p => p.User!).HasForeignKey<UserProgress>(p => p.UserId);
        builder.Entity<ApplicationUser>().HasOne(u => u.Journey).WithOne(j => j.User!).HasForeignKey<MoneyJourney>(j => j.UserId);
        builder.Entity<UserProgress>().HasIndex(p => p.UserId).IsUnique();
        builder.Entity<MoneyJourney>().HasIndex(j => j.UserId).IsUnique();
        builder.Entity<Scenario>().HasIndex(s => s.Code).IsUnique();
        builder.Entity<Badge>().HasIndex(b => b.Code).IsUnique();
        builder.Entity<UserChallenge>().HasIndex(c => new { c.UserId, c.ChallengeId }).IsUnique();
        builder.Entity<UserBadge>().HasIndex(b => new { b.UserId, b.BadgeId }).IsUnique();

        builder.Entity<MoneyJourney>()
            .HasOne(j => j.CurrentScenario)
            .WithMany()
            .HasForeignKey(j => j.CurrentScenarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<MoneyJourney>()
            .HasOne(j => j.PendingLifeEvent)
            .WithMany()
            .HasForeignKey(j => j.PendingLifeEventId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<LifeEvent>()
            .HasOne(e => e.Scenario)
            .WithMany()
            .HasForeignKey(e => e.ScenarioId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
