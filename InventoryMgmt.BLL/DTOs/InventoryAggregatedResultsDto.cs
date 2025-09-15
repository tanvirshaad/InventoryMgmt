using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.DTOs
{
    public class InventoryAggregatedResultsDto
    {
        public int InventoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ItemCount { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        
        // Custom field definitions
        public List<CustomFieldDefinitionDto> CustomFields { get; set; } = new List<CustomFieldDefinitionDto>();
        
        // Aggregated results
        public List<FieldAggregationResultDto> AggregatedResults { get; set; } = new List<FieldAggregationResultDto>();
    }

    public class CustomFieldDefinitionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // text, multiline, numeric, boolean, document
        public string? Description { get; set; }
        public bool ShowInTable { get; set; }
        public NumericFieldConfigDto? NumericConfig { get; set; } // Only for numeric fields
    }

    public class FieldAggregationResultDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        
        // For numeric fields
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? AverageValue { get; set; }
        public decimal? MedianValue { get; set; }
        
        // For text fields
        public List<TextValueFrequencyDto>? MostCommonValues { get; set; }
        
        // For boolean fields
        public int? TrueCount { get; set; }
        public int? FalseCount { get; set; }
        public double? TruePercentage { get; set; }
    }

    public class TextValueFrequencyDto
    {
        public string Value { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public double Percentage { get; set; }
    }
}