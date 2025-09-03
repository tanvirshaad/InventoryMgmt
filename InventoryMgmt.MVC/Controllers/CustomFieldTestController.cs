using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryMgmt.MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomFieldTestController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public CustomFieldTestController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // Test endpoint for direct field saving without JS
        // Example: GET /api/customfieldtest/testfield/5
        [HttpGet("testfield/{inventoryId}")]
        public async Task<IActionResult> TestAddField(int inventoryId)
        {
            if (inventoryId <= 0)
            {
                return BadRequest("Invalid inventory ID");
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"TestAddField called for inventory {inventoryId}");

                // Create a test field
                var testField = new CustomFieldData
                {
                    Id = "testfield-" + DateTime.Now.Ticks,
                    Type = "text",
                    Name = "Test Field " + DateTime.Now.ToString("HH:mm:ss"),
                    Description = "Test field created directly from API",
                    ShowInTable = true,
                    Order = 0
                };
                
                // Create a list with just this one field
                var fields = new List<CustomFieldData> { testField };
                
                // Save it
                var result = await _inventoryService.CustomFieldService.UpdateCustomFieldsAsync(inventoryId, fields);
                
                return Ok(new { 
                    success = result, 
                    message = result ? "Field added successfully" : "Failed to add field",
                    field = testField 
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in TestAddField: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
