using System.ComponentModel.DataAnnotations;

namespace MoneyWise.ViewModels;

public class RegisterViewModel
{
    [Required, StringLength(40)]
    public string Name { get; set; } = "Aarav";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = "";

    [Required]
    public string AgeGroup { get; set; } = "13-15";

    [Required]
    public string Experience { get; set; } = "starting";
}

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";
}

public class ProfileEditViewModel
{
    [Required, StringLength(40)]
    public string Name { get; set; } = "";

    [Required]
    public string AgeGroup { get; set; } = "13-15";

    [Required]
    public string Experience { get; set; } = "starting";
}
