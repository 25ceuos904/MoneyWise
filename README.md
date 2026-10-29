# MoneyWise - Financial Literacy Education Platform

MoneyWise is an ASP.NET Core 8 MVC application designed to teach financial literacy to students aged 10 to 18 through simulated real-life scenario choices, lessons, minigames, and investment concepts. All money in MoneyWise is simulated.

---

## Technical Stack & Architecture

- **Framework:** .NET 8.0 ASP.NET Core MVC (Razor Views)
- **Database:** SQL Server Express LocalDB (`(localdb)\MSSQLLocalDB`) with Entity Framework Core 8.0
- **Authentication:** ASP.NET Core Identity with custom `ApplicationUser` entity
- **Testing:** xUnit unit testing framework (`MoneyWise.Tests`) with EF Core In-Memory database integration
- **Styles & UI:** Custom responsive CSS grid system with desktop & mobile navbars

---

## Core Features & Age Tailoring

MoneyWise tailors income, financial goals, available simulation tools, and scenarios based on the player's age group:

- **Ages 10–12 (Pocket Money Stage):**
  - Starter income: ₹2,000 | Initial goal: "A bicycle" (₹8,000)
  - Max Stage: 3 (Pocket Money, Smart Saver, Budget Builder)
  - Content: Need vs. Want sorting, saving habit formation, basic budget minigames.

- **Ages 13–15 (Stipend & Savings Stage):**
  - Starter income: ₹4,000 | Initial goal: "A phone you chose" (₹15,000)
  - Max Stage: 5 (Includes First Income & Financial Planner)
  - Content: Compound growth, inflation awareness, fixed deposits, emergency fund allocation.

- **Ages 16–18 (Salary & Master Stage):**
  - Starter income: ₹20,000 | Initial goal: "Emergency cushion" (₹30,000)
  - Max Stage: 7 (Unlocks Investment Explorer & Money Master)
  - Content: EMI choices, Credit Card limit management, Health Insurance coverage, SIPs & Mutual Funds, Stock market risk/return models.

---

## Solution Structure

```
wad money project/
├── MoneyWise.sln              # Visual Studio Solution containing both projects
├── moneywise/                  # ASP.NET Core MVC Web Project
│   ├── Controllers/           # MVC Controllers (Account, Dashboard, Journey, Simulation, etc.)
│   ├── Data/                  # AppDbContext & DbSeeder
│   ├── Models/                # Domain Entities (User, Journey, Scenario, Quiz, Badge, etc.)
│   ├── Services/              # Core Services (MoneyJourneyService, ProgressService, BadgeService, etc.)
│   ├── Views/                 # Razor Views & Layouts
│   └── wwwroot/               # Static assets & CSS
└── MoneyWise.Tests/           # xUnit Test Suite
    ├── GameRulesTests.cs
    ├── MoneyJourneyServiceTests.cs
    ├── BadgeServiceTests.cs
    └── ControllerValidationTests.cs
```

---

## How to Setup and Run

### Prerequisites
1. .NET 8.0 SDK installed
2. SQL Server Express 2019/2022 LocalDB installed (`sqllocaldb`)

### Running via Visual Studio 2022
1. Open `MoneyWise.sln` in Visual Studio 2022.
2. Ensure `moneywise` is set as the Startup Project.
3. Press **F5** or click **Start Debugging** (`https` or `http` profile).
4. The database `MoneyWiseDB` will automatically migrate and seed on first run.
5. Navigate to `https://localhost:7132` (or `http://localhost:5142`) in your browser.

### Running via .NET CLI
```bash
cd "wad money project"
dotnet build MoneyWise.sln
cd moneywise
dotnet run
```

### Running Unit Tests
```bash
cd "wad money project"
dotnet test MoneyWise.sln
```

---

## License & Data Privacy
MoneyWise is built solely for educational purposes. No real money, bank accounts, or financial transactions are connected or stored.
