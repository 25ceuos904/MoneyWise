using Microsoft.AspNetCore.Identity;

namespace MoneyWise.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = "Aarav";
    public string AgeGroup { get; set; } = "13-15";
    public string ExperienceLevel { get; set; } = "starting";
    public UserProgress? Progress { get; set; }
    public MoneyJourney? Journey { get; set; }
}
