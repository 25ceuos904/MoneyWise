using MoneyWise.Models;

namespace MoneyWise.Services;

public record DecisionOutcome(
    int Balance,
    int Savings,
    int Emergency,
    int Investments,
    int Debt,
    int Score,
    int Xp,
    string Headline,
    string Immediate,
    string Future,
    string Why);

public class SimulationService
{
    public DecisionOutcome Apply(MoneyJourney journey, ScenarioOption option)
    {
        journey.VirtualBalance += option.BalanceChange;
        journey.Savings += option.SavingsChange;
        journey.EmergencyFund += option.EmergencyChange;
        journey.InvestmentValue += option.InvestmentChange;
        journey.Debt += option.DebtChange;
        journey.GoalProgress += Math.Max(0, option.GoalChange);
        if (journey.VirtualBalance < 0)
        {
            var need = -journey.VirtualBalance;
            var fromSavings = Math.Min(journey.Savings, need);
            journey.Savings -= fromSavings;
            need -= fromSavings;
            journey.VirtualBalance = 0;
            if (need > 0) journey.Debt += need;
        }
        journey.Savings = Math.Max(0, journey.Savings);
        journey.EmergencyFund = Math.Max(0, journey.EmergencyFund);
        journey.InvestmentValue = Math.Max(0, journey.InvestmentValue);
        journey.Debt = Math.Max(0, journey.Debt);
        journey.GoalProgress = Math.Min(journey.GoalAmount, Math.Max(0, journey.GoalProgress));
        var headline = option.Tone == "nice" ? "Nice move." : "Interesting choice.";
        return new DecisionOutcome(
            journey.VirtualBalance, journey.Savings, journey.EmergencyFund, journey.InvestmentValue,
            journey.Debt, option.Score, option.Xp, headline, option.ImmediateResult, option.FutureEffect, option.WhyItMatters);
    }

    public static int SavingPercent(int spent, int saved)
    {
        var total = spent + saved;
        return total <= 0 ? 0 : (int)Math.Round(saved * 100d / total);
    }
}
