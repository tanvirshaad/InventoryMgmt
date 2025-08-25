namespace InventoryMgmt.BLL.DTOs
{
    public class CustomFieldData
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool ShowInTable { get; set; }
        public int Order { get; set; }
        
        // Extended configurations for different field types
        public NumericFieldConfig? NumericConfig { get; set; }
    }
}
