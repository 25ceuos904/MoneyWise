using MoneyWise.Models;

namespace MoneyWise.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Lessons.Any()) return;

        var scenarios = BuildScenarios();
        db.Scenarios.AddRange(scenarios);
        db.Lessons.AddRange(BuildLessons());
        db.Challenges.AddRange(BuildChallenges());
        db.Badges.AddRange(BuildBadges());
        db.InvestmentConcepts.AddRange(BuildConcepts());
        await db.SaveChangesAsync();

        string[] eventCodes = ["phone-repair", "festival", "fake-discount", "friend-borrow", "course-fee", "travel", "moving", "medical", "pocket-month", "invest-month"];
        foreach (var code in eventCodes)
        {
            var scenario = scenarios.First(s => s.Code == code);
            db.LifeEvents.Add(new LifeEvent
            {
                Title = scenario.Title,
                Story = scenario.Story,
                Emoji = scenario.Emoji,
                MinAge = scenario.MinAge,
                Scenario = scenario
            });
        }
        await db.SaveChangesAsync();
    }

    private static ScenarioOption Opt(string label, int bal, int sav, int em, int inv, int score, string why, string tone = "tradeoff", int debt = 0, int goal = 0) => new()
    {
        Label = label,
        BalanceChange = bal,
        SavingsChange = sav,
        EmergencyChange = em,
        InvestmentChange = inv,
        DebtChange = debt,
        GoalChange = goal,
        Score = score,
        Xp = 18,
        ImmediateResult = tone == "nice"
            ? "Your wallet still has room for a surprise."
            : "This choice gives you less room for the next surprise.",
        FutureEffect = sav > 0
            ? "Future you keeps a little more choice."
            : "A later goal or repair may feel tighter.",
        WhyItMatters = why,
        Tone = tone
    };

    private static Scenario Sc(string code, string title, string emoji, string story, int age, int stage, string category, bool life, params ScenarioOption[] options) => new()
    {
        Code = code, Title = title, Emoji = emoji, Story = story, MinAge = age, Stage = stage,
        Category = category, IsLifeEvent = life, Options = options.ToList()
    };

    private static List<Scenario> BuildScenarios() =>
    [
        Sc("pocket-month", "Pocket money arrives", "💰", "You are 14. This month's pocket money is in your hand. A bicycle goal is still waiting.", 10, 1, "Saving", false,
            Opt("Spend ₹1,500 and save ₹500", -1500, 500, 0, 0, 78, "Saving ₹500 is a real start, and you still have money for the month.", "nice", goal: 500),
            Opt("Spend ₹1,800 and save ₹200", -1800, 200, 0, 0, 55, "A small save keeps the habit alive, but the bicycle stays far.", goal: 200),
            Opt("Save ₹1,200 and spend ₹800", -800, 1200, 0, 0, 84, "The goal moves faster. Check that food and travel still fit in the ₹800.", "nice", goal: 1200)),
        Sc("saver-month", "The savings jar", "🏦", "Friends want snacks after school. Your jar is finally starting to look serious.", 10, 2, "Saving", false,
            Opt("Keep the plan and save ₹400", -200, 400, 0, 0, 86, "Saying not this time protects the goal without banning every snack.", "nice", goal: 400),
            Opt("Go out and save nothing", -600, 0, 0, 0, 42, "The outing can be fun. It also pauses the goal for this week."),
            Opt("Move ₹300 into an emergency note", 0, -300, 300, 0, 80, "A separate emergency note is harder to spend by accident.", "nice")),
        Sc("budget-month", "Split the month", "📊", "You have ₹5,000 of virtual money to last until next month.", 10, 3, "Budgeting", false,
            Opt("Spend ₹4,500 and save ₹500", -4500, 500, 0, 0, 52, "You saved 10%. That is a beginning, not a full plan.", goal: 500),
            Opt("Spend ₹3,000 and save ₹2,000", -3000, 2000, 0, 0, 90, "You saved 40% and kept enough for needs. That is a strong month.", "nice", goal: 2000),
            Opt("Spend ₹2,000 and save ₹3,000", -2000, 3000, 0, 0, 74, "A high save rate helps only if ₹2,000 really covers needs.", "nice", goal: 3000)),
        Sc("income-month", "First stipend", "💼", "₹20,000 of virtual take-home pay just arrived. Food, travel, and a recharge are real.", 13, 4, "Planning", false,
            Opt("Spend ₹18,000 and save ₹2,000", -18000, 2000, 0, 0, 48, "The first salary sets a habit. A thin save is easy to outgrow.", goal: 2000),
            Opt("Spend ₹14,000 and save ₹6,000", -14000, 4000, 2000, 0, 92, "Needs fit, and part of the save can start an emergency fund.", "nice", goal: 4000),
            Opt("Save almost everything", -2000, 18000, 0, 0, 60, "A huge save number fails if food and travel do not fit.")),
        Sc("planner-month", "Rent and the rest", "🏠", "You earn ₹20,000. Three rooms are possible. Food and saving still have to fit.", 13, 5, "Budgeting", false,
            Opt("Rent ₹12,000 and save ₹1,000", -19000, 1000, 0, 0, 44, "When rent crowds out saving, a surprise becomes harder."),
            Opt("Rent ₹8,000 and save ₹4,000", -16000, 4000, 0, 0, 90, "A home and a real saving rate can exist together.", "nice", goal: 4000),
            Opt("Share a room and save ₹6,000", -14000, 6000, 0, 0, 86, "A lower rent is powerful when the living situation still works for you.", "nice", goal: 6000)),
        Sc("invest-month", "Money you will not need soon", "📈", "₹3,000 is separate from fees due next month. Where should this virtual amount sit?", 13, 6, "Investing", false,
            Opt("Leave it as cash at home", 0, 0, 0, 0, 48, "Cash is easy to reach and easy to spend or lose."),
            Opt("Park it in a savings or FD-style deposit", -3000, 0, 0, 3000, 86, "A deposit is calmer than one company share. Growth may be modest.", "nice"),
            Opt("Put all of it in one stock idea", -3000, 0, 0, 3000, 40, "One company can fall sharply. This is not a tip to buy any share.")),
        Sc("master-month", "A plan you can repeat", "🏆", "Look at the month you just lived. What do you protect first next month?", 16, 7, "Planning", false,
            Opt("Protect the emergency fund first", -1000, 0, 2000, 0, 90, "A repeatable plan starts with the surprise you cannot schedule.", "nice"),
            Opt("Upgrade lifestyle first", -4000, 0, 0, 0, 46, "An upgrade is allowed. It should not erase the buffer."),
            Opt("Split between goal, buffer, and a small want", -1500, 1500, 1000, 0, 88, "Trade-offs are the skill. Nothing here has to be perfect.", "nice", goal: 1500)),
        Sc("phone-repair", "Your phone stops working", "📱", "Repair cost: ₹5,000. You can use savings, borrow, sell a virtual investment, or wait.", 10, 1, "Planning", true,
            Opt("Use emergency savings", 0, 0, -2000, 0, 88, "A repair is what backup money is for. Refill it next month.", "nice"),
            Opt("Borrow the repair cost", 0, 0, 0, 0, 40, "Borrowing solves today and adds a payment you did not plan.", debt: 5000),
            Opt("Ignore it for now", 0, 0, 0, 0, 36, "Waiting can be fine for a want. A tool you need for school is a need.")),
        Sc("festival", "Festival week", "🎉", "Gifts and sweets could cost ₹1,500. You already promised a small gift for a cousin.", 10, 1, "Spending", true,
            Opt("Spend ₹1,000 and keep ₹500", -1000, 500, 0, 0, 86, "You can celebrate and still land softly after the week.", "nice", goal: 500),
            Opt("Spend the full ₹1,500", -1500, 0, 0, 0, 48, "Festivals matter. A zero balance makes the next week fragile."),
            Opt("Skip every celebration", 0, 1500, 0, 0, 64, "Saving is stronger when the promises you already made still fit.", goal: 1500)),
        Sc("fake-discount", "50% off, today only", "⚠️", "Headphones you like are half price. Your current pair still works.", 10, 2, "Spending", true,
            Opt("Buy them because of the discount", -2000, 0, 0, 0, 44, "A discount is not a reason if you did not need the item."),
            Opt("Buy a simpler pair and save the rest", -1200, 800, 0, 0, 84, "A smaller version of a want is a smart spending skill.", "nice", goal: 800),
            Opt("Leave them and keep the money", 0, 500, 0, 0, 80, "Waiting is powerful when the current pair still works.", "nice", goal: 500)),
        Sc("friend-borrow", "A friend asks for ₹1,000", "🤝", "They want to repay you next week. You also have a goal this month.", 13, 2, "Planning", true,
            Opt("Lend only what you can miss", -500, 0, 0, 0, 72, "Helping is kind. Lend an amount that does not break your own plan.", "nice"),
            Opt("Lend the full ₹1,000", -1000, 0, 0, 0, 50, "If they are late, your goal and emergency money feel it."),
            Opt("Say you cannot this month", 0, 300, 0, 0, 76, "A clear no protects your plan. You can still help in another way.", "nice", goal: 300)),
        Sc("course-fee", "A short course costs ₹8,000", "🎓", "The course could help later. It is not due today, but the seat is limited.", 13, 3, "Planning", true,
            Opt("Pay from savings now", 0, -3000, 0, 0, 70, "Learning can be a need. Check that the emergency fund survives it."),
            Opt("Save toward it for two months", -1000, 2000, 0, 0, 86, "A short delay can turn a rush into a plan.", "nice", goal: 2000),
            Opt("Skip it and spend on a want", -2500, 0, 0, 0, 40, "A want can wait more easily than a course you actually value.")),
        Sc("travel", "Unexpected travel ₹4,000", "🚗", "You need to get home this weekend. This was not in the plan.", 13, 3, "Planning", true,
            Opt("Use the emergency fund", 0, 0, -2000, 0, 90, "This is a textbook use of backup money. Refill it after.", "nice"),
            Opt("Put it on a pay-later plan", 0, 0, 0, 0, 42, "Pay-later is still a loan. Interest can make the trip cost more.", debt: 4000),
            Opt("Ask family and cut wants this month", -1500, 0, 0, 0, 74, "Sharing the cost and shrinking wants keeps debt out of the story.", "nice")),
        Sc("moving", "Rent starts now", "🏠", "A room is available. Income is ₹20,000. The cheap room is shared.", 16, 5, "Budgeting", true,
            Opt("Take the costly private room", -12000, 0, 0, 0, 46, "Privacy has a price. If saving disappears, the room becomes a strain."),
            Opt("Choose the middle rent", -8000, 3000, 1000, 0, 88, "The middle path leaves food, travel, and a buffer.", "nice", goal: 3000),
            Opt("Share and save the difference", -6000, 4000, 2000, 0, 90, "A lower rent is one of the strongest saving tools.", "nice", goal: 4000)),
        Sc("medical", "Clinic visit ₹1,200", "🩺", "You have ₹4,000 for the month and no separate emergency fund yet.", 13, 3, "Risk", true,
            Opt("Pay for care and still save ₹1,000", -3000, 1000, 0, 0, 88, "Health comes first, and a small save starts the next buffer.", "nice", goal: 1000),
            Opt("Skip care and keep the shopping plan", -4000, 0, 0, 0, 30, "A plan should bend for health."),
            Opt("Pay for care and save nothing", -4000, 0, 0, 0, 58, "Paying for care was right. A small leftover save would steady the month."))
    ];

    private static QuizQuestion Q(string prompt, string why, string correct, string wrong1, string wrong2) => new()
    {
        Prompt = prompt,
        Explanation = why,
        Options =
        [
            new() { Text = correct, IsCorrect = true },
            new() { Text = wrong1, IsCorrect = false },
            new() { Text = wrong2, IsCorrect = false }
        ]
    };

    private static Lesson L(string title, string situation, string explain, string category, int age, int stage, QuizQuestion a, QuizQuestion b) => new()
    {
        Title = title, Situation = situation, Explanation = explain, Category = category, MinAge = age, Stage = stage, XpReward = 20,
        Questions = [a, b]
    };

    private static List<Lesson> BuildLessons() =>
    [
        L("Bus fare or stickers", "You have ₹200 before school. The bus is ₹40. Stickers are also ₹40.", "Needs keep the day going. Wants can wait until needs are covered.", "Spending", 10, 1,
            Q("What is more likely a need today?", "Getting to school is part of the day. Stickers can wait.", "Bus fare", "Stickers", "A new game skin"),
            Q("Why sort needs and wants?", "The point is order, not a ban on every want.", "So needs are funded first", "So wants are never allowed", "So saving is useless")),
        L("The envelope", "Meera wants cricket shoes in three months and gets ₹100 a week.", "Saving a little as soon as money arrives is easier than hoping something is left.", "Saving", 10, 1,
            Q("When is a useful time to save?", "Move the save first, before snacks use it.", "When the money arrives", "After it is all spent", "Only if the amount is huge"),
            Q("₹20 a week for a year is about ₹1,040. What is that?", "Small amounts become real when they repeat.", "A habit that adds up", "The same as buying a stock", "Too small to matter")),
        L("Three jars", "Kabir has ₹800 this exam month.", "A budget is a plan: needs, wants, and saving. It is a guide, not a punishment.", "Budgeting", 10, 1,
            Q("A budget is mainly…", "You decide jobs for money before it disappears.", "A plan for money you expect", "A bank loan", "A promise you will be rich"),
            Q("If a want is bigger than the plan, you can…", "Shrink it or wait. Do not skip needs.", "Wait or choose a smaller version", "Ignore the plan", "Stop paying for needs")),
        L("The extra ₹50", "A school session says ₹1,000 in a savings account might earn about ₹50 in a year at 5%. That rate is only an example.", "Interest is extra money connected to time. Savers may earn it. Borrowers usually pay it.", "Saving", 10, 2,
            Q("Interest on savings is…", "Banks may pay interest. Rates differ.", "Extra money a bank may pay you", "A fine for saving", "Always the same everywhere"),
            Q("If you borrow, interest usually means…", "You pay for using someone else's money.", "You may repay more than you borrowed", "The loan is free", "You repay less")),
        L("Festival money", "You set aside ₹1,500 for gifts and sweets.", "A festival can be enjoyable and still leave a little behind.", "Spending", 10, 2,
            Q("Spending all of a festival budget means…", "Nothing is left if a gift costs more than you guessed.", "The next week starts at zero", "You automatically saved", "Inflation stops"),
            Q("Keeping ₹500 back is…", "You still celebrate, with a softer landing.", "A balance of joy and a buffer", "A rule that fun is bad", "The same as investing")),
        L("The repair", "Your cycle needs ₹400 and you have ₹1,000.", "Using savings for a real repair is a success. Ignoring a need is not 'better saving'.", "Planning", 10, 2,
            Q("A good use of backup money is…", "The repair lets you get to school.", "Fixing the cycle", "Snacks until the balance is zero", "A new keychain"),
            Q("Saving every rupee and skipping the repair…", "Needs can come before the saving target.", "Can make school harder", "Is always the strongest plan", "Removes risk")),
        L("Interest on interest", "₹1,000 earns ₹50 and becomes ₹1,050. Next year, interest uses ₹1,050. The rate is an illustration, not a promise.", "Compound interest is interest on earlier interest. Time matters more than a perfect rate.", "Saving", 13, 2,
            Q("Compound interest is calculated on…", "The new total, not only the first deposit.", "The original amount plus interest already earned", "Only money you spent", "A stock tip"),
            Q("Why does starting earlier help?", "More years means more rounds. It does not guarantee a result.", "Time gives compounding more years", "Early savers never lose money", "Banks cancel fees")),
        L("The kirana bill", "The same basket of groceries costs more than it did two years ago.", "Inflation means many prices rise, so the same rupees can buy less later.", "Planning", 13, 3,
            Q("If prices rise, ₹500 next year…", "The number can stay the same while buying power falls.", "May buy less than today", "Always buys more", "Becomes a share"),
            Q("Inflation means…", "It is a rise in prices, not a reason to stop saving.", "Prices of many things tend to rise", "Pocket money doubles", "Saving is useless")),
        L("The glasses", "Riya's glasses break before exams. The family has a separate jar.", "An emergency fund is for surprises, not shopping. Using it for a real surprise is the point.", "Saving", 13, 3,
            Q("A good use of an emergency fund is…", "Medical or repair surprises qualify.", "A surprise doctor visit", "A festival outfit you already wanted", "Daily chips"),
            Q("Why keep it separate?", "It is harder to spend on an ordinary want.", "So ordinary spending does not use it", "So it can never be touched", "So inflation stops")),
        L("Fees next week", "Anika has ₹1,500 for exam fees next month and another pot she will not touch for years.", "Money needed soon should stay easy to reach. Investing can fall in value.", "Investing", 13, 3,
            Q("Money needed next week is usually better…", "Access matters more than a chance of growth.", "Somewhere easy to use", "In one volatile idea", "Spent so it is not idle"),
            Q("Investing means…", "The value can rise or fall. Nothing here is guaranteed.", "Putting money where the value can move", "Guaranteed profit", "Hiding cash")),
        L("UPI at the fest", "Five small phone payments add up to ₹450.", "UPI moves bank money. It is not free money. MoneyWise never connects to UPI.", "Budgeting", 13, 3,
            Q("A UPI payment is…", "The money still leaves the account.", "Real spending from a bank account", "Free cash", "A mutual fund"),
            Q("A safe habit is…", "Never share a PIN or OTP.", "Keep PINs and OTPs private", "Share an OTP with a caller", "Approve every request")),
        L("One year lock", "Gift money is not needed until college forms next year.", "An FD trades easy access for a stated rate. Breaking it early can reduce interest. The rate is not a promise of wealth.", "Investing", 13, 4,
            Q("An FD usually…", "The lock-in is the main idea.", "Locks money for a period", "Is the same as one share", "Can be spent with no rules"),
            Q("Breaking an FD early can…", "Banks often pay less if you leave early.", "Reduce the interest", "Increase the agreed rate", "Turn it into a stock")),
        L("The salary slip", "Dev is offered ₹12,000. The amount that arrives is a little less.", "Budget the take-home amount. If spending rises with every raise, saving stays stuck.", "Planning", 16, 4,
            Q("Take-home pay is…", "Plan with what actually arrives.", "What remains after deductions", "Always higher than the offer", "A type of fund"),
            Q("If income and spending rise together…", "That is lifestyle creep.", "Saving can stay stuck", "Saving automatically improves", "Tax disappears")),
        L("The EMI phone", "A ₹15,000 phone can be ₹1,200 a month. The current phone still works. Interest makes the total higher. This is a lesson, not a bank offer.", "An EMI is a monthly promise, not free money.", "Planning", 16, 5,
            Q("EMI is usually…", "Equated monthly instalments are regular payments.", "A monthly repayment", "Paid once in a lifetime", "Optional whenever you want"),
            Q("Interest on a loan means…", "You may repay more than the shelf price.", "The total can be higher than the price", "The shop gifts you money", "Payments stop after one month")),
        L("Pay later shoes", "Shoes you want cost ₹3,000. A pay-later button is on the screen. You already have ₹3,000 of virtual money.", "A credit limit is borrowing, not income. Paying only the minimum can grow with interest.", "Risk", 16, 5,
            Q("A credit limit is…", "You have to repay what you use.", "Money you can borrow", "Salary paid early", "An emergency fund"),
            Q("Paying only the minimum…", "The unpaid part can become an expensive balance.", "Can leave a balance that grows", "Clears the purchase for free", "Increases savings")),
        L("The hospital story", "A relative's hospital bill was larger than the emergency jar.", "Insurance is protection you pay for, not an investment and not a product recommendation.", "Risk", 16, 5,
            Q("Insurance is mainly for…", "It is a contract for a covered loss.", "Protection against a large covered loss", "Doubling money monthly", "Replacing a budget"),
            Q("A premium is…", "The price of the cover.", "The amount you pay for cover", "A stock tip", "FD interest")),
        L("Many people, one pool", "A class imagines a fund that holds many companies. No real scheme is named.", "A mutual fund pools money and invests by a strategy. Units can fall in value.", "Investing", 16, 6,
            Q("A mutual fund…", "You own units, not a promise.", "Pools money and invests by a strategy", "Guarantees a yearly profit", "Is cash in a drawer"),
            Q("If the investments fall…", "You share the fall.", "Your units can be worth less", "The bank must double it", "Inflation stops")),
        L("₹500 every month", "A pretend SIP is ₹500 a month, not a lump sum. The illustration can still fall.", "A SIP is a habit of investing a fixed amount regularly. It does not remove risk.", "Investing", 16, 6,
            Q("SIP means…", "The schedule is the idea.", "Investing a fixed amount regularly", "A government salary", "Borrowing every month"),
            Q("A SIP guarantees…", "Spreading purchases is not insurance against loss.", "Nothing. The value can still fall", "You never lose money", "An FD rate")),
        L("One company", "In a classroom game a pretend share goes from ₹100 to ₹80.", "A stock is a slice of a company. Prices can rise or fall. This app names no real company.", "Investing", 16, 6,
            Q("Buying a stock means…", "It is ownership, not a deposit.", "You own a small part of a company", "The company owes you fixed interest", "You cannot lose money"),
            Q("Share prices…", "They move with results and buyers.", "Can rise or fall", "Only rise", "Are set by pocket money")),
        L("Five fruits", "A basket of one fruit spoils together. A mix does not depend on one piece.", "Diversification means one result matters less. It does not guarantee profit.", "Risk", 16, 7,
            Q("Diversification means…", "You avoid a single point of failure.", "Spreading money so one result matters less", "Buying as much as possible of one stock", "Keeping every rupee as cash"),
            Q("Does diversification guarantee profit?", "A whole market can still fall.", "No", "Yes", "Only if the rate is high"))
    ];

    private static List<Challenge> BuildChallenges() =>
    [
        new() { Title = "Save 30% of what you hold", Description = "Grow savings and your emergency fund until they are at least 30% of your virtual money.", RuleKey = "save-30", Xp = 25, MinAge = 10 },
        new() { Title = "Sort needs and wants", Description = "Finish the need-or-want card game.", RuleKey = "needs", Xp = 20, MinAge = 10 },
        new() { Title = "Build a ₹30,000 budget", Description = "Allocate a virtual salary across needs, wants, saving, emergency, and investing.", RuleKey = "budget", Xp = 30, MinAge = 13 },
        new() { Title = "Run an investment illustration", Description = "Watch a 12-month simulated path. It is not a forecast.", RuleKey = "invest", Xp = 30, MinAge = 13 },
        new() { Title = "Build a portfolio", Description = "Split ₹10,000 of virtual money across four places.", RuleKey = "portfolio", Xp = 30, MinAge = 13 },
        new() { Title = "Survive 3 events", Description = "Face three random life events in your journey.", RuleKey = "survive-3", Xp = 35, MinAge = 10 },
        new() { Title = "Finish one situation", Description = "Complete any short lesson quiz.", RuleKey = "lesson", Xp = 15, MinAge = 10 },
        new() { Title = "Make one journey choice", Description = "Play a money decision and read why it matters.", RuleKey = "sim", Xp = 15, MinAge = 10 },
        new() { Title = "Start an emergency fund", Description = "Hold at least ₹1,000 in the emergency fund.", RuleKey = "emergency", Xp = 25, MinAge = 10 },
        new() { Title = "Reach a goal", Description = "Fill the goal bar on your current stage.", RuleKey = "goal", Xp = 40, MinAge = 10 },
        new() { Title = "Reach Budget Builder", Description = "Unlock stage 3 of the Money Journey.", RuleKey = "stage-3", Xp = 25, MinAge = 10 },
        new() { Title = "Play a second month", Description = "Advance the journey to month 2.", RuleKey = "month-2", Xp = 20, MinAge = 10 },
        new() { Title = "Finish a choice without new debt", Description = "Make a decision while your debt stays at zero.", RuleKey = "debt-free", Xp = 20, MinAge = 13 },
        new() { Title = "Notice risk", Description = "Lift your risk learning score through decisions.", RuleKey = "risk", Xp = 20, MinAge = 13 },
        new() { Title = "Practice planning", Description = "Lift your planning score through journey choices.", RuleKey = "plan", Xp = 20, MinAge = 10 }
    ];

    private static List<Badge> BuildBadges() =>
    [
        new() { Code = "journey_starter", Title = "Journey Starter", Description = "You made your first money decision.", Icon = "🌱" },
        new() { Code = "first_saver", Title = "First Saver", Description = "You put virtual money aside.", Icon = "💰" },
        new() { Code = "smart_decision", Title = "Smart Decision Maker", Description = "You chose a balanced option.", Icon = "🧠" },
        new() { Code = "budget_builder", Title = "Budget Builder", Description = "You allocated a full month.", Icon = "🏦" },
        new() { Code = "emergency_ready", Title = "Emergency Ready", Description = "You built a ₹1,000 buffer.", Icon = "🛟" },
        new() { Code = "investment_explorer", Title = "Investment Explorer", Description = "You tried a simulated investment.", Icon = "📈" },
        new() { Code = "risk_balancer", Title = "Risk Balancer", Description = "You mixed saving with investing.", Icon = "⚖️" },
        new() { Code = "goal_crusher", Title = "Goal Crusher", Description = "You reached a journey goal.", Icon = "🎯" },
        new() { Code = "streak_7", Title = "7 Day Streak", Description = "You came back seven days in a row.", Icon = "🔥" },
        new() { Code = "money_master", Title = "Money Master", Description = "You reached the top of your journey or a high XP total.", Icon = "🏆" }
    ];

    private static List<InvestmentConcept> BuildConcepts() =>
    [
        new() { Title = "Saving vs investing", MinAge = 10, RiskLevel = "Depends", ReturnCategory = "Saving is steadier", Situation = "Exam fees are due next month. A college fund is five years away.", Explanation = "Money needed soon should stay easy to reach. Money that can wait might be invested, and it can also fall." },
        new() { Title = "Fixed deposit", MinAge = 13, RiskLevel = "Lower than shares", ReturnCategory = "Modest, set by the deposit", Situation = "You will not need ₹5,000 until a known date next year.", Explanation = "An FD locks money for a period. Leaving early can reduce interest. A quoted rate is not a promise of wealth." },
        new() { Title = "Inflation", MinAge = 13, RiskLevel = "Cash loses buying power", ReturnCategory = "Not a product", Situation = "₹100 buys fewer notebooks than it did a few years ago.", Explanation = "When prices rise, the same rupees buy less. That does not mean every rupee should go into a risky asset." },
        new() { Title = "Compound growth", MinAge = 13, RiskLevel = "Depends where money sits", ReturnCategory = "Illustration only", Situation = "₹1,000 earns interest, then next year interest uses the new total.", Explanation = "Growth can build on earlier growth. Losses can shrink the base too. A sample rate is not an offer." },
        new() { Title = "Mutual fund", MinAge = 16, RiskLevel = "Varies by strategy", ReturnCategory = "Uncertain", Situation = "Many people pool money. A strategy decides what the pool buys.", Explanation = "You own units in a fund, not a promise. Values can fall. This app names no real scheme." },
        new() { Title = "SIP", MinAge = 16, RiskLevel = "Same as the investment", ReturnCategory = "Uncertain", Situation = "₹500 goes in every month instead of all at once.", Explanation = "A SIP is a schedule. It does not remove the chance of loss." },
        new() { Title = "Stocks", MinAge = 16, RiskLevel = "Higher", ReturnCategory = "Potentially higher, and possibly negative", Situation = "A classroom share falls from ₹100 to ₹80.", Explanation = "A stock is ownership. Prices can move a lot. MoneyWise recommends no real company." },
        new() { Title = "Risk and return", MinAge = 16, RiskLevel = "They tend to rise together", ReturnCategory = "A range, not a promise", Situation = "Someone offers to double money in a month with no risk.", Explanation = "A high return with no risk is a warning, not a plan." },
        new() { Title = "Diversification", MinAge = 16, RiskLevel = "Lower concentration", ReturnCategory = "Smoother path, not a fixed number", Situation = "One company has a bad quarter. A mix falls less in the same story.", Explanation = "Spreading means one result matters less. It does not guarantee profit." },
        new() { Title = "Liquidity", MinAge = 13, RiskLevel = "Access risk", ReturnCategory = "Not a return", Situation = "You need the money next week, but it is locked.", Explanation = "Liquidity is how quickly you can use the money. A higher hoped-for return often makes access harder." }
    ];
}
