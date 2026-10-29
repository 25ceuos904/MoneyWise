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

## How to Setup and Run

### Running via .NET CLI
```bash
dotnet run
```

### Running Unit Tests
```bash
dotnet test ..\MoneyWise.sln
```
