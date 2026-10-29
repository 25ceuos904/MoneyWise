# MoneyWise - Complete Comprehensive Project Study Guide & Viva Manual

---

## SECTION 1: PROJECT OVERVIEW

### 1.1 What is MoneyWise?
**MoneyWise** is an interactive financial-literacy education platform and simulation game built using **ASP.NET Core 8.0 MVC** and **Entity Framework Core 8.0**. It is specifically designed for school and college students aged **10 to 18**.

The app divides students into three distinct age brackets (10–12, 13–15, and 16–18), tailoring financial decision-making scenarios, income levels, financial goals, educational lessons, minigames, and investment concepts to each group's maturity level.

### 1.2 What Problem Does It Solve?
Most young students learn math and science in school but graduate without basic real-world financial skills—such as distinguishing needs from wants, creating a monthly budget, maintaining an emergency fund, understanding loan interest/EMI, avoiding credit card debt traps, or understanding how investments (like fixed deposits, mutual funds, SIPs, and stocks) grow or fluctuate.

MoneyWise solves this problem by providing a **completely risk-free, sandboxed environment** where every rupee is virtual. Students learn by making financial decisions, seeing immediate and long-term consequences, answering interactive quiz questions, completing challenges, earning badges, and tracking their experience points (XP) and financial literacy scores.

### 1.3 The 1-Minute Presentation Opening Script
> *"Good morning respected evaluators. Today I am presenting **MoneyWise**, an interactive financial-literacy learning platform built with ASP.NET Core 8 MVC and SQL Server LocalDB. MoneyWise transforms abstract financial concepts into a hands-on gamified journey for students aged 10 to 18. Users progress through age-tailored financial stages—managing simulated monthly income, allocating budgets across needs and wants, building emergency savings, handling unexpected life events, and exploring investment tools like FDs, SIPs, and stock portfolios. Built on a clean MVC architecture with Entity Framework Core, ASP.NET Core Identity authentication, and robust server-side validation, MoneyWise ensures students gain practical money-management habits in a completely safe, risk-free environment. I will now walk you through the system architecture, database design, live features, and test coverage."*

---

## SECTION 2: TECHNOLOGY STACK & ARCHITECTURE

### 2.1 Complete Technology Inventory

| Layer / Role | Technology / Library | Version | Purpose & Justification |
| :--- | :--- | :--- | :--- |
| **Backend Framework** | ASP.NET Core MVC | 8.0 | Web application framework providing strong typing, dependency injection, and clean MVC architecture. |
| **Language** | C# | 12.0 | Modern object-oriented language for backend logic, business rules, and service layers. |
| **Database Engine** | SQL Server Express LocalDB | 2019/2022 | Lightweight, developer-focused SQL Server instance running locally (`(localdb)\MSSQLLocalDB`). |
| **ORM / Data Access** | Entity Framework Core | 8.0.11 | Object-Relational Mapper for translating LINQ queries into SQL queries and managing database schemas via migrations. |
| **EF Core SQL Provider** | `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.11 | Database provider enabling EF Core to connect and interact specifically with SQL Server / LocalDB. |
| **EF Core Design Tools** | `Microsoft.EntityFrameworkCore.Design` | 8.0.11 | Tooling required for creating Entity Framework database migrations (`Add-Migration`, `Update-Database`). |
| **Authentication System**| ASP.NET Core Identity | 8.0.11 | Handles secure user authentication, cookie-based session management, password hashing, and user registration. |
| **Identity EF Core Storage**| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.11 | Integrates ASP.NET Core Identity tables (`AspNetUsers`, etc.) directly into `AppDbContext`. |
| **Testing Framework** | xUnit | 2.5.3 | Unit testing framework for writing isolated backend tests for services, game rules, and controllers. |
| **Test Runner** | `Microsoft.NET.Test.Sdk` & `xunit.runner.visualstudio` | 17.8.0 / 2.5.3 | Enables CLI (`dotnet test`) and Visual Studio Test Explorer execution of unit test suites. |
| **In-Memory Db Provider**| `Microsoft.EntityFrameworkCore.InMemory` | 8.0.11 | Fast in-memory database context provider for running isolated EF Core integration tests without SQL Server. |
| **Mocking Library** | Moq | 4.20.70 | Mocking library used in unit tests to simulate HTTP contexts, session state, and `ITempDataDictionary`. |
| **Frontend Rendering** | Razor View Engine (`.cshtml`) | 8.0 | Server-side templating engine for generating dynamic HTML responses embedded with C# data. |
| **Frontend Styling** | Custom CSS3 (`site.css`) | Modern CSS | Custom grid system with CSS variables, sticky top navigation, responsive mobile bottom navbar, and card components. |
| **Client-side Scripting**| Vanilla JavaScript (ES6) | Native | Handles real-time client-side form validation, confirmation modals, and interactive UI toggles without heavy external dependencies. |

### 2.2 Model-View-Controller (MVC) Architecture Explained

MoneyWise follows the **MVC Design Pattern**, which separates the application into three core components:

1. **Model (`Models/`, `Data/`):** Represents the application's data structure, database entities (`ApplicationUser`, `MoneyJourney`, `Scenario`, `Lesson`, etc.), and database context (`AppDbContext`). It encapsulates the state and database operations.
2. **View (`Views/`):** The user interface layer rendered using Razor (`.cshtml`). Views convert data supplied by Controllers into HTML, presenting pages like the Dashboard, Money Journey decisions, Budget minigames, and Quizzes to the browser.
3. **Controller (`Controllers/`):** The central brain handling incoming HTTP requests. Controllers inspect user input, call backend business logic services (`MoneyJourneyService`, `ProgressService`, `SimulationService`), modify data in the Database via EF Core, and return appropriate Views or HTTP Redirects.

### 2.3 End-to-End Request Flow (Browser to DB to View)

Here is the exact step-by-step path when a user submits a decision in the **Money Journey**:

```
[User Clicks Option] 
       │
       ▼
1. Browser sends HTTP POST request to /MoneyJourney/Choose (with optionId and anti-forgery token)
       │
       ▼
2. ASP.NET Core Routing matches URL to MoneyJourneyController.Choose(int optionId, bool fromEvent)
       │
       ▼
3. Anti-Forgery Token Middleware validates security token on the request header/form
       │
       ▼
4. MoneyJourneyController retrieves UserId from ClaimsPrincipal (User.FindFirstValue)
       │
       ▼
