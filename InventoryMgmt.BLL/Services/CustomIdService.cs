using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Services
{
    public interface ICustomIdService
    {
        string GenerateCustomId(string format, int sequenceNumber);
        string GenerateAdvancedCustomId(List<CustomIdElement> elements, int sequenceNumber);
        Task<bool> UpdateCustomIdConfigurationAsync(int inventoryId, List<CustomIdElement> elements);
    }

    public class CustomIdService : ICustomIdService
    {
        private readonly DataAccess _dataAccess;
        private readonly Dictionary<string, ICustomIdElementProcessor> _processors;

        public CustomIdService(DataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            _processors = InitializeProcessors();
        }

        public string GenerateCustomId(string format, int sequenceNumber)
        {
            var result = format;
            var random = new Random();

            // Replace placeholders
            result = result.Replace("{SEQUENCE}", sequenceNumber.ToString("D4"));
            result = result.Replace("{RANDOM}", random.Next(1000, 9999).ToString());
            result = result.Replace("{GUID}", Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper());
            result = result.Replace("{DATE}", DateTime.Now.ToString("yyyyMMdd"));
            result = result.Replace("{TIME}", DateTime.Now.ToString("HHmm"));

            return result;
        }

        public string GenerateAdvancedCustomId(List<CustomIdElement> elements, int sequenceNumber)
        {
            var result = new StringBuilder();

            System.Diagnostics.Debug.WriteLine($"Generating advanced custom ID with {elements?.Count ?? 0} elements");

            if (elements == null)
            {
                System.Diagnostics.Debug.WriteLine("Elements list is null");
                return string.Empty;
            }

            foreach (var element in elements.OrderBy(e => e.Order))
            {
                System.Diagnostics.Debug.WriteLine($"Processing element: Type={element.Type}, Value='{element.Value}'");

                if (_processors.TryGetValue(element.Type.ToLower(), out var processor))
                {
                    var generatedValue = processor.ProcessElement(element, sequenceNumber);
                    result.Append(generatedValue);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Unknown element type: {element.Type}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Generated ID: '{result.ToString()}'");
            return result.ToString();
        }

        public async Task<bool> UpdateCustomIdConfigurationAsync(int inventoryId, List<CustomIdElement> elements)
        {
            try
            {
                var inventory = await _dataAccess.InventoryData.GetByIdAsync(inventoryId);
                if (inventory == null) return false;

                var elementsJson = JsonSerializer.Serialize(elements);

                System.Diagnostics.Debug.WriteLine($"Updating custom ID elements for inventory {inventoryId}");
                System.Diagnostics.Debug.WriteLine($"Elements count: {elements.Count}");
                System.Diagnostics.Debug.WriteLine($"JSON to save: {elementsJson}");

                inventory.CustomIdElements = elementsJson;
                inventory.UpdatedAt = DateTime.UtcNow;

                _dataAccess.InventoryData.DetachEntity(inventory);
                _dataAccess.InventoryData.UpdateProperties(inventory, nameof(inventory.CustomIdElements), nameof(inventory.UpdatedAt));

                await _dataAccess.InventoryData.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine("Custom ID elements updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating custom ID elements: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private Dictionary<string, ICustomIdElementProcessor> InitializeProcessors()
        {
            return new Dictionary<string, ICustomIdElementProcessor>
            {
                ["fixed"] = new FixedElementProcessor(),
                ["20-bit random"] = new Random20BitProcessor(),
                ["32-bit random"] = new Random32BitProcessor(),
                ["6-digit random"] = new Random6DigitProcessor(),
                ["9-digit random"] = new Random9DigitProcessor(),
                ["guid"] = new GuidElementProcessor(),
                ["date/time"] = new DateTimeElementProcessor(),
                ["sequence"] = new SequenceElementProcessor()
            };
        }
    }

    public interface ICustomIdElementProcessor
    {
        string ProcessElement(CustomIdElement element, int sequenceNumber);
    }

    public class FixedElementProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            System.Diagnostics.Debug.WriteLine($"Adding fixed value: '{element.Value}'");

            string fixedValue = element.GetCleanValue();

            if (fixedValue.Contains("_") || fixedValue.Any(c => char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherSymbol))
            {
                System.Diagnostics.Debug.WriteLine("SPECIAL CHARACTER DETECTED IN FIXED VALUE");

                foreach (char c in fixedValue)
                {
                    var category = char.GetUnicodeCategory(c);
                    System.Diagnostics.Debug.WriteLine($"Character: '{c}' Unicode: U+{((int)c).ToString("X4")} Category: {category}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(fixedValue);
                var reconstructed = System.Text.Encoding.UTF8.GetString(bytes);

                if (reconstructed != fixedValue)
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Character encoding issue detected!");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Fixed value characters: {string.Join(", ", fixedValue.Select(c => $"{c}(U+{((int)c).ToString("X4")})"))}");

            return fixedValue;
        }
    }

    public class Random20BitProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            var random = new Random();
            var random20Bit = random.Next(0, 1048576); // 2^20
            System.Diagnostics.Debug.WriteLine($"Adding 20-bit random value: {random20Bit}, format: {element.Value}");
            return FormatRandomValue(random20Bit, element.Value);
        }

        private string FormatRandomValue(int value, string format)
        {
            if (string.IsNullOrEmpty(format)) return value.ToString();

            System.Diagnostics.Debug.WriteLine($"FormatRandomValue: format='{format}'");

            string formatSpecifier = string.Empty;
            string suffix = string.Empty;

            if (format.StartsWith("X"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"X(\d+)(.*)");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int digitCount))
                    {
                        formatSpecifier = $"X{digitCount}";
                        suffix = match.Groups[2].Value;
                    }
                }
            }
            else if (format.StartsWith("D"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"D(\d+)(.*)");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int digitCount))
                    {
                        formatSpecifier = $"D{digitCount}";
                        suffix = match.Groups[2].Value;
                    }
                }
            }

            string result;
            if (!string.IsNullOrEmpty(formatSpecifier))
            {
                result = value.ToString(formatSpecifier) + suffix;
                System.Diagnostics.Debug.WriteLine($"Formatted with '{formatSpecifier}' and suffix '{suffix}': {result}");
            }
            else
            {
                result = value.ToString() + format.Substring(1);
                System.Diagnostics.Debug.WriteLine($"Default formatting with appended suffix: {result}");
            }

            return result;
        }
    }

    public class Random32BitProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            var random = new Random();
            var random32Bit = random.Next();
            System.Diagnostics.Debug.WriteLine($"Adding 32-bit random value: {random32Bit}, format: {element.Value}");
            return FormatRandomValue(random32Bit, element.Value);
        }

        private string FormatRandomValue(int value, string format)
        {
            if (string.IsNullOrEmpty(format)) return value.ToString();

            System.Diagnostics.Debug.WriteLine($"FormatRandomValue: format='{format}'");

            string formatSpecifier = string.Empty;
            string suffix = string.Empty;

            if (format.StartsWith("X"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"X(\d+)(.*)");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int digitCount))
                    {
                        formatSpecifier = $"X{digitCount}";
                        suffix = match.Groups[2].Value;
                    }
                }
            }
            else if (format.StartsWith("D"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"D(\d+)(.*)");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int digitCount))
                    {
                        formatSpecifier = $"D{digitCount}";
                        suffix = match.Groups[2].Value;
                    }
                }
            }

            string result;
            if (!string.IsNullOrEmpty(formatSpecifier))
            {
                result = value.ToString(formatSpecifier) + suffix;
                System.Diagnostics.Debug.WriteLine($"Formatted with '{formatSpecifier}' and suffix '{suffix}': {result}");
            }
            else
            {
                result = value.ToString() + format.Substring(1);
                System.Diagnostics.Debug.WriteLine($"Default formatting with appended suffix: {result}");
            }

            return result;
        }
    }

    public class Random6DigitProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            var random = new Random();
            var random6Digit = random.Next(100000, 999999);
            System.Diagnostics.Debug.WriteLine($"Adding 6-digit random value: {random6Digit}, format: {element.Value}");
            return FormatRandomValue(random6Digit, element.Value);
        }

        private string FormatRandomValue(int value, string format)
        {
            if (string.IsNullOrEmpty(format)) return value.ToString();

            System.Diagnostics.Debug.WriteLine($"FormatRandomValue: format='{format}'");

            string formatSpecifier = string.Empty;
            string suffix = string.Empty;

            if (format.StartsWith("X"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"X(\d+)(.*)");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int digitCount))
                    {
                        formatSpecifier = $"X{digitCount}";
                        suffix = match.Groups[2].Value;
                    }
                }
            }
            else if (format.StartsWith("D"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"D(\d+)(.*)");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int digitCount))
                    {
                        formatSpecifier = $"D{digitCount}";
                        suffix = match.Groups[2].Value;
                    }
                }
            }

            string result;
            if (!string.IsNullOrEmpty(formatSpecifier))
            {
                result = value.ToString(formatSpecifier) + suffix;
                System.Diagnostics.Debug.WriteLine($"Formatted with '{formatSpecifier}' and suffix '{suffix}': {result}");
            }
            else
            {
                result = value.ToString() + format.Substring(1);
                System.Diagnostics.Debug.WriteLine($"Default formatting with appended suffix: {result}");
            }

            return result;
        }
    }

    public class Random9DigitProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            var random = new Random();
            var random9Digit = random.Next(100000000, 999999999);
            System.Diagnostics.Debug.WriteLine($"Adding 9-digit random value: {random9Digit}, format: {element.Value}");
            return FormatRandomValue(random9Digit, element.Value);
        }

        private string FormatRandomValue(int value, string format)
        {
            if (string.IsNullOrEmpty(format)) return value.ToString();

            System.Diagnostics.Debug.WriteLine($"FormatRandomValue: format='{format}'");

            string formatSpecifier = string.Empty;
            string suffix = string.Empty;

            if (format.StartsWith("X"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"X(\d+)(.*)");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int digitCount))
                    {
                        formatSpecifier = $"X{digitCount}";
                        suffix = match.Groups[2].Value;
                    }
                }
            }
            else if (format.StartsWith("D"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"D(\d+)(.*)");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int digitCount))
                    {
                        formatSpecifier = $"D{digitCount}";
                        suffix = match.Groups[2].Value;
                    }
                }
            }

            string result;
            if (!string.IsNullOrEmpty(formatSpecifier))
            {
                result = value.ToString(formatSpecifier) + suffix;
                System.Diagnostics.Debug.WriteLine($"Formatted with '{formatSpecifier}' and suffix '{suffix}': {result}");
            }
            else
            {
                result = value.ToString() + format.Substring(1);
                System.Diagnostics.Debug.WriteLine($"Default formatting with appended suffix: {result}");
            }

            return result;
        }
    }

    public class GuidElementProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            var guid = Guid.NewGuid();
            System.Diagnostics.Debug.WriteLine($"Adding GUID value: {guid}, format: {element.Value}");
            return FormatGuid(guid, element.Value);
        }

        private string FormatGuid(Guid guid, string format)
        {
            if (string.IsNullOrEmpty(format)) return guid.ToString("N");

            System.Diagnostics.Debug.WriteLine($"FormatGuid: format='{format}'");

            string formatSpecifier;
            string suffix = string.Empty;

            var match = System.Text.RegularExpressions.Regex.Match(format, @"^([ndbp])(.*)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success)
            {
                formatSpecifier = match.Groups[1].Value.ToUpper();
                suffix = match.Groups[2].Value;
                System.Diagnostics.Debug.WriteLine($"GUID format specifier: {formatSpecifier}, suffix: '{suffix}'");
            }
            else
            {
                formatSpecifier = "N";
                suffix = format;
                System.Diagnostics.Debug.WriteLine($"Using default GUID format specifier: {formatSpecifier}, with suffix: '{suffix}'");
            }

            string result = formatSpecifier.ToUpper() switch
            {
                "N" => guid.ToString("N") + suffix,
                "D" => guid.ToString("D") + suffix,
                "B" => guid.ToString("B") + suffix,
                "P" => guid.ToString("P") + suffix,
                _ => guid.ToString("N") + suffix
            };

            System.Diagnostics.Debug.WriteLine($"Formatted GUID result: '{result}'");
            return result;
        }
    }

    public class DateTimeElementProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            System.Diagnostics.Debug.WriteLine($"Adding date/time value with format: {element.Value}");
            return FormatDateTime(DateTime.Now, element.Value);
        }

        private string FormatDateTime(DateTime dateTime, string format)
        {
            if (string.IsNullOrEmpty(format)) return dateTime.ToString("yyyy");

            System.Diagnostics.Debug.WriteLine($"FormatDateTime: format='{format}'");

            if (format.Contains('_') || format.IndexOfAny(new[] { '-', '/', '\\' }) >= 0)
            {
                System.Diagnostics.Debug.WriteLine("Format contains special characters, doing special handling");

                try
                {
                    var match = System.Text.RegularExpressions.Regex.Match(format, @"([yMdHhms]+)([_\-/\\].*)");
                    if (match.Success)
                    {
                        string dateFormat = match.Groups[1].Value;
                        string suffix = match.Groups[2].Value;

                        string result = dateTime.ToString(dateFormat) + suffix;
                        System.Diagnostics.Debug.WriteLine($"Split into dateFormat='{dateFormat}' and suffix='{suffix}', result='{result}'");
                        return result;
                    }

                    return dateTime.ToString(format);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in date formatting: {ex.Message}");
                    if (format.Contains("yyyy") && format.Contains("_"))
                    {
                        string result = format.Replace("yyyy", dateTime.Year.ToString());
                        System.Diagnostics.Debug.WriteLine($"Manually replaced 'yyyy' with year: {result}");
                        return result;
                    }
                }
            }
            else
            {
                try
                {
                    return dateTime.ToString(format);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in standard date formatting: {ex.Message}");
                }
            }

            return dateTime.ToString("yyyy");
        }
    }

    public class SequenceElementProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            System.Diagnostics.Debug.WriteLine($"Adding sequence value: {sequenceNumber}, format: {element.Value}");
            return FormatSequence(sequenceNumber, element.Value);
        }

        private string FormatSequence(int sequence, string format)
        {
            if (string.IsNullOrEmpty(format)) return sequence.ToString("D3");

            System.Diagnostics.Debug.WriteLine($"FormatSequence: format='{format}'");

            string formatSpecifier = string.Empty;
            string suffix = string.Empty;

            if (format.StartsWith("D"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"D(\d+)(.*)");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int digitCount))
                    {
                        formatSpecifier = $"D{digitCount}";
                        suffix = match.Groups[2].Value;
                    }
                }
            }

            string result;
            if (!string.IsNullOrEmpty(formatSpecifier))
            {
                result = sequence.ToString(formatSpecifier) + suffix;
                System.Diagnostics.Debug.WriteLine($"Sequence formatted with '{formatSpecifier}' and suffix '{suffix}': {result}");
            }
            else
            {
                result = sequence.ToString("D3") + (format.Length > 1 ? format.Substring(1) : "");
                System.Diagnostics.Debug.WriteLine($"Default D3 formatting with appended suffix: {result}");
            }

            return result;
        }
    }
}
