using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.MVC.Attributes;
using InventoryMgmt.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InventoryMgmt.MVC.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ISalesforceService _salesforceService;

        public UserController(
            IUserService userService, 
            ISalesforceService salesforceService,
            InventoryMgmt.BLL.Interfaces.IAuthorizationService authorizationService) 
            : base(authorizationService)
        {
            _userService = userService;
            _salesforceService = salesforceService;
        }

        [RequireAuthenticated]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [RequireAuthenticated]
        public async Task<IActionResult> SalesforceIntegration()
        {
            var userId = GetCurrentUserId();
            
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            
            if (user == null)
            {
                return NotFound();
            }

            // Pre-populate the form with user data
            var model = new SalesforceAccountDto
            {
                UserId = userId.Value,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email ?? "",
                CompanyName = "" // Default empty as we don't have this in the User model
            };

            return View(model);
        }

        [HttpPost]
        [RequireAuthenticated]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalesforceIntegration(SalesforceAccountDto model)
        {
            var userId = GetCurrentUserId();
            
            if (!userId.HasValue || userId.Value != model.UserId)
            {
                return ForbiddenResult();
            }

            if (ModelState.IsValid)
            {
                var (success, errorMessage) = await _salesforceService.CreateAccountAndContactAsync(model);
                
                if (success)
                {
                    TempData["Success"] = "Your information has been successfully sent to Salesforce!";
                    return RedirectToAction(nameof(Profile));
                }
                else
                {
                    // If we have a specific error message from the service, use it
                    string userMessage = string.IsNullOrEmpty(errorMessage)
                        ? "Failed to create Salesforce account. Please try again later."
                        : $"Salesforce error: {errorMessage}";
                        
                    ModelState.AddModelError("", userMessage);
                }
            }

            return View(model);
        }
        
        [RequireAuthenticated]
        public async Task<IActionResult> TestSalesforceAuth()
        {
            var (success, errorMessage) = await _salesforceService.TestAuthenticationAsync();
            
            if (success)
            {
                TempData["Success"] = "Successfully authenticated with Salesforce!";
            }
            else
            {
                TempData["Error"] = $"Salesforce authentication failed: {errorMessage}";
            }
            
            return RedirectToAction(nameof(Profile));
        }
    }
}
