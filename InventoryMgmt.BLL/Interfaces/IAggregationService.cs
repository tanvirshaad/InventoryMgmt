using InventoryMgmt.BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Interfaces
{
    public interface IAggregationService
    {
        /// <summary>
        /// Gets aggregated statistical information for all fields in an inventory
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <returns>Dictionary of field aggregated results</returns>
        Task<Dictionary<string, FieldAggregationDto>> GetInventoryAggregatedFieldsAsync(int inventoryId);

        /// <summary>
        /// Gets a complete aggregated result DTO for an inventory
        /// </summary>
        /// <param name="inventoryId">The inventory ID</param>
        /// <returns>Inventory aggregated results DTO</returns>
        Task<InventoryAggregatedResultsDto> GetInventoryAggregatedResultsAsync(int inventoryId);
    }
}