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
