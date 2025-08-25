using System;

namespace InventoryMgmt.BLL.DTOs
{
    public class NumericFieldConfig
    {
        public bool IsInteger { get; set; } = false;
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal StepValue { get; set; } = 0.01m;
        public string DisplayFormat { get; set; } = string.Empty;
    }
}
