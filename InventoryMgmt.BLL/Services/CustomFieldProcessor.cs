using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL.EF.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryMgmt.BLL.Services
{
    public interface ICustomFieldProcessor
    {
        void ProcessFieldUpdates(Inventory inventory, List<CustomFieldData> fields);
        void ClearAllFields(Inventory inventory);
    }

    public class CustomFieldProcessor : ICustomFieldProcessor
    {
        private readonly Dictionary<string, IFieldConfiguration> _fieldConfigurations;

        public CustomFieldProcessor()
        {
            _fieldConfigurations = InitializeFieldConfigurations();
        }

        public void ProcessFieldUpdates(Inventory inventory, List<CustomFieldData> fields)
        {
            LogCurrentFieldState(inventory);

            // Create a dictionary of fields by ID for quick lookup
            var fieldDictionary = fields.Where(f => !string.IsNullOrEmpty(f.Id))
                                       .ToDictionary(f => f.Id, f => f);

            // Clear any field that isn't in the request
            foreach (var fieldConfig in _fieldConfigurations)
            {
                if (!fieldDictionary.ContainsKey(fieldConfig.Key))
                {
                    fieldConfig.Value.ClearField(inventory);
                }
            }

            // Now update fields that are in the request
            System.Diagnostics.Debug.WriteLine($"Processing {fields.Count} field configurations");

            foreach (var fieldConfig in _fieldConfigurations)
            {
                if (fieldDictionary.TryGetValue(fieldConfig.Key, out var field))
                {
                    System.Diagnostics.Debug.WriteLine($"Updating field {fieldConfig.Key} with value: {field.Name}");
                    fieldConfig.Value.ApplyField(inventory, field);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No update provided for field {fieldConfig.Key}, preserving existing value");
                }
            }

            LogCurrentFieldState(inventory);
        }

        public void ClearAllFields(Inventory inventory)
        {
            System.Diagnostics.Debug.WriteLine("!!!! RESETTING ALL CUSTOM FIELDS TO NULL !!!!");

            foreach (var fieldConfig in _fieldConfigurations.Values)
            {
                fieldConfig.ClearField(inventory);
            }
        }

        private Dictionary<string, IFieldConfiguration> InitializeFieldConfigurations()
        {
            return new Dictionary<string, IFieldConfiguration>
            {
                // Text fields
                ["text-field-1"] = new TextFieldConfiguration(1),
                ["text-field-2"] = new TextFieldConfiguration(2),
                ["text-field-3"] = new TextFieldConfiguration(3),

                // Numeric fields
                ["numeric-field-1"] = new NumericFieldConfiguration(1),
                ["numeric-field-2"] = new NumericFieldConfiguration(2),
                ["numeric-field-3"] = new NumericFieldConfiguration(3),

                // Boolean fields
                ["boolean-field-1"] = new BooleanFieldConfiguration(1),
                ["boolean-field-2"] = new BooleanFieldConfiguration(2),
                ["boolean-field-3"] = new BooleanFieldConfiguration(3),

                // MultiText fields
                ["multitext-field-1"] = new MultiTextFieldConfiguration(1),
                ["multitext-field-2"] = new MultiTextFieldConfiguration(2),
                ["multitext-field-3"] = new MultiTextFieldConfiguration(3),

                // Document fields
                ["document-field-1"] = new DocumentFieldConfiguration(1),
                ["document-field-2"] = new DocumentFieldConfiguration(2),
                ["document-field-3"] = new DocumentFieldConfiguration(3)
            };
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

    // Interface for field configurations
    public interface IFieldConfiguration
    {
        void ApplyField(Inventory inventory, CustomFieldData field);
        void ClearField(Inventory inventory);
    }

    // Base class for common field operations
    public abstract class BaseFieldConfiguration : IFieldConfiguration
    {
        protected int FieldNumber { get; }

        protected BaseFieldConfiguration(int fieldNumber)
        {
            FieldNumber = fieldNumber;
        }

        public abstract void ApplyField(Inventory inventory, CustomFieldData field);
        public abstract void ClearField(Inventory inventory);

        protected void LogFieldOperation(string fieldType, string operation)
        {
            System.Diagnostics.Debug.WriteLine($"{operation} {fieldType}Field{FieldNumber}");
        }
    }

    // Text Field Configuration
    public class TextFieldConfiguration : BaseFieldConfiguration
    {
        public TextFieldConfiguration(int fieldNumber) : base(fieldNumber) { }

        public override void ApplyField(Inventory inventory, CustomFieldData field)
        {
            LogFieldOperation("Text", "Setting");
            
            switch (FieldNumber)
            {
                case 1:
                    inventory.TextField1Name = field.Name;
                    inventory.TextField1Description = field.Description;
                    inventory.TextField1ShowInTable = field.ShowInTable;
                    break;
                case 2:
                    inventory.TextField2Name = field.Name;
                    inventory.TextField2Description = field.Description;
                    inventory.TextField2ShowInTable = field.ShowInTable;
                    break;
                case 3:
                    inventory.TextField3Name = field.Name;
                    inventory.TextField3Description = field.Description;
                    inventory.TextField3ShowInTable = field.ShowInTable;
                    break;
            }
        }

        public override void ClearField(Inventory inventory)
        {
            LogFieldOperation("Text", "Clearing");
            
            switch (FieldNumber)
            {
                case 1:
                    inventory.TextField1Name = null;
                    inventory.TextField1Description = null;
                    inventory.TextField1ShowInTable = false;
                    break;
                case 2:
                    inventory.TextField2Name = null;
                    inventory.TextField2Description = null;
                    inventory.TextField2ShowInTable = false;
                    break;
                case 3:
                    inventory.TextField3Name = null;
                    inventory.TextField3Description = null;
                    inventory.TextField3ShowInTable = false;
                    break;
            }
        }
    }

    // Numeric Field Configuration
    public class NumericFieldConfiguration : BaseFieldConfiguration
    {
        public NumericFieldConfiguration(int fieldNumber) : base(fieldNumber) { }

        public override void ApplyField(Inventory inventory, CustomFieldData field)
        {
            LogFieldOperation("Numeric", "Setting");
            
            switch (FieldNumber)
            {
                case 1:
                    inventory.NumericField1Name = field.Name;
                    inventory.NumericField1Description = field.Description;
                    inventory.NumericField1ShowInTable = field.ShowInTable;
                    if (field.NumericConfig != null)
                    {
                        inventory.NumericField1IsInteger = field.NumericConfig.IsInteger;
                        inventory.NumericField1MinValue = field.NumericConfig.MinValue;
                        inventory.NumericField1MaxValue = field.NumericConfig.MaxValue;
                    }
                    break;
                case 2:
                    inventory.NumericField2Name = field.Name;
                    inventory.NumericField2Description = field.Description;
                    inventory.NumericField2ShowInTable = field.ShowInTable;
                    if (field.NumericConfig != null)
                    {
                        inventory.NumericField2IsInteger = field.NumericConfig.IsInteger;
                        inventory.NumericField2MinValue = field.NumericConfig.MinValue;
                        inventory.NumericField2MaxValue = field.NumericConfig.MaxValue;
                    }
                    break;
                case 3:
                    inventory.NumericField3Name = field.Name;
                    inventory.NumericField3Description = field.Description;
                    inventory.NumericField3ShowInTable = field.ShowInTable;
                    if (field.NumericConfig != null)
                    {
                        inventory.NumericField3IsInteger = field.NumericConfig.IsInteger;
                        inventory.NumericField3MinValue = field.NumericConfig.MinValue;
                        inventory.NumericField3MaxValue = field.NumericConfig.MaxValue;
                    }
                    break;
            }
        }

        public override void ClearField(Inventory inventory)
        {
            LogFieldOperation("Numeric", "Clearing");
            
            switch (FieldNumber)
            {
                case 1:
                    inventory.NumericField1Name = null;
                    inventory.NumericField1Description = null;
                    inventory.NumericField1ShowInTable = false;
                    inventory.NumericField1IsInteger = false;
                    inventory.NumericField1MinValue = null;
                    inventory.NumericField1MaxValue = null;
                    break;
                case 2:
                    inventory.NumericField2Name = null;
                    inventory.NumericField2Description = null;
                    inventory.NumericField2ShowInTable = false;
                    inventory.NumericField2IsInteger = false;
                    inventory.NumericField2MinValue = null;
                    inventory.NumericField2MaxValue = null;
                    break;
                case 3:
                    inventory.NumericField3Name = null;
                    inventory.NumericField3Description = null;
                    inventory.NumericField3ShowInTable = false;
                    inventory.NumericField3IsInteger = false;
                    inventory.NumericField3MinValue = null;
                    inventory.NumericField3MaxValue = null;
                    break;
            }
        }
    }

    // Boolean Field Configuration
    public class BooleanFieldConfiguration : BaseFieldConfiguration
    {
        public BooleanFieldConfiguration(int fieldNumber) : base(fieldNumber) { }

        public override void ApplyField(Inventory inventory, CustomFieldData field)
        {
            LogFieldOperation("Boolean", "Setting");
            
            switch (FieldNumber)
            {
                case 1:
                    inventory.BooleanField1Name = field.Name;
                    inventory.BooleanField1Description = field.Description;
                    inventory.BooleanField1ShowInTable = field.ShowInTable;
                    break;
                case 2:
                    inventory.BooleanField2Name = field.Name;
                    inventory.BooleanField2Description = field.Description;
                    inventory.BooleanField2ShowInTable = field.ShowInTable;
                    break;
                case 3:
                    inventory.BooleanField3Name = field.Name;
                    inventory.BooleanField3Description = field.Description;
                    inventory.BooleanField3ShowInTable = field.ShowInTable;
                    break;
            }
        }

        public override void ClearField(Inventory inventory)
        {
            LogFieldOperation("Boolean", "Clearing");
            
            switch (FieldNumber)
            {
                case 1:
                    inventory.BooleanField1Name = null;
                    inventory.BooleanField1Description = null;
                    inventory.BooleanField1ShowInTable = false;
                    break;
                case 2:
                    inventory.BooleanField2Name = null;
                    inventory.BooleanField2Description = null;
                    inventory.BooleanField2ShowInTable = false;
                    break;
                case 3:
                    inventory.BooleanField3Name = null;
                    inventory.BooleanField3Description = null;
                    inventory.BooleanField3ShowInTable = false;
                    break;
            }
        }
    }

    // MultiText Field Configuration
    public class MultiTextFieldConfiguration : BaseFieldConfiguration
    {
        public MultiTextFieldConfiguration(int fieldNumber) : base(fieldNumber) { }

        public override void ApplyField(Inventory inventory, CustomFieldData field)
        {
            LogFieldOperation("MultiText", "Setting");
            
            switch (FieldNumber)
            {
                case 1:
                    inventory.MultiTextField1Name = field.Name;
                    inventory.MultiTextField1Description = field.Description;
                    inventory.MultiTextField1ShowInTable = field.ShowInTable;
                    break;
                case 2:
                    inventory.MultiTextField2Name = field.Name;
                    inventory.MultiTextField2Description = field.Description;
                    inventory.MultiTextField2ShowInTable = field.ShowInTable;
                    break;
                case 3:
                    inventory.MultiTextField3Name = field.Name;
                    inventory.MultiTextField3Description = field.Description;
                    inventory.MultiTextField3ShowInTable = field.ShowInTable;
                    break;
            }
        }

        public override void ClearField(Inventory inventory)
        {
            LogFieldOperation("MultiText", "Clearing");
            
            switch (FieldNumber)
            {
                case 1:
                    inventory.MultiTextField1Name = null;
                    inventory.MultiTextField1Description = null;
                    inventory.MultiTextField1ShowInTable = false;
                    break;
                case 2:
                    inventory.MultiTextField2Name = null;
                    inventory.MultiTextField2Description = null;
                    inventory.MultiTextField2ShowInTable = false;
                    break;
                case 3:
                    inventory.MultiTextField3Name = null;
                    inventory.MultiTextField3Description = null;
                    inventory.MultiTextField3ShowInTable = false;
                    break;
            }
        }
    }

    // Document Field Configuration
    public class DocumentFieldConfiguration : BaseFieldConfiguration
    {
        public DocumentFieldConfiguration(int fieldNumber) : base(fieldNumber) { }

        public override void ApplyField(Inventory inventory, CustomFieldData field)
        {
            LogFieldOperation("Document", "Setting");
            
            switch (FieldNumber)
            {
                case 1:
                    inventory.DocumentField1Name = field.Name;
                    inventory.DocumentField1Description = field.Description;
                    inventory.DocumentField1ShowInTable = field.ShowInTable;
                    break;
                case 2:
                    inventory.DocumentField2Name = field.Name;
                    inventory.DocumentField2Description = field.Description;
                    inventory.DocumentField2ShowInTable = field.ShowInTable;
                    break;
                case 3:
                    inventory.DocumentField3Name = field.Name;
                    inventory.DocumentField3Description = field.Description;
                    inventory.DocumentField3ShowInTable = field.ShowInTable;
                    break;
            }
        }

        public override void ClearField(Inventory inventory)
        {
            LogFieldOperation("Document", "Clearing");
            
            switch (FieldNumber)
            {
                case 1:
                    inventory.DocumentField1Name = null;
                    inventory.DocumentField1Description = null;
                    inventory.DocumentField1ShowInTable = false;
                    break;
                case 2:
                    inventory.DocumentField2Name = null;
                    inventory.DocumentField2Description = null;
                    inventory.DocumentField2ShowInTable = false;
                    break;
                case 3:
                    inventory.DocumentField3Name = null;
                    inventory.DocumentField3Description = null;
                    inventory.DocumentField3ShowInTable = false;
                    break;
            }
        }
    }
}
