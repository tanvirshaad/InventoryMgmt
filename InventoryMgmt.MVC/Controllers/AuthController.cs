using Microsoft.AspNetCore.Mvc;
using InventoryMgmt.BLL.Services;
using InventoryMgmt.MVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
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

        [HttpGet]
        public IActionResult GoogleLogin(string returnUrl = null)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl }),
                Items =
                {
                    { "scheme", GoogleDefaults.AuthenticationScheme }
                }
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string returnUrl = null)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("External");
            
            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login");
            }

            // Extract the Google user info from external claims
            var emailClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Email);
            var nameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Name);
            var givenNameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.GivenName);
            var surnameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Surname);
            
            if (emailClaim == null)
            {
                return RedirectToAction("Login");
            }

            // Check if user exists in your database
            var existingUser = await _authService.GetUserByEmailAsync(emailClaim.Value);
            
            if (existingUser == null)
            {
                // Register the new user
                var registerResult = await _authService.RegisterAsync(
                    emailClaim.Value, // Use email as username
                    emailClaim.Value,
                    _jwtService.HashPassword(Guid.NewGuid().ToString()), // Generate a secure random password
                    givenNameClaim?.Value ?? nameClaim?.Value?.Split(' ').FirstOrDefault() ?? string.Empty,
                    surnameClaim?.Value ?? nameClaim?.Value?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty
                );
                
                if (!registerResult.success)
                {
                    ModelState.AddModelError(string.Empty, registerResult.message);
                    return View("Login");
                }
                
                existingUser = await _authService.GetUserByEmailAsync(emailClaim.Value);
            }
            
            if (existingUser != null)
            {
                // Create claims for the user from our database
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, existingUser.UserName),
                    new Claim(ClaimTypes.Email, existingUser.Email),
                    new Claim(ClaimTypes.Role, existingUser.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignOutAsync("External");
                await HttpContext.SignInAsync("Cookies", claimsPrincipal);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
