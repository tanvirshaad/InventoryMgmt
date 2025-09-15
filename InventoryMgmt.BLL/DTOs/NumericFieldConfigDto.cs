using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.DTOs
{
    public class NumericFieldConfigDto
    {
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public int DecimalPlaces { get; set; }
        public decimal Step { get; set; } = 1;
        public bool IsInteger { get; set; }
        public decimal StepValue { get; set; } = 1;
        public string? DisplayFormat { get; set; }
    }
}