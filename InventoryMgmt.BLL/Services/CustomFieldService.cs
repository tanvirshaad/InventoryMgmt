using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Services
{
    public interface ICustomFieldService
    {
        Task<bool> UpdateCustomFieldsAsync(int inventoryId, List<CustomFieldData> fields);
        Task<bool> ClearAllCustomFieldsAsync(int inventoryId);
        Task<Inventory?> GetRawInventoryDataAsync(int id);
    }

    public class CustomFieldService : ICustomFieldService
    {
        private readonly DataAccess _dataAccess;
        private readonly ICustomFieldProcessor _fieldProcessor;

        public CustomFieldService(DataAccess dataAccess, ICustomFieldProcessor fieldProcessor)
        {
            _dataAccess = dataAccess;
            _fieldProcessor = fieldProcessor;
        }

        public async Task<bool> UpdateCustomFieldsAsync(int inventoryId, List<CustomFieldData> fields)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateCustomFieldsAsync called for inventory {inventoryId} with {fields?.Count ?? 0} fields");

            try
            {
                if (inventoryId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Invalid inventory ID: {inventoryId}");
                    return false;
                }

                if (fields == null)
                {
                    fields = new List<CustomFieldData>();
                    System.Diagnostics.Debug.WriteLine("WARNING: fields parameter was null, created empty list");
                }

                var inventory = await _dataAccess.InventoryData.GetByIdAsync(inventoryId);
                if (inventory == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Inventory with ID {inventoryId} not found");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Found inventory: ID={inventory.Id}, Title={inventory.Title}, OwnerId={inventory.OwnerId}");

                // Process field updates using the field processor
                _fieldProcessor.ProcessFieldUpdates(inventory, fields);

                inventory.UpdatedAt = DateTime.UtcNow;
                _dataAccess.InventoryData.Update(inventory);

                System.Diagnostics.Debug.WriteLine("Saving changes to database");
                await _dataAccess.InventoryData.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Changes saved successfully");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UpdateCustomFieldsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> ClearAllCustomFieldsAsync(int inventoryId)
        {
            System.Diagnostics.Debug.WriteLine($"ClearAllCustomFieldsAsync called for inventory {inventoryId}");

            try
            {
                if (inventoryId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Invalid inventory ID: {inventoryId}");
                    return false;
                }

                var inventory = await _dataAccess.InventoryData.GetByIdAsync(inventoryId);
                if (inventory == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Inventory with ID {inventoryId} not found");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Found inventory: ID={inventory.Id}, Title={inventory.Title}, OwnerId={inventory.OwnerId}");

                _fieldProcessor.ClearAllFields(inventory);

                inventory.UpdatedAt = DateTime.UtcNow;
                _dataAccess.InventoryData.Update(inventory);

                System.Diagnostics.Debug.WriteLine("Saving changes to database after clearing all fields");
                await _dataAccess.InventoryData.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Changes saved successfully");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ClearAllCustomFieldsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Inventory?> GetRawInventoryDataAsync(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Fetching raw inventory data for ID: {id}");

                var inventory = await _dataAccess.InventoryData.GetByIdAsync(id);

                if (inventory == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inventory with ID {id} not found");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Found inventory: {inventory.Title} (ID: {inventory.Id})");
                LogCurrentFieldState(inventory);

                return inventory;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetRawInventoryDataAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void LogCurrentFieldState(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("Current Field State:");

            // Text fields
            System.Diagnostics.Debug.WriteLine($"TextField1: {(string.IsNullOrEmpty(inventory.TextField1Name) ? "Empty" : inventory.TextField1Name)}");
            System.Diagnostics.Debug.WriteLine($"TextField2: {(string.IsNullOrEmpty(inventory.TextField2Name) ? "Empty" : inventory.TextField2Name)}");
            System.Diagnostics.Debug.WriteLine($"TextField3: {(string.IsNullOrEmpty(inventory.TextField3Name) ? "Empty" : inventory.TextField3Name)}");

            // MultiText fields
            System.Diagnostics.Debug.WriteLine($"MultiTextField1: {(string.IsNullOrEmpty(inventory.MultiTextField1Name) ? "Empty" : inventory.MultiTextField1Name)}");
            System.Diagnostics.Debug.WriteLine($"MultiTextField2: {(string.IsNullOrEmpty(inventory.MultiTextField2Name) ? "Empty" : inventory.MultiTextField2Name)}");
            System.Diagnostics.Debug.WriteLine($"MultiTextField3: {(string.IsNullOrEmpty(inventory.MultiTextField3Name) ? "Empty" : inventory.MultiTextField3Name)}");

            // Numeric fields
            System.Diagnostics.Debug.WriteLine($"NumericField1: {(string.IsNullOrEmpty(inventory.NumericField1Name) ? "Empty" : inventory.NumericField1Name)}");
            System.Diagnostics.Debug.WriteLine($"NumericField2: {(string.IsNullOrEmpty(inventory.NumericField2Name) ? "Empty" : inventory.NumericField2Name)}");
            System.Diagnostics.Debug.WriteLine($"NumericField3: {(string.IsNullOrEmpty(inventory.NumericField3Name) ? "Empty" : inventory.NumericField3Name)}");

            // Boolean fields
            System.Diagnostics.Debug.WriteLine($"BooleanField1: {(string.IsNullOrEmpty(inventory.BooleanField1Name) ? "Empty" : inventory.BooleanField1Name)}");
            System.Diagnostics.Debug.WriteLine($"BooleanField2: {(string.IsNullOrEmpty(inventory.BooleanField2Name) ? "Empty" : inventory.BooleanField2Name)}");
            System.Diagnostics.Debug.WriteLine($"BooleanField3: {(string.IsNullOrEmpty(inventory.BooleanField3Name) ? "Empty" : inventory.BooleanField3Name)}");
        }
    }
}
