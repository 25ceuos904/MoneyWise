using MoneyWise.Services;

namespace MoneyWise.Tests;

public class GameRulesTests
{
    [Fact]
    public void AgeBandsCapTheJourney()
    {
        Assert.Equal(3, GameRules.MaxStage("10-12"));
        Assert.Equal(5, GameRules.MaxStage("13-15"));
        Assert.Equal(7, GameRules.MaxStage("16-18"));
    }

    [Fact]
    public void LevelsFollowXpThresholds()
    {
        Assert.Equal(1, GameRules.Level(0));
        Assert.Equal("Smart Saver", GameRules.LevelTitle(80));
        Assert.Equal(7, GameRules.Level(980));
    }

    [Fact]
    public void InrUsesIndianGrouping()
    {
        Assert.Equal("₹2,000", GameRules.Inr(2000));
        Assert.Equal("₹1,00,000", GameRules.Inr(100000));
    }

    [Fact]
    public void SavingPercentUsesBothParts()
    {
        Assert.Equal(25, SimulationService.SavingPercent(1500, 500));
        Assert.Equal(0, SimulationService.SavingPercent(0, 0));
    }
}
