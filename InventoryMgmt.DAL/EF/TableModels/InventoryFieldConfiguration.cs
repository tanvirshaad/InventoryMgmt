using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.EF.TableModels
{
    public class InventoryFieldConfiguration
    {
        public int InventoryId { get; set; }

        // Text fields (up to 3)
        public string? TextField1Title { get; set; }
        public string? TextField1Description { get; set; }
        public bool TextField1ShowInTable { get; set; }

        public string? TextField2Title { get; set; }
        public string? TextField2Description { get; set; }
        public bool TextField2ShowInTable { get; set; }

        public string? TextField3Title { get; set; }
        public string? TextField3Description { get; set; }
        public bool TextField3ShowInTable { get; set; }

        // Multiline text fields (up to 3)
        public string? MultilineTextField1Title { get; set; }
        public string? MultilineTextField1Description { get; set; }
        public bool MultilineTextField1ShowInTable { get; set; }

        public string? MultilineTextField2Title { get; set; }
        public string? MultilineTextField2Description { get; set; }
        public bool MultilineTextField2ShowInTable { get; set; }

        public string? MultilineTextField3Title { get; set; }
        public string? MultilineTextField3Description { get; set; }
        public bool MultilineTextField3ShowInTable { get; set; }

        // Numeric fields (up to 3)
        public string? NumericField1Title { get; set; }
        public string? NumericField1Description { get; set; }
        public bool NumericField1ShowInTable { get; set; }

        public string? NumericField2Title { get; set; }
        public string? NumericField2Description { get; set; }
        public bool NumericField2ShowInTable { get; set; }

        public string? NumericField3Title { get; set; }
        public string? NumericField3Description { get; set; }
        public bool NumericField3ShowInTable { get; set; }

        // Document/Image fields (up to 3)
        public string? DocumentField1Title { get; set; }
        public string? DocumentField1Description { get; set; }
        public bool DocumentField1ShowInTable { get; set; }

        public string? DocumentField2Title { get; set; }
        public string? DocumentField2Description { get; set; }
        public bool DocumentField2ShowInTable { get; set; }

        public string? DocumentField3Title { get; set; }
        public string? DocumentField3Description { get; set; }
        public bool DocumentField3ShowInTable { get; set; }

        // Boolean fields (up to 3)
        public string? BooleanField1Title { get; set; }
        public string? BooleanField1Description { get; set; }
        public bool BooleanField1ShowInTable { get; set; }

        public string? BooleanField2Title { get; set; }
        public string? BooleanField2Description { get; set; }
        public bool BooleanField2ShowInTable { get; set; }

        public string? BooleanField3Title { get; set; }
        public string? BooleanField3Description { get; set; }
        public bool BooleanField3ShowInTable { get; set; }

        public string FieldOrderJson { get; set; } = "[]";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Inventory Inventory { get; set; } = null!;
    }
}
