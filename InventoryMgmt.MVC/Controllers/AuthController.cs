using Microsoft.AspNetCore.Mvc;
using InventoryMgmt.BLL.Services;
using InventoryMgmt.MVC.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace InventoryMgmt.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _authService.LoginAsync(model.Email, model.Password);
                if (result.success)
                {
                    // Get user from JWT claims
                    var jwtClaims = _jwtService.ValidateToken(result.token);
                    if (jwtClaims == null)
                    {
                        ModelState.AddModelError(string.Empty, "Authentication failed.");
                        return View(model);
                    }
                    
                    // Extract user ID from token
                    var userId = jwtClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var userName = jwtClaims.FindFirst(ClaimTypes.Name)?.Value;
                    var userEmail = jwtClaims.FindFirst(ClaimTypes.Email)?.Value;
                    var userRole = jwtClaims.FindFirst(ClaimTypes.Role)?.Value;
                    
                    // Create claims for the user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId ?? "0"),
                        new Claim(ClaimTypes.Name, userName ?? model.Email),
                        new Claim(ClaimTypes.Email, userEmail ?? model.Email)
                    };
                    
                    // Add role claim if available
                    if (!string.IsNullOrEmpty(userRole))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync("Cookies", claimsPrincipal);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.message);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.RegisterAsync(model.UserName, model.Email, model.Password, model.FirstName, model.LastName);
                if (result.success)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please log in.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.message);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }
    }
}
