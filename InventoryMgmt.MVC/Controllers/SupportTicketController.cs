using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.DAL.Interfaces;
using InventoryMgmt.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InventoryMgmt.MVC.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketController : ControllerBase
    {
        private readonly ISupportTicketService _supportTicketService;
        private readonly IInventoryRepo _inventoryRepo;

        public SupportTicketController(
            ISupportTicketService supportTicketService,
            IInventoryRepo inventoryRepo)
        {
            _supportTicketService = supportTicketService;
            _inventoryRepo = inventoryRepo;
        }

        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] SupportTicketFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string username = User.FindFirstValue(ClaimTypes.Name);
                
                // Get inventory title if an inventory ID was provided
                string inventoryTitle = null;
                if (model.InventoryId.HasValue)
                {
                    var inventory = await _inventoryRepo.GetByIdAsync(model.InventoryId.Value);
                    inventoryTitle = inventory?.Title;
                }

                // Create and upload the ticket
                var result = await _supportTicketService.CreateAndUploadTicketAsync(model, username, inventoryTitle);

                if (result.Success)
                {
                    return Ok(result);
                }
                
                return StatusCode(500, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        
        
        [HttpGet("adminEmails")]
        [Authorize]
        public async Task<IActionResult> GetAdminEmails()
        {
            try
            {
                var emails = await _supportTicketService.GetAdminEmailsAsync();
                return Ok(emails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}