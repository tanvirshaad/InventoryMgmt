using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace InventoryMgmt.MVC.Attributes
{
    public class RequireAuthenticatedAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }

    public class RequireAdminAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            var authService = context.HttpContext.RequestServices.GetRequiredService<InventoryMgmt.BLL.Interfaces.IAuthorizationService>();
            var isAdmin = await authService.IsAdminAsync(userId);
            
            if (!isAdmin)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public class RequireInventoryPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly InventoryPermission _requiredPermission;
        private readonly string _inventoryIdParameter;

        public RequireInventoryPermissionAttribute(InventoryPermission requiredPermission, string inventoryIdParameter = "id")
        {
            _requiredPermission = requiredPermission;
            _inventoryIdParameter = inventoryIdParameter;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            int? userId = null;
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                userId = parsedUserId;
            }

            // Get inventory ID from route values
            if (!context.RouteData.Values.TryGetValue(_inventoryIdParameter, out var inventoryIdObj) ||
                !int.TryParse(inventoryIdObj?.ToString(), out int inventoryId))
            {
                context.Result = new BadRequestResult();
                return;
            }

            var authService = context.HttpContext.RequestServices.GetRequiredService<InventoryMgmt.BLL.Interfaces.IAuthorizationService>();
            var permissions = await authService.GetUserInventoryPermissionsAsync(userId, inventoryId);
            
            if (permissions.Permission < _requiredPermission)
            {
                if (userId == null)
                {
                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                }
                else
                {
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
