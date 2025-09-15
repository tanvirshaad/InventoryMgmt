using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
// Remove BLL.Services to avoid ambiguity with IAggregationService
using InventoryMgmt.DAL.EF.TableModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace InventoryMgmt.MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryApiController : ControllerBase
    {
        private readonly IApiTokenService _tokenService;
        private readonly IInventoryService _inventoryService;
        private readonly IAggregationService _aggregationService;
        private readonly ILogger<InventoryApiController> _logger;

        public InventoryApiController(
            IApiTokenService tokenService,
            IInventoryService inventoryService,
            IAggregationService aggregationService,
            ILogger<InventoryApiController> logger)
        {
            _tokenService = tokenService;
            _inventoryService = inventoryService;
            _aggregationService = aggregationService;
            _logger = logger;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetInventoryInfo([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { error = "API token is required" });
                }

                // Special handling for test_api_token in development environment
                int? inventoryId = null;
                if (token == "test_api_token" && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    // Use a sample inventory - get the first one
                    var inventories = await _inventoryService.GetLatestInventoriesAsync(1);
                    inventoryId = inventories.FirstOrDefault()?.Id;
                    _logger.LogInformation($"Using test token with inventory ID: {inventoryId}");
                }
                else
                {
                    // Validate the token
                    inventoryId = await _tokenService.GetInventoryIdFromTokenAsync(token);
                }
                
                if (!inventoryId.HasValue)
                {
                    return Unauthorized(new { error = "Invalid API token" });
                }

                // Get inventory information
                var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId.Value);
                if (inventory == null)
                {
                    return NotFound(new { error = "Inventory not found" });
                }

                return Ok(new
                {
                    id = inventory.Id,
                    title = inventory.Title,
                    description = inventory.Description,
                    category = inventory.Category?.Name,
                    isPublic = inventory.IsPublic,
                    createdAt = inventory.CreatedAt,
                    updatedAt = inventory.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory information");
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        [HttpGet("aggregated")]
        public async Task<IActionResult> GetAggregatedInventoryData([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { error = "API token is required" });
                }

                // Special handling for test_api_token in development environment
                int? inventoryId = null;
                if (token == "test_api_token" && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    // Use a sample inventory - get the first one
                    var inventories = await _inventoryService.GetLatestInventoriesAsync(1);
                    inventoryId = inventories.FirstOrDefault()?.Id;
                    _logger.LogInformation($"Using test token with inventory ID: {inventoryId}");
                }
                else
                {
                    // Validate the token
                    inventoryId = await _tokenService.GetInventoryIdFromTokenAsync(token);
                }
                
                if (!inventoryId.HasValue)
                {
                    return Unauthorized(new { error = "Invalid API token" });
                }

                // Get aggregated data
                var aggregatedData = await _aggregationService.GetInventoryAggregatedResultsAsync(inventoryId.Value);

                return Ok(aggregatedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aggregated inventory data");
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        [HttpGet("generate-token/{inventoryId}")]
        public async Task<IActionResult> GenerateToken(int inventoryId)
        {
            try
            {
                // Check if user has access to this inventory
                // This should be replaced with proper authorization logic
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized(new { error = "Authentication required" });
                }

                // Check if inventory exists
                var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
                if (inventory == null)
                {
                    return NotFound(new { error = "Inventory not found" });
                }

                // Check if user has permission (this should be replaced with your authorization logic)
                // For now, we'll just generate the token
                var token = await _tokenService.GenerateTokenForInventoryAsync(inventoryId);

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating API token");
                return StatusCode(500, new { error = "An error occurred while generating the token" });
            }
        }
        
        [HttpGet("get-token/{inventoryId}")]
        public async Task<IActionResult> GetToken(int inventoryId)
        {
            try
            {
                // Check if user has access to this inventory
                // This should be replaced with proper authorization logic
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized(new { error = "Authentication required" });
                }

                // Check if inventory exists
                var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
                if (inventory == null)
                {
                    return NotFound(new { error = "Inventory not found" });
                }

                // Get the token
                var token = await _tokenService.GetTokenForInventoryAsync(inventoryId);

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API token");
                return StatusCode(500, new { error = "An error occurred while retrieving the token" });
            }
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetInventoryItems([FromQuery] string token, [FromQuery] int? page = null, [FromQuery] int? pageSize = null)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { error = "API token is required" });
                }

                // Special handling for test_api_token in development environment
                int? inventoryId = null;
                if (token == "test_api_token" && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    // Use a sample inventory - get the first one
                    var inventories = await _inventoryService.GetLatestInventoriesAsync(1);
                    inventoryId = inventories.FirstOrDefault()?.Id;
                    _logger.LogInformation($"Using test token with inventory ID: {inventoryId}");
                }
                else
                {
                    // Validate the token
                    inventoryId = await _tokenService.GetInventoryIdFromTokenAsync(token);
                }
                
                if (!inventoryId.HasValue)
                {
                    return Unauthorized(new { error = "Invalid API token" });
                }

                // Get inventory items (you'll need to implement this in your ItemService)
                var items = await _inventoryService.GetInventoryItemsAsync(inventoryId.Value, page, pageSize);
                
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory items");
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }
    }
}