5. MoneyJourneyController calls MoneyJourneyService.ChooseAsync(userId, optionId, fromEvent)
       │
       ▼
6. MoneyJourneyService fetches User and MoneyJourney records from AppDbContext via EF Core (SQL SELECT)
       │
       ▼
7. SimulationService.Apply(journey, option) calculates balance changes, savings, debt, and score
       │
       ▼
8. MoneyJourneyService records a new MoneyTransaction and SimulationResult into AppDbContext
       │
       ▼
9. ProgressService.AddXpAsync adds XP, updates category scores, checks challenge completion, and calls BadgeService
       │
       ▼
10. AppDbContext.SaveChangesAsync() commits all pending changes to SQL Server LocalDB in a single transaction (SQL INSERT/UPDATE)
       │
       ▼
11. Controller returns RedirectToAction("Result") or View("Result", outcomeModel)
       │
       ▼
12. Razor View Engine renders Result.cshtml template populated with outcome details and returns HTML to Browser
```

---

## SECTION 3: DATABASE ARCHITECTURE & DESIGN

### 3.1 Database Environment & Connection Details
- **Database Engine:** Microsoft SQL Server Express LocalDB (`(localdb)\MSSQLLocalDB`).
- **Database Name:** `MoneyWiseDB`.
- **Connection String Location:** Configured in [appsettings.json](file:///c:/Users/esha/Downloads/wad%20money%20project%20%281%29/wad%20money%20project/moneywise/appsettings.json):
  ```json
  "ConnectionStrings": {
    "MoneyWiseDB": "Server=(localdb)\\mssqllocaldb;Database=MoneyWiseDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
  ```

### 3.2 Tables and Entity Definitions (`AppDbContext.cs`)

`AppDbContext` extends `IdentityDbContext<ApplicationUser>`, combining default ASP.NET Core Identity authentication tables with 14 custom application tables:

| Entity Name | Database Table Name | Primary Key | Key Columns & Data Types | Relationships |
| :--- | :--- | :--- | :--- | :--- |
| `ApplicationUser` | `AspNetUsers` | `Id` (`nvarchar`) | `DisplayName`, `AgeGroup`, `ExperienceLevel`, `CreatedAt`, `JourneyId` (FK) | 1-to-1 with `MoneyJourney` & `UserProgress` |
| `MoneyJourney` | `MoneyJourneys` | `Id` (`int`) | `UserId` (FK), `MonthlyIncome`, `VirtualBalance`, `Savings`, `EmergencyFund`, `InvestmentValue`, `Debt`, `MonthlyObligation`, `CurrentStage`, `CurrentMonth`, `GoalName`, `GoalAmount`, `GoalProgress`, `CurrentScenarioId` (FK), `PendingLifeEventId` (FK), `AwaitingMonthEnd`, `EventsSurvived`, Minigame Flags | 1-to-1 with `ApplicationUser`, FK to `Scenario` & `LifeEvent` |
| `UserProgress` | `UserProgresses` | `Id` (`int`) | `UserId` (FK), `Xp`, `Streak`, `LastActiveDate`, `SavingScore`, `BudgetingScore`, `SpendingScore`, `InvestingScore`, `RiskScore`, `PlanningScore` | 1-to-1 with `ApplicationUser` |
| `Scenario` | `Scenarios` | `Id` (`int`) | `Code`, `Title`, `Emoji`, `Story`, `MinAge`, `Stage`, `Category`, `IsLifeEvent` | 1-to-Many with `ScenarioOption` |
| `ScenarioOption` | `ScenarioOptions` | `Id` (`int`) | `ScenarioId` (FK), `Label`, `BalanceChange`, `SavingsChange`, `EmergencyChange`, `InvestmentChange`, `DebtChange`, `GoalChange`, `Score`, `Xp`, `ImmediateResult`, `FutureEffect`, `WhyItMatters`, `Tone` | FK to `Scenario` |
| `LifeEvent` | `LifeEvents` | `Id` (`int`) | `Title`, `Story`, `Emoji`, `MinAge`, `ScenarioId` (FK) | FK to `Scenario` |
| `Lesson` | `Lessons` | `Id` (`int`) | `Title`, `Situation`, `Explanation`, `Category`, `MinAge`, `Stage`, `XpReward` | 1-to-Many with `QuizQuestion` |
| `QuizQuestion` | `QuizQuestions` | `Id` (`int`) | `LessonId` (FK), `Prompt`, `Explanation` | FK to `Lesson`, 1-to-Many with `QuizOption` |
| `QuizOption` | `QuizOptions` | `Id` (`int`) | `QuizQuestionId` (FK), `Text`, `IsCorrect` | FK to `QuizQuestion` |
| `QuizResult` | `QuizResults` | `Id` (`int`) | `UserId` (FK), `LessonId` (FK), `Score`, `XpEarned`, `CompletedAt` | FK to `ApplicationUser` & `Lesson` |
| `Challenge` | `Challenges` | `Id` (`int`) | `Title`, `Description`, `RuleKey`, `Xp`, `MinAge` | 1-to-Many with `UserChallenge` |
| `UserChallenge` | `UserChallenges` | `Id` (`int`) | `UserId` (FK), `ChallengeId` (FK), `CompletedAt` | FK to `ApplicationUser` & `Challenge` |
| `Badge` | `Badges` | `Id` (`int`) | `Code`, `Title`, `Description`, `Icon` | 1-to-Many with `UserBadge` |
| `UserBadge` | `UserBadges` | `Id` (`int`) | `UserId` (FK), `BadgeId` (FK), `AwardedAt` | FK to `ApplicationUser` & `Badge` |
| `InvestmentConcept`| `InvestmentConcepts`| `Id` (`int`) | `Title`, `MinAge`, `RiskLevel`, `ReturnCategory`, `Situation`, `Explanation` | Standalone reference content |
| `MoneyTransaction` | `MoneyTransactions` | `Id` (`int`) | `UserId` (FK), `Amount`, `Type`, `Note`, `CreatedAt` | FK to `ApplicationUser` |
| `SimulationResult` | `SimulationResults` | `Id` (`int`) | `UserId` (FK), `ScenarioId` (FK), `ScenarioOptionId` (FK), `Score`, `XpEarned`, `CreatedAt` | FK to `ApplicationUser` |

### 3.3 Text-Based Entity Relationship Diagram (ERD)

```
       ┌─────────────────────────┐
       │     AspNetUsers         │
       │ (Id, Email, AgeGroup...)│
       └────┬────────────────┬───┘
            │ 1:1            │ 1:1
            ▼                ▼
  ┌──────────────────┐  ┌──────────────────┐
  │  MoneyJourneys   │  │  UserProgresses  │
  │ (Balance, Stage) │  │ (Xp, Streak...)  │
  └─────────┬────────┘  └──────────────────┘
            │ FK
            ▼
  ┌──────────────────┐      1:N     ┌──────────────────┐
  │    Scenarios     │─────────────►│  ScenarioOptions │
  │ (Title, Stage...)│              │ (Label, Changes) │
  └──────────────────┘              └──────────────────┘

  ┌──────────────────┐      1:N     ┌──────────────────┐      1:N     ┌──────────────────┐
  │     Lessons      │─────────────►│  QuizQuestions   │─────────────►│   QuizOptions    │
  │ (Title, Age...)  │              │ (Prompt, Why...) │              │ (Text, IsCorrect)│
  └──────────────────┘              └──────────────────┘              └──────────────────┘

  ┌──────────────────┐      1:N     ┌──────────────────┐
  │     Badges       │─────────────►│    UserBadges    │◄───────── (AspNetUsers)
  │ (Code, Icon...)  │              │(UserId, Awarded) │
  └──────────────────┘              └──────────────────┘
```

### 3.4 Migrations and Automatic Database Seeding
- **Migrations Pipeline:** Built using Entity Framework Core Migrations (`20261004140903_InitialCreate.cs`).
- **Automatic Execution on Startup:** In [Program.cs](file:///c:/Users/esha/Downloads/wad%20money%20project%20%281%29/wad%20money%20project/moneywise/Program.cs):
  ```csharp
  using (var scope = app.Services.CreateScope())
  {
      var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      db.Database.Migrate(); // Ensures LocalDB database exists and schema is up to date
      await DbSeeder.SeedAsync(db); // Seeds static initial data if database is empty
  }
  ```
- **Seeded Data (`DbSeeder.cs`):**
  - **15 Scenarios** & corresponding options (Stages 1 through 7 across age groups).
  - **10 Random Life Events** (phone repair, festival spending, fake discounts, clinic visits, rent starts, etc.).
  - **20 Educational Lessons** with multiple-choice quiz questions.
  - **15 Challenges** covering savings targets, emergency fund creation, and budgeting minigames.
  - **10 Badges** (`first_saver`, `emergency_ready`, `budget_builder`, `streak_7`, `money_master`, etc.).
  - **10 Investment Concepts** (saving vs investing, FDs, inflation, mutual funds, SIPs, stocks, risk/return, diversification, liquidity).

### 3.5 Viewing & Resetting the Database in Visual Studio
- **Viewing Stored Data:**
  1. Open Visual Studio 2022.
  2. Navigate to **View > SQL Server Object Explorer**.
  3. Expand **(localdb)\MSSQLLocalDB > Databases > MoneyWiseDB > Tables**.
  4. Right-click any table (e.g. `dbo.MoneyJourneys`) and select **View Data**.
- **Resetting Database (Full Clean State):**
  1. Open **Tools > NuGet Package Manager > Package Manager Console**.
  2. Run `Drop-Database`.
  3. Re-run the application (`F5`). EF Core will recreate `MoneyWiseDB` and `DbSeeder` will re-seed fresh data.

---

## SECTION 4: COMPLETE FILE-BY-FILE CODEBASE EXPLANATION

```
wad money project/
│
├── MoneyWise.sln                             ⭐ Root Visual Studio Solution File
├── MoneyWise.slnx                            .NET Solution File (Modern XML format)
├── README.md                                 ⭐ Root Project Documentation
│
├── moneywise/                                ⭐ Main Web Application Project
│   ├── MoneyWise.csproj                      C# Project File (Dependencies & .NET 8 target)
│   ├── Program.cs                            ⭐ Application Startup & Dependency Injection Setup
│   ├── appsettings.json                      Configuration Settings & Database Connection String
│   ├── appsettings.Development.json          Development Environment Overrides
│   ├── README.md                             Inner Web Project README
│   ├── .gitignore                            Git Exclusion Rules
│   │
│   ├── Data/
│   │   ├── AppDbContext.cs                   ⭐ EF Core DbContext (DbSets & Entity Mappings)
│   │   └── DbSeeder.cs                       ⭐ Static Seed Data Seeder (Lessons, Scenarios, Badges)
│   │
│   ├── Migrations/
│   │   ├── 20261004140903_InitialCreate.cs    EF Core Initial Schema Migration
│   │   ├── 20261004140903_InitialCreate.Designer.cs  Auto-generated EF Migration Metadata
│   │   └── AppDbContextModelSnapshot.cs      Snapshot of Current EF Core Data Model
│   │
│   ├── Models/
│   │   ├── ApplicationUser.cs                ⭐ ASP.NET Identity User (AgeGroup, Experience)
│   │   ├── Gamification.cs                   UserProgress, Challenge, UserChallenge, Badge, UserBadge
│   │   ├── Journey.cs                        ⭐ MoneyJourney, LifeEvent, MoneyTransaction, SimulationResult
│   │   ├── Learning.cs                       Lesson, QuizQuestion, QuizOption, QuizResult
│   │   └── Scenarios.cs                      Scenario, ScenarioOption, InvestmentConcept
│   │
│   ├── ViewModels/
│   │   └── AccountViewModels.cs              RegisterViewModel, LoginViewModel, ProfileEditViewModel
│   │
│   ├── Services/
│   │   ├── GameRules.cs                      ⭐ Static Helper Rules (Age floors, max stages, levels, INR format)
│   │   ├── MoneyJourneyService.cs            ⭐ Core Engine for Monthly Journey, Scenarios & Goals
│   │   ├── ProgressService.cs                ⭐ Engine for XP, Level Progress, Challenge Checks & Scores
│   │   ├── SimulationService.cs              ⭐ Math Engine for Choice Outcome Calculation & Balance Updates
│   │   ├── BadgeService.cs                   ⭐ Evaluation Engine for Unlocking Badges
│   │   └── LifeEventService.cs               Random Life Event Picker Service
│   │
│   ├── Controllers/
│   │   ├── AccountController.cs              ⭐ Handles Player Register, Login, and Logout
│   │   ├── GameController.cs                 ⭐ Abstract Base Controller with [Authorize] & Anti-Forgery
│   │   ├── DashboardController.cs            Renders Main Home Dashboard
│   │   ├── MoneyJourneyController.cs         ⭐ Handles Scenario Choices, Life Events, Month Advancement
│   │   ├── LearnController.cs                Handles Lessons Listing, Quiz Execution & Results
│   │   ├── SimulationController.cs           ⭐ Minigames: Need/Want, Budget, EMI, Credit, Insurance
│   │   ├── InvestmentController.cs           ⭐ Minigames: Concepts, Portfolio Split, 12-Month Path
│   │   ├── ChallengeController.cs            Displays Active and Completed Challenges
│   │   ├── ProgressController.cs             Renders XP Progress, Category Scores, Badge Showcases
│   │   ├── ProfileController.cs              ⭐ Handles Profile Editing, Age Group Changes & Full Reset
│   │   └── HomeController.cs                 Landing Page & Global Error Handling
│   │
│   ├── Views/
│   │   ├── _ViewImports.cshtml               Global TagHelpers & Namespace Imports
│   │   ├── _ViewStart.cshtml                 Sets Default Layout Page (`_Layout.cshtml`)
│   │   ├── Account/
│   │   │   ├── Login.cshtml                  Login Form with JS Client-side Validation
│   │   │   └── Register.cshtml               Player Registration Form with JS Validation
│   │   ├── Challenge/Index.cshtml            Challenges Listing Page
│   │   ├── Dashboard/Index.cshtml            Main Player Dashboard (Wallet, Stage, Quick Actions)
│   │   ├── Home/
│   │   │   ├── Index.cshtml                  Public Landing Page
│   │   │   └── Error.cshtml                  Error Display Page
│   │   ├── Investment/
│   │   │   ├── Index.cshtml                  Investment Concepts List
│   │   │   ├── Detail.cshtml                 Investment Concept Detail View
│   │   │   ├── Portfolio.cshtml              Portfolio Split Minigame Form
│   │   │   └── Timeline.cshtml               12-Month Simulated Path Minigame Form
│   │   ├── Learn/
│   │   │   ├── Index.cshtml                  Lessons List View
│   │   │   ├── Play.cshtml                   Interactive Lesson Quiz Form
│   │   │   └── Result.cshtml                 Quiz Score & Explanation Results Page
│   │   ├── MoneyJourney/
│   │   │   ├── Index.cshtml                  Money Journey Main Decision Page
│   │   │   └── Result.cshtml                 Immediate Decision Outcome & Reflection Page
│   │   ├── Profile/Index.cshtml              Player Profile Edit, Reset Form & Stats Summary
│   │   ├── Progress/Index.cshtml             XP Progress Bars, Category Breakdown & Badges Grid
│   │   ├── Shared/
│   │   │   ├── _Layout.cshtml                ⭐ Main HTML Shell (Topbar, Responsive Mobile Nav, Scripts)
│   │   │   └── _Wallet.cshtml                Partial View rendering Virtual Balance & Savings Cards
│   │   └── Simulation/
│   │       ├── Index.cshtml                  Minigames Overview Grid
│   │       ├── Budget.cshtml                 ₹30,000 Monthly Budget Allocator Minigame
│   │       ├── Compound.cshtml               Compound Interest Calculator Simulator
│   │       ├── Credit.cshtml                 Credit Card Limit Minigame
│   │       ├── Emi.cshtml                    Phone Purchase EMI Minigame
│   │       ├── Feedback.cshtml               Minigame Outcome Feedback Display Page
│   │       ├── Inflation.cshtml              Inflation Buying Power Calculator Simulator
│   │       ├── Insurance.cshtml              Health Insurance Policy Minigame
│   │       ├── Locked.cshtml                 Age-restricted Minigame Lock View
│   │       └── Needs.cshtml                  Needs vs. Wants Card Sorting Minigame
│   │
│   └── wwwroot/
│       └── css/site.css                      ⭐ Custom CSS System & Responsive Layout Media Queries
│
└── MoneyWise.Tests/                          ⭐ xUnit Unit Test Project
    ├── MoneyWise.Tests.csproj                Test Project File & Package Dependencies
    ├── GameRulesTests.cs                     Tests for GameRules Helper Logic
    ├── MoneyJourneyServiceTests.cs           ⭐ Tests for MoneyJourney Startup & Stage 4/5 Scenarios
    ├── BadgeServiceTests.cs                  ⭐ Tests for Badge Evaluation & Unlocking
    └── ControllerValidationTests.cs          ⭐ Tests for Negative Input Rejection & Profile Reset
```

---

## SECTION 5: ALL FEATURES & DETAILED FUNCTIONALITIES

### 5.1 Registration & Authentication
- **User Experience:** User enters Name, Email, Password, Age Group (10–12, 13–15, 16–18), and Experience Level.
- **Internal Controller/Service Flow:**
  1. `AccountController.Register` POST receives `RegisterViewModel`.
  2. Calls `MoneyJourneyService.StartFor(user)` to initialize starter Virtual Balance, Income, Goal, and `UserProgress`.
  3. `UserManager.CreateAsync(user, password)` hashes password and saves user into `AspNetUsers`.
  4. `MoneyJourneyService.EnsureScenarioAsync` selects initial Stage 1 scenario.
  5. `SignInManager.SignInAsync` sets authenticated cookie. Redirects to `/Dashboard`.

### 5.2 Age-Tailored Money Journey & Scenarios
- **Age Brackets & Business Rules:**
  - **10–12 Years:** Starter Income: ₹2,000 | Goal: "A bicycle" (₹8,000) | Max Stage: 3
  - **13–15 Years:** Starter Income: ₹4,000 | Goal: "A phone you chose" (₹15,000) | Max Stage: 5
  - **16–18 Years:** Starter Income: ₹20,000 | Goal: "Emergency cushion" (₹30,000) | Max Stage: 7
- **Making a Choice:**
  - User views story decision card with 3 options.
  - Submitting POST to `MoneyJourneyController.Choose` invokes `MoneyJourneyService.ChooseAsync`.
  - `SimulationService.Apply` updates `VirtualBalance`, `Savings`, `EmergencyFund`, `InvestmentValue`, `Debt`, and `GoalProgress`.
  - Records a transaction in `MoneyTransactions` and result in `SimulationResults`.
  - Triggers a random `LifeEvent` for the next turn before month advancement.
- **Advancing the Month:**
  - User clicks **Advance Month**.
  - `MoneyJourneyService.AdvanceMonthAsync` adds net monthly income (`MonthlyIncome - MonthlyObligation`), pays off active debt, increments `CurrentMonth`, advances `CurrentStage` every 2 months (up to `MaxStage`), and assigns new stage goals.

### 5.3 Interactive Educational Lessons & Quizzes
- **Flow:** Renders lessons filtered by `MinAge <= User.AgeGroup`.
- **Quiz Submission (`LearnController.Play` POST):**
  - Evaluates selected option `IsCorrect`.
  - Calculates score percentage.
  - Records `QuizResult`.
  - Calls `ProgressService.AddXpAsync` to reward XP (e.g. +20 XP) and update category score (e.g. Spending).

### 5.4 Minigames & Simulations (`SimulationController` & `InvestmentController`)
1. **Need vs. Want Sorting (`/Simulation/Needs`):** Sorts 8 items (groceries, game, rent, headphones, etc.). Rewards XP based on correct classifications.
2. **Budget Allocator (`/Simulation/Budget`):** Allocates ₹30,000 across Needs, Wants, Savings, Emergency, and Investing. Validates total equals ₹30,000 and all inputs $\ge 0$.
3. **EMI Purchase (`/Simulation/Emi`):** Choose between paying cash, saving, or taking a ₹40,000 EMI plan (+₹2,500 monthly obligation).
4. **Credit Card Limit (`/Simulation/Credit`):** Restrict to age 16+. Teaches minimum payment debt growth vs. full payment.
5. **Health Insurance (`/Simulation/Insurance`):** Restrict to age 16+. Demonstrates how a ₹2,000 premium protects against a ₹20,000 sudden clinic bill.
6. **Portfolio Allocation (`/Investment/Portfolio`):** Splits ₹10,000 across Savings, FDs, Mutual Funds, and Stocks. Validates total equals ₹10,000 and all inputs $\ge 0$.
7. **12-Month Simulated Path (`/Investment/Timeline`):** Runs a randomized path showing stock volatility vs. deposit growth over 12 illustrated months.

### 5.5 How Data is Added, Updated, and Deleted/Reset

| Action Type | Feature | Exact Controller & Method | Entities/Tables Affected | Exact SQL Action Performed |
| :--- | :--- | :--- | :--- | :--- |
| **CREATE (Add)** | Player Registration | `AccountController.Register` [POST] | `AspNetUsers`, `MoneyJourneys`, `UserProgresses` | `INSERT INTO AspNetUsers...`, `INSERT INTO MoneyJourneys...` |
| **CREATE (Add)** | Make Decision / Minigame | `MoneyJourneyController.Choose` [POST] | `MoneyTransactions`, `SimulationResults`, `UserBadges` | `INSERT INTO MoneyTransactions...`, `INSERT INTO SimulationResults...` |
| **UPDATE (Edit)**| Profile Edit | `ProfileController.Index` [POST] | `AspNetUsers` | `UPDATE AspNetUsers SET DisplayName = @name, ExperienceLevel = @exp WHERE Id = @id` |
| **UPDATE (Edit)**| Age Group Change | `ProfileController.Index` [POST] | `AspNetUsers`, `MoneyJourneys` | Updates `AgeGroup`, re-applies starter income/goals, clamps `CurrentStage` to new `MaxStage`. |
| **DELETE / RESET**| Progress Reset | `ProfileController.Reset` [POST] | `UserProgresses`, `MoneyJourneys`, `QuizResults`, `SimulationResults`, `UserBadges`, `MoneyTransactions` | `DELETE FROM QuizResults WHERE UserId = @id; DELETE FROM MoneyTransactions WHERE UserId = @id; UPDATE UserProgresses SET Xp=0, Streak=1...` |

---

## SECTION 6: SECURITY & VALIDATION

### 6.1 Authentication & Authorization
- **ASP.NET Core Identity:** Standard secure authentication framework. User passwords are hashed using **PBKDF2 with HMAC-SHA256**.
- **Cookie Authentication:** On successful login, an encrypted HTTP-only cookie (`.AspNetCore.Identity.Application`) is issued to the browser.
- **Route Protection:** All game controllers inherit from `GameController.cs`, which is decorated with `[Authorize]`. Unauthenticated users attempting to access dashboard, journey, or minigames are automatically redirected to `/Account/Login`.

### 6.2 Anti-Forgery Protection (`[AutoValidateAntiforgeryToken]`)
- Prevents **Cross-Site Request Forgery (CSRF)** attacks.
- Applied globally to all POST actions via `[AutoValidateAntiforgeryToken]` on `GameController.cs` and `AccountController.cs`.
- Every HTML form includes `@Html.AntiForgeryToken()` (or auto-generated hidden `__RequestVerificationToken` input by Razor TagHelpers). The server rejects forged cross-site requests missing a valid token.

### 6.3 Server-Side & Client-Side Validation
- **Server-side Validation:**
  - `ModelState.IsValid` checks data annotations (`[Required]`, `[EmailAddress]`).
  - Controller actions explicitly validate numeric inputs (e.g. `if (needs < 0 || wants < 0...)` in `SimulationController` and `InvestmentController`), rejecting negative inputs even if client-side JS is bypassed.
- **Client-side Validation:**
  - HTML5 attributes (`required`, `type="email"`, `type="number"`, `min="0"`, `minlength="6"`).
  - Form submit event listeners in JavaScript display instant feedback in `<div id="validationSummary">` without unnecessary server roundtrips.

---

## SECTION 7: WHAT WAS FIXED & CHANGED RECENTLY

The codebase underwent a complete quality, bug-fix, and test audit. Here is the exact summary of fixes:

| # | Bug / Deficiency Description | File(s) Modified | Before (Broken Behavior) | After (Fixed Behavior) |
| :--- | :--- | :--- | :--- | :--- |
| 1 | **Age 13–15 Stage 4/5 Fallback Bug** | `Data/DbSeeder.cs` | Stage 4 (`income-month`) and 5 (`planner-month`) had `MinAge = 16`. Age 13–15 players got stuck repeating Stage 1 ("Pocket money"). | Lowered `MinAge` to 13 on Stage 4 and 5 scenarios. Age 13–15 players now smoothly progress through all stages up to Stage 5. |
| 2 | **Negative Input Acceptance Bug** | `Controllers/SimulationController.cs`, `Controllers/InvestmentController.cs`, Razor Views | Form endpoints only checked sum totals (`sum == 30000`), allowing negative values like `-5000` in one box to pass. | Added explicit negative input checks (`< 0`) in controllers and added `min="0"` on HTML inputs in `Budget.cshtml`, `Portfolio.cshtml`, and `Timeline.cshtml`. |
| 3 | **Incomplete Reset Bug** | `Controllers/ProfileController.cs` | `Reset()` cleared XP and scores, but failed to reset `Streak` and `LastActiveDate`. | Updated `Reset()` to set `Streak = 1` and reset `LastActiveDate = DateTime.UtcNow`. |
| 4 | **Age Group Change Inconsistency** | `Controllers/ProfileController.cs` | Changing `AgeGroup` in profile updated the user table but left old stage goals and income intact. | Updated `Index` POST to recalculate starter income, reset stage goals, and clamp `CurrentStage` to the new `MaxStage`. |
| 5 | **Missing Mobile Bottom Navigation** | `Views/Shared/_Layout.cshtml`, `wwwroot/css/site.css` | Mobile navbar only had 5 links, omitting Invest, Challenges, and Progress. | Added all navigation links with active highlighting and horizontal scroll styling in `site.css`. |
| 6 | **Unused / Leftover Flutter Files** | Entire project directory | Directory contained 17 unused Flutter build/config files and unused `App_Data/moneywise.json` / `FormModels.cs`. | Deleted all 17 Flutter leftovers and dead files. Codebase is 100% clean C# ASP.NET Core. |
| 7 | **Missing Solution File & Broken Test Reference** | `MoneyWise.sln`, `MoneyWise.Tests.csproj` | Solution file was missing and `MoneyWise.Tests.csproj` had an incorrect case-sensitive project path (`..\MoneyWise\`). | Fixed project reference to `..\moneywise\` and generated unified `MoneyWise.sln` solution file. |
| 8 | **Limited Test Coverage** | `MoneyWise.Tests/` | Only 4 tests existed for `GameRules` and `SimulationService`. Nothing tested services or controllers. | Expanded test suite to **14 comprehensive unit tests** covering `MoneyJourneyService`, `BadgeService`, and Controller Validation. |

---

## SECTION 8: UNIT TESTING & MANUAL TEST CHECKLIST

### 8.1 Unit Test Suite Overview (`MoneyWise.Tests`)
The test project contains **14 automated unit tests** built with xUnit, `Microsoft.EntityFrameworkCore.InMemory`, and `Moq`:

1. **`GameRulesTests.cs` (4 Tests):** Verifies age group max stages (3, 5, 7), level title thresholds (Level 1 to 7), Indian currency formatting (`₹1,00,000`), and saving percentage calculations.
2. **`MoneyJourneyServiceTests.cs` (3 Tests):** Verifies `StartFor` initialization across all age groups, scenario lookup for Age 13 Stage 4 & 5 scenarios, and monthly balance/income calculations during month advancement.
3. **`BadgeServiceTests.cs` (1 Test):** Verifies `EvaluateAsync` logic for awarding `first_saver`, `emergency_ready`, and `streak_7` badges.
4. **`ControllerValidationTests.cs` (6 Tests):** Verifies negative input rejection in `SimulationController` (Budget), `InvestmentController` (Portfolio & Timeline), and tests full streak/date reset in `ProfileController.Reset`.

### 8.2 How to Run Unit Tests
In Visual Studio: **Test > Run All Tests**, or via CLI:
```bash
cd "wad money project"
dotnet test MoneyWise.sln
```
**Expected Output:** `Passed! - Failed: 0, Passed: 14, Total: 14` (Duration $< 1$ sec).

### 8.3 Manual Browser Testing Checklist

| Step # | Page / Feature | Action / Input | Expected Result | Pass / Fail |
| :--- | :--- | :--- | :--- | :--- |
| 1 | Registration | Register new user (`test@demo.com`, Age 13–15). | Account created, logged in, redirected to `/Dashboard`. Starter balance = ₹4,000. | PASS |
| 2 | Money Journey | Click **Start Decision**, choose Option 1. | Choice outcome displayed, Virtual Balance updated, life event assigned. | PASS |
| 3 | Journey Stage 4/5 | Advance month to Month 7 (Stage 4). | Stage 4 scenario (`income-month`) loads successfully without fallback to Stage 1. | PASS |
| 4 | Budget Minigame | Submit Budget with `-5000` in Needs. | Rejected with message *"Budget amounts cannot be negative."* | PASS |
| 5 | Portfolio Minigame| Submit Portfolio with valid ₹10,000 split. | Accepted, feedback displays risk level & diversification rating. | PASS |
| 6 | Learn Quiz | Complete "Bus fare or stickers" quiz. | Score displayed, XP awarded, progress bar advances. | PASS |
| 7 | Profile Reset | Click **Reset Learning Progress** and confirm. | All progress reset, XP = 0, Streak = 1, redirected to Dashboard. | PASS |
| 8 | Mobile Navigation | Reduce browser width to $< 800px$. | Bottom navbar appears with all 8 menu links accessible. | PASS |

---

## SECTION 9: HOW TO RUN & TROUBLESHOOTING

### 9.1 Step-by-Step Instructions to Run

#### Method A: Running in Visual Studio 2022
1. Open Visual Studio 2022.
2. Select **File > Open > Project/Solution** and select `wad money project\MoneyWise.sln`.
3. Set `moneywise` as the **Startup Project** (right-click `moneywise` > **Set as Startup Project**).
4. Ensure the run profile in the toolbar is set to **https** or **http**.
5. Press **F5** (or click the green **Start Debugging** button).
6. Browser will open automatically at `https://localhost:7132` (or `http://localhost:5142`).

#### Method B: Running via .NET CLI
```bash
cd "C:\projects\wad money project"
dotnet build MoneyWise.sln
cd moneywise
dotnet run
```

### 9.2 Troubleshooting Common Errors

| Symptom / Error | Root Cause | Solution / Fix |
| :--- | :--- | :--- |
| `Cannot open database "MoneyWiseDB"` or SQL network error. | SQL Server LocalDB instance is stopped or not created. | Run `sqllocaldb start MSSQLLocalDB` in terminal. If instance missing, run `sqllocaldb create MSSQLLocalDB`. |
| `File is locked by MoneyWise` or red X on `app.Run()`. | An older instance of `MoneyWise.exe` is still running in the background. | Open PowerShell and run `taskkill /IM MoneyWise.exe /F`, then press F5 again. |
| `Pending model changes` or Migration Error. | Database schema out of sync with EF Core models. | Open Package Manager Console and run `Drop-Database`, then restart the app. |
| HTTPS Certificate Warning. | ASP.NET Core dev certificate untrusted. | Run `dotnet dev-certs https --trust` in terminal or select HTTP run profile in Visual Studio. |

---

## SECTION 10: LIVE PRESENTATION DEMO SCRIPT (5–7 MINUTES)

| Time | Presentation Phase | What to Click & Show on Screen | What to Say (Script) |
| :--- | :--- | :--- | :--- |
| **0:00 - 1:00** | Introduction & Registration | Open landing page (`/`). Click **Start Playing** > **Register**. Enter name, email, select **Age 13–15**. | *"MoneyWise is an ASP.NET Core 8 MVC platform designed to teach financial literacy. Notice how registration sets up age-tailored parameters—for age 13–15, starter income is ₹4,000 with a goal of ₹15,000."* |
| **1:00 - 2:30** | Money Journey & Decision | Click **Journey** > **Make Choice**. Select an option, view immediate result, then click **Advance Month**. | *"Here in the Money Journey, choices directly alter virtual balance, savings, and debt. Every choice logs a financial transaction and evaluates scoring. Notice how life events like phone repairs appear dynamically."* |
| **2:30 - 3:30** | Minigames & Validation Demo | Navigate to **Play > Budget Builder**. Type `-5000` into Needs and click Submit. | *"Notice our robust validation. Attempting to enter negative values triggers both client-side HTML5 constraints and server-side controller rejection, preventing illegal inputs."* |
| **3:30 - 4:30** | Learn Quizzes & Badges | Navigate to **Learn > Bus fare or stickers**. Complete the quiz and view score result. | *"Educational lessons combine situation explanations with multiple-choice quizzes. Completing quizzes awards XP, updates category scores, and unlocks achievements via BadgeService."* |
| **4:30 - 5:30** | Profile & Architecture | Navigate to **Profile**. Point out Age Group selector and Reset button. | *"Under Profile, users can adjust their age group or reset progress. Calling Reset clears transaction logs while resetting streak and active dates cleanly."* |
| **5:30 - 7:00** | Code & Test Walkthrough | Open Visual Studio. Show `MoneyJourneyService.cs`, `DbSeeder.cs`, and run `dotnet test`. | *"Looking at the codebase, we have a clean separation of concerns with Entity Framework Core, ASP.NET Identity, and 14 automated unit tests that pass in under a second."* |

---

## SECTION 11: VIVA VOCE PREPARATION (30 QUESTIONS & ANSWERS)

### Architecture & Framework Questions

#### Q1: What architecture pattern does MoneyWise follow?
**Answer:** MoneyWise follows the Model-View-Controller (MVC) design pattern using ASP.NET Core 8. Controllers process incoming requests, services handle business logic, EF Core models manage database state, and Razor views render HTML.

#### Q2: What is Dependency Injection (DI) and how is it used in this project?
**Answer:** DI is a design pattern where dependencies are injected into classes rather than instantiated directly. In `Program.cs`, services like `MoneyJourneyService`, `SimulationService`, and `BadgeService` are registered using `builder.Services.AddScoped<Service>()` and injected into controller constructors.

#### Q3: What is the difference between `AddScoped`, `AddTransient`, and `AddSingleton`?
**Answer:** `AddScoped` creates one instance per HTTP request; `AddTransient` creates a new instance every time it is requested; `AddSingleton` creates one single instance shared across the entire application lifetime. MoneyWise uses `AddScoped` for database-dependent services.

#### Q4: How does ASP.NET Core Routing work in this app?
**Answer:** Default route mapping is configured in `Program.cs` as `"{controller=Home}/{action=Index}/{id?}"`. For example, a request to `/MoneyJourney/Index` invokes `MoneyJourneyController.Index()`.

#### Q5: What is `Program.cs` responsible for?
**Answer:** `Program.cs` initializes the `WebApplicationBuilder`, registers services in the DI container, configures DbContext and ASP.NET Core Identity, sets up middleware pipelines (StaticFiles, Routing, Authentication, Authorization), executes database migrations/seeding, and starts the web server.

### Database & Entity Framework Core Questions

#### Q6: What ORM is used and what are its benefits?
**Answer:** Entity Framework Core 8.0. It allows interacting with SQL Server using strongly typed C# LINQ queries instead of writing raw SQL strings, providing compile-time type safety and migration tracking.

#### Q7: How does EF Core manage database schema changes?
**Answer:** Through EF Core Migrations. Developers generate C# migration files (`Add-Migration`) that snapshot model changes, which are applied to SQL Server using `db.Database.Migrate()`.

#### Q8: How does the application automatically seed data on startup?
**Answer:** In `Program.cs`, a service scope is created at application startup. If `db.Lessons.Any()` is false, `DbSeeder.SeedAsync(db)` populates scenarios, lessons, quizzes, badges, and investment concepts.

#### Q9: What entity relationships exist in `AppDbContext`?
**Answer:** Key relationships include 1-to-1 between `ApplicationUser` and `MoneyJourney`/`UserProgress`, 1-to-Many between `Scenario` and `ScenarioOption`, and 1-to-Many between `Lesson` and `QuizQuestion`.

#### Q10: How are database transactions handled during choice selection?
**Answer:** In `MoneyJourneyService.ChooseAsync`, updates to `MoneyJourney`, additions to `MoneyTransactions`, and additions to `SimulationResults` occur within the same EF Core context tracking, committed atomically when `await _db.SaveChangesAsync()` is called.

### Authentication, Security & Validation Questions

#### Q11: How is user authentication implemented?
**Answer:** Using ASP.NET Core Identity with EF Core storage. It handles user registration, password hashing (PBKDF2), and issues encrypted HTTP-only auth cookies.

#### Q12: How are routes secured from unauthorized users?
**Answer:** All game controllers inherit from `GameController.cs`, which has the `[Authorize]` attribute. Any unauthenticated request is intercepted and redirected to `/Account/Login`.

#### Q13: What is Cross-Site Request Forgery (CSRF) and how is MoneyWise protected?
**Answer:** CSRF is an attack where an unauthorized website tricks a user's browser into submitting unwanted actions to a trusted site. MoneyWise uses `[AutoValidateAntiforgeryToken]` on controller classes to validate hidden token hashes on all POST forms.

#### Q14: How does validation work for form submissions?
**Answer:** Validation occurs on two levels: client-side (HTML5 attributes and JavaScript event listeners) and server-side (`ModelState.IsValid` and explicit conditional checks in controller actions).

#### Q15: Why are negative numbers blocked on budget and investment forms?
**Answer:** Because minigames only verified total sum constraints. Inputting negative amounts (e.g. `-5000`) allowed users to bypass budget limits. We added explicit `< 0` checks in controllers and `min="0"` on HTML form fields.

### Business Logic & Domain Questions

#### Q16: How does MoneyWise adapt content for different age groups?
**Answer:** `GameRules.AgeFloor(ageGroup)` maps groups to minimum ages (10, 13, 16) and `GameRules.MaxStage(ageGroup)` sets stage limits (3, 5, 7). Queries filter scenarios and lessons using `s.MinAge <= age`.

#### Q17: How is user XP and level calculated?
**Answer:** `GameRules.Level(xp)` maps XP thresholds to 7 levels (Level 1 at 0 XP up to Level 7 at 980 XP). `ProgressService.AddXpAsync` increments XP and evaluates level progress.

#### Q18: What happens when a user changes their Age Group in Profile?
**Answer:** `ProfileController.Index` POST detects the change, clamps `CurrentStage` to the new group's `MaxStage`, recalculates starter income and goals, and updates active scenarios.

#### Q19: What does the Progress Reset feature do?
**Answer:** `ProfileController.Reset` sets XP to 0, resets Category Scores to 20, sets `Streak = 1`, updates `LastActiveDate = DateTime.UtcNow`, re-initializes starter journey values, and deletes historical transactions, quiz results, and badges.

#### Q20: How are badges awarded?
**Answer:** `BadgeService.EvaluateAsync` checks unlocked criteria (e.g. `Savings > 0` for `first_saver`, `Streak >= 7` for `streak_7`) and inserts new records into `UserBadges`.

### Testing & Code Quality Questions

#### Q21: What testing framework is used and how many tests exist?
**Answer:** xUnit framework. The project contains 14 automated unit tests in `MoneyWise.Tests`.

#### Q22: How are database-dependent services unit tested without SQL Server?
**Answer:** Using `Microsoft.EntityFrameworkCore.InMemory`. It creates an isolated in-memory database instance in memory for each test run.

#### Q23: What components are covered by the unit test suite?
**Answer:** `GameRules` methods, `MoneyJourneyService` (startup & scenario resolution), `BadgeService` (badge awarding), and Controller actions (negative input rejection & profile reset).

#### Q24: What is Moq and why is it used in tests?
**Answer:** Moq is a mocking library used to mock ASP.NET Core `HttpContext`, `ClaimsPrincipal`, and `ITempDataDictionary` so controllers can be tested in isolation.

### Tricky & Personal Presentation Questions

#### Q25: Who wrote this code?
**Answer:** *"This project was developed as a collaborative coursework application. I have conducted a thorough codebase audit, fixed critical bugs (such as age group scenario fallbacks and negative input handling), implemented anti-forgery security, expanded the unit test suite, and refactored the project structure."*

#### Q26: Why is SQL Server LocalDB used instead of SQLite or PostgreSQL?
**Answer:** LocalDB provides a zero-configuration, lightweight native SQL Server environment that integrates seamlessly with Visual Studio and EF Core on Windows while maintaining full T-SQL compatibility.

#### Q27: How does the application prevent real monetary risk?
**Answer:** All monetary figures are virtual numbers stored in `MoneyJourneys`. The app explicitly contains no payment gateway integrations, banking APIs, or real stock purchasing logic.

#### Q28: What are the main limitations of the current implementation?
**Answer:** Minigames currently use static randomized formulas for stock timelines rather than real market data feeds, and multi-tenant admin management features are not yet implemented.

#### Q29: What future improvements would you recommend?
**Answer:** Adding multiplayer classroom leaderboards, integrating real historical market data charts, creating downloadable PDF financial literacy certificates, and adding multi-language support.

#### Q30: How would you scale this application for production deployment?
**Answer:** Migrate database to Azure SQL Database, deploy the ASP.NET Core app to Azure App Service, configure Redis for session caching, and implement a CDN for static wwwroot assets.

---

## SECTION 12: CHEAT SHEET (QUICK MEMORIZATION REVISION)

- **App Purpose:** Financial literacy game for age groups 10–12, 13–15, 16–18.
- **Tech Stack:** ASP.NET Core 8.0 MVC, C# 12, EF Core 8.0, SQL Server LocalDB, ASP.NET Identity, xUnit.
- **Connection String:** `appsettings.json` -> `Server=(localdb)\mssqllocaldb;Database=MoneyWiseDB`.
- **Database Tables:** `AspNetUsers`, `MoneyJourneys`, `UserProgresses`, `Scenarios`, `ScenarioOptions`, `Lessons`, `QuizQuestions`, `Badges`, `UserBadges`, `MoneyTransactions`, etc.
- **Key Services:**
  - `MoneyJourneyService`: Handles monthly income, decisions, stage goals.
  - `SimulationService`: Calculates decision balance changes and scores.
  - `ProgressService`: Manages XP, levels, category scores.
  - `BadgeService`: Evaluates badge unlocks.
- **Age Brackets & Caps:**
  - 10–12 yrs: Income ₹2,000 | Goal ₹8,000 | Max Stage 3
  - 13–15 yrs: Income ₹4,000 | Goal ₹15,000 | Max Stage 5
  - 16–18 yrs: Income ₹20,000 | Goal ₹30,000 | Max Stage 7
- **Key Recent Fixes:**
  1. Lowered `MinAge` to 13 on Stage 4/5 scenarios in `DbSeeder.cs`.
  2. Blocked negative inputs (`< 0`) in `SimulationController` & `InvestmentController`.
  3. Reset `Streak = 1` and updated `LastActiveDate` in `ProfileController.Reset`.
  4. Updated `ProfileController.Index` to re-apply starter values on age group change.
  5. Added mobile bottom navigation for all links in `_Layout.cshtml`.
  6. Expanded unit test coverage to 14 passing tests.
- **Security:** ASP.NET Identity (PBKDF2 hashing), `[Authorize]` on `GameController`, `[AutoValidateAntiforgeryToken]`, HTML5 + JS validation.
- **Commands to Know:**
  - Build Solution: `dotnet build MoneyWise.sln`
  - Run App: `dotnet run` (inside `moneywise` folder)
  - Run Tests: `dotnet test MoneyWise.sln`
