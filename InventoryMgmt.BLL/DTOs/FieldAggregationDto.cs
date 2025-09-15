using System;
using System.Collections.Generic;

namespace InventoryMgmt.BLL.DTOs
{
    public class FieldAggregationDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        
        // For numeric fields
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public decimal? Average { get; set; }
        public decimal? Median { get; set; }
        
        // For text fields
        public Dictionary<string, int> MostFrequent { get; set; } = new Dictionary<string, int>();
        
        // For boolean fields
        public int TrueCount { get; set; }
        public int FalseCount { get; set; }
    }
}