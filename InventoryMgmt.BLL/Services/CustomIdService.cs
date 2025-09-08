using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL;
using Microsoft.EntityFrameworkCore;
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
        Task<bool> IsCustomIdUniqueInInventoryAsync(int inventoryId, string customId, int? excludeItemId = null);
        Task<string> GenerateUniqueCustomIdAsync(int inventoryId);
        Task<int> GetNextSequenceNumberAsync(int inventoryId);
        bool ValidateCustomIdFormat(string customId, List<CustomIdElement> elements);
        string GetCustomIdValidationErrorMessage(string customId, List<CustomIdElement> elements);
        string GetFormatExample(List<CustomIdElement> elements);
        Task<string> GenerateValidCustomIdExampleAsync(int inventoryId);
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

            if (elements == null)
            {
                return string.Empty;
            }

            foreach (var element in elements.OrderBy(e => e.Order))
            {
                if (_processors.TryGetValue(element.Type.ToLower(), out var processor))
                {
                    var generatedValue = processor.ProcessElement(element, sequenceNumber);
                    result.Append(generatedValue);
                }
            }

            return result.ToString();
        }

        public async Task<bool> UpdateCustomIdConfigurationAsync(int inventoryId, List<CustomIdElement> elements)
        {
            try
            {
                var inventory = await _dataAccess.InventoryData.GetByIdAsync(inventoryId);
                if (inventory == null) return false;

                var elementsJson = JsonSerializer.Serialize(elements);
                inventory.CustomIdElements = elementsJson;
                inventory.UpdatedAt = DateTime.UtcNow;

                _dataAccess.InventoryData.DetachEntity(inventory);
                _dataAccess.InventoryData.UpdateProperties(inventory, nameof(inventory.CustomIdElements), nameof(inventory.UpdatedAt));

                await _dataAccess.InventoryData.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Checks if a custom ID is unique within the scope of an inventory
        /// </summary>
        /// <param name="inventoryId">The inventory ID to check within</param>
        /// <param name="customId">The custom ID to validate</param>
        /// <param name="excludeItemId">Optional item ID to exclude from the check (for updates)</param>
        /// <returns>True if the custom ID is unique within the inventory</returns>
        public async Task<bool> IsCustomIdUniqueInInventoryAsync(int inventoryId, string customId, int? excludeItemId = null)
        {
            return await _dataAccess.ItemData.IsCustomIdUniqueInInventoryAsync(inventoryId, customId, excludeItemId);
        }

        /// <summary>
        /// Generates a unique custom ID for an inventory using its current format configuration
        /// </summary>
        /// <param name="inventoryId">The inventory ID to generate a custom ID for</param>
        /// <returns>A unique custom ID for the inventory</returns>
        public async Task<string> GenerateUniqueCustomIdAsync(int inventoryId)
        {
            var inventory = await _dataAccess.InventoryData.GetByIdAsync(inventoryId);
            if (inventory == null)
                throw new ArgumentException($"Inventory with ID {inventoryId} not found");

            var nextSequence = await GetNextSequenceNumberAsync(inventoryId);
            
            string customId;
            bool isUnique = false;
            int attempts = 0;
            const int maxAttempts = 10;
            
            do
            {
                customId = string.IsNullOrEmpty(inventory.CustomIdElements)
                    ? GenerateCustomId(inventory.CustomIdFormat ?? "{SEQUENCE}", nextSequence + attempts)
                    : GenerateAdvancedCustomId(
                        JsonSerializer.Deserialize<List<CustomIdElement>>(inventory.CustomIdElements) ?? new List<CustomIdElement>(),
                        nextSequence + attempts);
                
                isUnique = await IsCustomIdUniqueInInventoryAsync(inventoryId, customId);
                attempts++;
                
                // If we've tried too many times, add a timestamp to ensure uniqueness
                if (attempts >= maxAttempts && !isUnique)
                {
                    customId = $"{customId}_{DateTime.Now.Ticks}";
                    isUnique = true;
                }
            } while (!isUnique && attempts < maxAttempts);

            return customId;
        }

        /// <summary>
        /// Gets the next sequence number for an inventory based on existing items
        /// </summary>
        /// <param name="inventoryId">The inventory ID to get the next sequence for</param>
        /// <returns>The next available sequence number</returns>
        public async Task<int> GetNextSequenceNumberAsync(int inventoryId)
        {
            var items = await _dataAccess.ItemData.FindAsync(i => i.InventoryId == inventoryId);
            return items.Any() ? items.Max(i => i.Id) + 1 : 1;
        }

        /// <summary>
        /// Validates if a custom ID matches the current format configuration of an inventory
        /// </summary>
        /// <param name="customId">The custom ID to validate</param>
        /// <param name="elements">The custom ID format elements</param>
        /// <returns>True if the custom ID is valid according to the format</returns>
        public bool ValidateCustomIdFormat(string customId, List<CustomIdElement> elements)
        {
            if (string.IsNullOrEmpty(customId))
                return false;

            if (elements == null || !elements.Any())
                return !string.IsNullOrWhiteSpace(customId) && customId.Length <= 100; // If no format is defined, any non-empty ID is valid

            // Create a regex pattern to strictly match the expected format
            var pattern = BuildValidationPattern(elements);
            if (string.IsNullOrEmpty(pattern))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(customId, pattern);
        }

        /// <summary>
        /// Builds a regex pattern to validate custom ID format strictly
        /// </summary>
        /// <param name="elements">The custom ID format elements</param>
        /// <returns>A regex pattern for validation</returns>
        private string BuildValidationPattern(List<CustomIdElement> elements)
        {
            // Instead of trying to build a complex regex that matches the exact format,
            // we'll use a more practical approach: generate a test ID and create a pattern
            // that matches the structure, allowing for variations in dynamic content
            
            var sortedElements = elements.OrderBy(e => e.Order).ToList();
            var patternParts = new List<string>();

            foreach (var element in sortedElements)
            {
                var elementType = element.Type?.ToLower();
                var elementValue = element.Value;
                
                switch (elementType)
                {
                    case "fixed":
                        var fixedValue = element.GetCleanValue();
                        if (!string.IsNullOrEmpty(fixedValue))
                        {
                            // Escape special regex characters
                            patternParts.Add(System.Text.RegularExpressions.Regex.Escape(fixedValue));
                        }
                        break;
                        
                    case "20-bit random":
                        // Check if element has a format string that might include separators
                        if (!string.IsNullOrEmpty(elementValue))
                        {
                            // Parse the format to see what pattern it produces
                            var testPattern = GetPatternForRandomElement(elementValue, 6); // Test with 6 digits
                            if (!string.IsNullOrEmpty(testPattern))
                            {
                                patternParts.Add(testPattern);
                            }
                            else
                            {
                                patternParts.Add(@"[0-9A-Fa-f]{1,7}"); // Allow 1-7 hex/decimal chars
                            }
                        }
                        else
                        {
                            patternParts.Add(@"\d{1,7}"); // 20-bit can be 1-7 digits
                        }
                        break;
                        
                    case "32-bit random":
                        // Check if element has a format string
                        if (!string.IsNullOrEmpty(elementValue))
                        {
                            var testPattern = GetPatternForRandomElement(elementValue, 9);
                            if (!string.IsNullOrEmpty(testPattern))
                            {
                                patternParts.Add(testPattern);
                            }
                            else
                            {
                                patternParts.Add(@"[0-9A-Fa-f]{1,10}");
                            }
                        }
                        else
                        {
                            patternParts.Add(@"\d{1,10}"); // 32-bit can be 1-10 digits
                        }
                        break;
                        
                    case "guid":
                        // GUID without dashes (32 hex characters)
                        patternParts.Add(@"[a-fA-F0-9]{32}");
                        break;
                        
                    case "date/time":
                        var format = element.Value;
                        if (!string.IsNullOrEmpty(format))
                        {
                            var datePattern = ConvertDateFormatToRegex(format);
                            if (!string.IsNullOrEmpty(datePattern))
                            {
                                patternParts.Add(datePattern);
                            }
                        }
                        break;
                        
                    case "sequence":
                        // Sequence can be any number
                        patternParts.Add(@"\d+");
                        break;
                }
            }

            if (!patternParts.Any())
                return string.Empty;

            // Create a pattern that matches the entire string (start to end)
            return $"^{string.Join("", patternParts)}$";
        }
        
        /// <summary>
        /// Gets a regex pattern for a random element based on its format string
        /// </summary>
        /// <param name="format">The format string (e.g., "X5_", "D6_")</param>
        /// <param name="defaultDigits">Default number of digits if no format specified</param>
        /// <returns>A regex pattern</returns>
        private string GetPatternForRandomElement(string format, int defaultDigits)
        {
            if (string.IsNullOrEmpty(format)) 
                return $@"\d{{{defaultDigits}}}";

            var pattern = format;
            
            // Handle hex formats like X5
            if (format.StartsWith("X"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"X(\d+)(.*)");
                if (match.Success)
                {
                    var digitCount = match.Groups[1].Value;
                    var suffix = match.Groups[2].Value;
                    return $@"[0-9A-Fa-f]{{{digitCount}}}{System.Text.RegularExpressions.Regex.Escape(suffix)}";
                }
            }
            // Handle decimal formats like D6
            else if (format.StartsWith("D"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(format, @"D(\d+)(.*)");
                if (match.Success)
                {
                    var digitCount = match.Groups[1].Value;
                    var suffix = match.Groups[2].Value;
                    return $@"\d{{{digitCount}}}{System.Text.RegularExpressions.Regex.Escape(suffix)}";
                }
            }
            // Handle other formats - escape and allow for any suffix
            else
            {
                // If format starts with something else, treat it as a suffix
                return $@"[0-9A-Fa-f]+{System.Text.RegularExpressions.Regex.Escape(format)}";
            }
            
            return string.Empty;
        }

        /// <summary>
        /// Converts a date format string to a regex pattern
        /// </summary>
        /// <param name="dateFormat">The date format string</param>
        /// <returns>A regex pattern for the date format</returns>
        private string ConvertDateFormatToRegex(string dateFormat)
        {
            var pattern = dateFormat;
            
            // Replace date format tokens with regex patterns
            pattern = pattern.Replace("yyyy", @"\d{4}");  // 4-digit year
            pattern = pattern.Replace("yy", @"\d{2}");    // 2-digit year
            pattern = pattern.Replace("MM", @"\d{2}");    // 2-digit month
            pattern = pattern.Replace("M", @"\d{1,2}");   // 1-2 digit month
            pattern = pattern.Replace("dd", @"\d{2}");    // 2-digit day
            pattern = pattern.Replace("d", @"\d{1,2}");   // 1-2 digit day
            pattern = pattern.Replace("HH", @"\d{2}");    // 2-digit hour (24h)
            pattern = pattern.Replace("H", @"\d{1,2}");   // 1-2 digit hour (24h)
            pattern = pattern.Replace("mm", @"\d{2}");    // 2-digit minute
            pattern = pattern.Replace("m", @"\d{1,2}");   // 1-2 digit minute
            pattern = pattern.Replace("ss", @"\d{2}");    // 2-digit second
            pattern = pattern.Replace("s", @"\d{1,2}");   // 1-2 digit second
            
            // Escape other special regex characters that might be in the format
            pattern = System.Text.RegularExpressions.Regex.Escape(pattern);
            
            // Unescape the digit patterns we just added
            pattern = pattern.Replace(@"\\d\{4\}", @"\d{4}");
            pattern = pattern.Replace(@"\\d\{2\}", @"\d{2}");
            pattern = pattern.Replace(@"\\d\{1,2\}", @"\d{1,2}");
            
            return pattern;
        }

        /// <summary>
        /// Gets a user-friendly error message for custom ID validation failures
        /// </summary>
        /// <param name="customId">The custom ID that failed validation</param>
        /// <param name="elements">The custom ID format elements</param>
        /// <returns>A descriptive error message</returns>
        public string GetCustomIdValidationErrorMessage(string customId, List<CustomIdElement> elements)
        {
            if (string.IsNullOrEmpty(customId))
                return "Custom ID cannot be empty.";

            if (customId.Length > 100)
                return "Custom ID cannot be longer than 100 characters.";

            if (string.IsNullOrWhiteSpace(customId))
                return "Custom ID cannot contain only whitespace.";

            if (elements == null || !elements.Any())
                return "Custom ID format is invalid.";

            // Get the expected format description
            var formatDescription = GetFormatDescription(elements);
            var formatExample = GetFormatExample(elements);

            return $"Custom ID '{customId}' does not match the required format. Expected format: {formatDescription}. Example: {formatExample}";
        }

        /// <summary>
        /// Gets a human-readable description of the custom ID format
        /// </summary>
        /// <param name="elements">The custom ID format elements</param>
        /// <returns>A description of the format</returns>
        private string GetFormatDescription(List<CustomIdElement> elements)
        {
            var sortedElements = elements.OrderBy(e => e.Order).ToList();
            var descriptions = new List<string>();

            foreach (var element in sortedElements)
            {
                var elementType = element.Type?.ToLower();
                
                switch (elementType)
                {
                    case "fixed":
                        var fixedValue = element.GetCleanValue();
                        if (!string.IsNullOrEmpty(fixedValue))
                        {
                            descriptions.Add($"'{fixedValue}'");
                        }
                        break;
                        
                    case "20-bit random":
                        descriptions.Add("6-digit number");
                        break;
                        
                    case "32-bit random":
                        descriptions.Add("9-digit number");
                        break;
                        
                    case "guid":
                        descriptions.Add("32-character GUID");
                        break;
                        
                    case "date/time":
                        var format = element.Value;
                        if (!string.IsNullOrEmpty(format))
                        {
                            descriptions.Add($"date/time ({format})");
                        }
                        break;
                        
                    case "sequence":
                        descriptions.Add("sequence number");
                        break;
                }
            }

            return descriptions.Any() ? string.Join(" + ", descriptions) : "undefined format";
        }

        /// <summary>
        /// Gets an example of what a valid custom ID should look like based on the format elements
        /// </summary>
        /// <param name="elements">The custom ID format elements</param>
        /// <returns>A string showing the expected format</returns>
        public string GetFormatExample(List<CustomIdElement> elements)
        {
            if (elements == null || !elements.Any())
                return "No format defined";

            var exampleParts = new List<string>();
            
            foreach (var element in elements.OrderBy(e => e.Order))
            {
                switch (element.Type?.ToLower())
                {
                    case "fixed":
                        exampleParts.Add(element.GetCleanValue());
                        break;
                    case "20-bit random":
                        exampleParts.Add("[20-bit-random]");
                        break;
                    case "32-bit random":
                        exampleParts.Add("[32-bit-random]");
                        break;
                    case "6-digit random":
                        exampleParts.Add("[6-digit-random]");
                        break;
                    case "9-digit random":
                        exampleParts.Add("[9-digit-random]");
                        break;
                    case "guid":
                        exampleParts.Add("[GUID]");
                        break;
                    case "date/time":
                        var format = element.Value;
                        if (!string.IsNullOrEmpty(format))
                        {
                            if (format.Contains("yyyy"))
                                exampleParts.Add("[YYYY]");
                            else if (format.Contains("yy"))
                                exampleParts.Add("[YY]");
                            else
                                exampleParts.Add("[DATE]");
                        }
                        else
                        {
                            exampleParts.Add("[DATE]");
                        }
                        break;
                    case "sequence":
                        exampleParts.Add("[SEQUENCE]");
                        break;
                    default:
                        exampleParts.Add($"[{element.Type?.ToUpper() ?? "UNKNOWN"}]");
                        break;
                }
            }

            return string.Join("", exampleParts);
        }

        /// <summary>
        /// Generates a valid custom ID example for an inventory to show users what the format should look like
        /// </summary>
        /// <param name="inventoryId">The inventory ID to generate an example for</param>
        /// <returns>A valid custom ID example</returns>
        public async Task<string> GenerateValidCustomIdExampleAsync(int inventoryId)
        {
            var inventory = await _dataAccess.InventoryData.GetByIdAsync(inventoryId);
            if (inventory == null)
                return "EXAMPLE-ID";

            var nextSequence = await GetNextSequenceNumberAsync(inventoryId);
            
            if (!string.IsNullOrEmpty(inventory.CustomIdElements))
            {
                try 
                {
                    var elements = JsonSerializer.Deserialize<List<CustomIdElement>>(inventory.CustomIdElements);
                    return GenerateAdvancedCustomId(elements ?? new List<CustomIdElement>(), nextSequence);
                }
                catch (Exception)
                {
                    return GenerateCustomId(inventory.CustomIdFormat ?? "ITEM-{SEQUENCE}", nextSequence);
                }
            }
            else if (!string.IsNullOrEmpty(inventory.CustomIdFormat))
            {
                return GenerateCustomId(inventory.CustomIdFormat, nextSequence);
            }

            return $"ITEM-{nextSequence}";
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
            return element.GetCleanValue();
        }
    }

    public class Random20BitProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            var random = new Random();
            var random20Bit = random.Next(0, 1048576); // 2^20
            return FormatRandomValue(random20Bit, element.Value);
        }

        private string FormatRandomValue(int value, string format)
        {
            if (string.IsNullOrEmpty(format)) return value.ToString();

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
            }
            else
            {
                result = value.ToString() + format.Substring(1);
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
            return FormatRandomValue(random32Bit, element.Value);
        }

        private string FormatRandomValue(int value, string format)
        {
            if (string.IsNullOrEmpty(format)) return value.ToString();

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
            }
            else
            {
                result = value.ToString() + format.Substring(1);
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
            return FormatRandomValue(random6Digit, element.Value);
        }

        private string FormatRandomValue(int value, string format)
        {
            if (string.IsNullOrEmpty(format)) return value.ToString();

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
            }
            else
            {
                result = value.ToString() + format.Substring(1);
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
            return FormatRandomValue(random9Digit, element.Value);
        }

        private string FormatRandomValue(int value, string format)
        {
            if (string.IsNullOrEmpty(format)) return value.ToString();

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
            }
            else
            {
                result = value.ToString() + format.Substring(1);
            }

            return result;
        }
    }

    public class GuidElementProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            var guid = Guid.NewGuid();
            return FormatGuid(guid, element.Value);
        }

        private string FormatGuid(Guid guid, string format)
        {
            if (string.IsNullOrEmpty(format)) return guid.ToString("N");

            string formatSpecifier;
            string suffix = string.Empty;

            var match = System.Text.RegularExpressions.Regex.Match(format, @"^([ndbp])(.*)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success)
            {
                formatSpecifier = match.Groups[1].Value.ToUpper();
                suffix = match.Groups[2].Value;
            }
            else
            {
                formatSpecifier = "N";
                suffix = format;
            }

            string result = formatSpecifier.ToUpper() switch
            {
                "N" => guid.ToString("N") + suffix,
                "D" => guid.ToString("D") + suffix,
                "B" => guid.ToString("B") + suffix,
                "P" => guid.ToString("P") + suffix,
                _ => guid.ToString("N") + suffix
            };

            return result;
        }
    }

    public class DateTimeElementProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            return FormatDateTime(DateTime.Now, element.Value);
        }

        private string FormatDateTime(DateTime dateTime, string format)
        {
            if (string.IsNullOrEmpty(format)) return dateTime.ToString("yyyy");

            if (format.Contains('_') || format.IndexOfAny(new[] { '-', '/', '\\' }) >= 0)
            {
                try
                {
                    var match = System.Text.RegularExpressions.Regex.Match(format, @"([yMdHhms]+)([_\-/\\].*)");
                    if (match.Success)
                    {
                        string dateFormat = match.Groups[1].Value;
                        string suffix = match.Groups[2].Value;

                        return dateTime.ToString(dateFormat) + suffix;
                    }

                    return dateTime.ToString(format);
                }
                catch (Exception)
                {
                    if (format.Contains("yyyy") && format.Contains("_"))
                    {
                        return format.Replace("yyyy", dateTime.Year.ToString());
                    }
                }
            }
            else
            {
                try
                {
                    return dateTime.ToString(format);
                }
                catch (Exception)
                {
                    // Fallback to default format on error
                }
            }

            return dateTime.ToString("yyyy");
        }
    }

    public class SequenceElementProcessor : ICustomIdElementProcessor
    {
        public string ProcessElement(CustomIdElement element, int sequenceNumber)
        {
            return FormatSequence(sequenceNumber, element.Value);
        }

        private string FormatSequence(int sequence, string format)
        {
            if (string.IsNullOrEmpty(format)) return sequence.ToString("D3");

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
            }
            else
            {
                result = sequence.ToString("D3") + (format.Length > 1 ? format.Substring(1) : "");
            }

            return result;
        }
    }
}
