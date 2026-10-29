using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoneyWise.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public abstract class GameController : Controller
{
    protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
