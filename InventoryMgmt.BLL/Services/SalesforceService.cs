using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Services
{
    public class SalesforceService : ISalesforceService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private string _instanceUrl;
        private DateTime _tokenExpiration;

        public SalesforceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _tokenExpiration = DateTime.UtcNow;
        }

        private async Task EnsureTokenAsync()
        {
            if (DateTime.UtcNow >= _tokenExpiration)
            {
                await GetAccessTokenAsync();
            }
        }

        // Make this method public so it can be called for testing
        public async Task<(bool Success, string ErrorMessage)> TestAuthenticationAsync()
        {
            try
            {
                await GetAccessTokenAsync();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        
        private async Task GetAccessTokenAsync()
        {
            try
            {
                // Clear any existing headers
                _httpClient.DefaultRequestHeaders.Clear();
                
                var clientId = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET");
                var username = Environment.GetEnvironmentVariable("SALESFORCE_USERNAME");
                var password = Environment.GetEnvironmentVariable("SALESFORCE_PASSWORD");
                var securityToken = Environment.GetEnvironmentVariable("SALESFORCE_SECURITY_TOKEN");
                
                // If security token is provided, append it to password
                if (!string.IsNullOrEmpty(securityToken))
                {
                    password += securityToken;
                }
                
                var tokenEndpoint = Environment.GetEnvironmentVariable("SALESFORCE_TOKEN_ENDPOINT") ?? "https://login.salesforce.com/services/oauth2/token";

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) ||
                    string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new InvalidOperationException("Salesforce API credentials not configured in environment variables");
                }
                
                // Create form data for the request
                var formData = new Dictionary<string, string>()
                {
                    { "grant_type", "password" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "username", username },
                    { "password", password }
                };
                
                var requestContent = new FormUrlEncodedContent(formData);

                var response = await _httpClient.PostAsync(tokenEndpoint, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // If failed with production URL, try sandbox URL
                if (!response.IsSuccessStatusCode && tokenEndpoint.Contains("login.salesforce.com"))
                {
                    // Try sandbox URL as fallback
                    tokenEndpoint = "https://test.salesforce.com/services/oauth2/token";
                    
                    // Retry with sandbox URL
                    response = await _httpClient.PostAsync(tokenEndpoint, requestContent);
                    responseContent = await response.Content.ReadAsStringAsync();
                }

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        // Parse using JsonDocument for more flexibility and safety
                        using (JsonDocument document = JsonDocument.Parse(responseContent))
                        {
                            JsonElement root = document.RootElement;

                            // Check if the expected properties exist with case variations
                            string[] accessTokenVariations = { "access_token", "accessToken", "Access_Token", "AccessToken" };
                            string[] instanceUrlVariations = { "instance_url", "instanceUrl", "Instance_Url", "InstanceUrl" };
                            string[] expiresInVariations = { "expires_in", "expiresIn", "Expires_In", "ExpiresIn" };

                            // Try to find access_token with different case variations
                            bool accessTokenFound = false;
                            foreach (var tokenKey in accessTokenVariations)
                            {
                                if (root.TryGetProperty(tokenKey, out JsonElement accessTokenElement))
                                {
                                    _accessToken = accessTokenElement.GetString();
                                    accessTokenFound = true;
                                    break;
                                }
                            }

                            if (!accessTokenFound)
                            {
                                // Try using regex as a fallback
                                var accessTokenMatch = System.Text.RegularExpressions.Regex.Match(responseContent, "\"access[_]?[tT]oken\"\\s*:\\s*\"([^\"]+)\"");
                                if (accessTokenMatch.Success)
                                {
                                    _accessToken = accessTokenMatch.Groups[1].Value;
                                    accessTokenFound = true;
                                }
                                
                                if (!accessTokenFound)
                                {
                                    throw new Exception("access_token not found in token response");
                                }
                            }

                            // Try to find instance_url with different case variations
                            bool instanceUrlFound = false;
                            foreach (var urlKey in instanceUrlVariations)
                            {
                                if (root.TryGetProperty(urlKey, out JsonElement instanceUrlElement))
                                {
                                    _instanceUrl = instanceUrlElement.GetString();
                                    instanceUrlFound = true;
                                    break;
                                }
                            }

                            if (!instanceUrlFound)
                            {
                                // Try using regex as a fallback
                                var instanceUrlMatch = System.Text.RegularExpressions.Regex.Match(responseContent, "\"instance[_]?[uU]rl\"\\s*:\\s*\"([^\"]+)\"");
                                if (instanceUrlMatch.Success)
                                {
                                    _instanceUrl = instanceUrlMatch.Groups[1].Value;
                                    instanceUrlFound = true;
                                }
                                
                                // Fall back to configured instance URL if not found
                                if (!instanceUrlFound)
                                {
                                    _instanceUrl = Environment.GetEnvironmentVariable("SALESFORCE_INSTANCE_URL");
                                }
                            }

                            // Try to find expires_in with different case variations
                            int expiresIn = 3600; // Default to 1 hour
                            foreach (var expiresKey in expiresInVariations)
                            {
                                if (root.TryGetProperty(expiresKey, out JsonElement expiresInElement))
                                {
                                    expiresIn = expiresInElement.GetInt32();
                                    break;
                                }
                            }

                            _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn - 60); // Subtracting 60 seconds as buffer
                        }
                    }
                    catch (JsonException)
                    {
                        // Fall back to regex for extracting tokens if JSON parsing fails
                        var accessTokenMatch = System.Text.RegularExpressions.Regex.Match(responseContent, "\"access[_]?[tT]oken\"\\s*:\\s*\"([^\"]+)\"");
                        if (accessTokenMatch.Success)
                        {
                            _accessToken = accessTokenMatch.Groups[1].Value;
                        }
                        else
                        {
                            throw new Exception("Could not extract access token from response");
                        }

                        // Try to extract instance URL
                        var instanceUrlMatch = System.Text.RegularExpressions.Regex.Match(responseContent, "\"instance[_]?[uU]rl\"\\s*:\\s*\"([^\"]+)\"");
                        if (instanceUrlMatch.Success)
                        {
                            _instanceUrl = instanceUrlMatch.Groups[1].Value;
                        }
                        else
                        {
                            // Fall back to configuration
                            _instanceUrl = Environment.GetEnvironmentVariable("SALESFORCE_INSTANCE_URL");
                        }

                        // Default expiration to 1 hour
                        _tokenExpiration = DateTime.UtcNow.AddHours(1).AddMinutes(-1); // 59 minutes as buffer
                    }
                }
                else
                {
                    throw new Exception($"Failed to get Salesforce access token. Status: {response.StatusCode}, Message: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error authenticating with Salesforce: {ex.Message}", ex);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> CreateAccountAndContactAsync(SalesforceAccountDto accountDto)
        {
            try
            {
                try
                {
                    // Always get a fresh token
                    await GetAccessTokenAsync();
                }
                catch (Exception tokenEx)
                {
                    return (false, $"Failed to authenticate with Salesforce: {tokenEx.Message}");
                }
                
                // Validate token before proceeding
                if (string.IsNullOrEmpty(_accessToken))
                {
                    return (false, "Authentication failed: Access token is empty");
                }
                
                if (string.IsNullOrEmpty(_instanceUrl))
                {
                    return (false, "Authentication failed: Instance URL is empty");
                }

                // Create Account first
                try
                {
                    var accountId = await CreateAccountAsync(accountDto);
                    
                    if (!string.IsNullOrEmpty(accountId))
                    {
                        // Create Contact and link to the Account
                        try
                        {
                            var contactCreated = await CreateContactAsync(accountDto, accountId);
                            return contactCreated 
                                ? (true, string.Empty) 
                                : (false, "Contact creation failed with unknown error");
                        }
                        catch (Exception contactEx)
                        {
                            return (false, $"Failed to create contact: {contactEx.Message}");
                        }
                    }
                    
                    return (false, "Failed to create account: No account ID returned");
                }
                catch (Exception accountEx)
                {
                    return (false, $"Failed to create account: {accountEx.Message}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"An unexpected error occurred: {ex.Message}");
            }
        }

        private async Task<string> CreateAccountAsync(SalesforceAccountDto accountDto)
        {
            try
            {
                // Clear existing headers and set fresh ones
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var apiVersion = Environment.GetEnvironmentVariable("SALESFORCE_API_VERSION") ?? "59.0";
                
                // Use the instance URL obtained from the token response
                var accountEndpoint = $"{_instanceUrl}/services/data/v{apiVersion}/sobjects/Account";
                
                var accountData = new
                {
                    Name = accountDto.CompanyName,
                    Industry = accountDto.Industry,
                    AnnualRevenue = accountDto.AnnualRevenue,
                    Phone = accountDto.PhoneNumber,
                    Website = accountDto.Website,
                    Description = accountDto.Description
                };

                var jsonContent = JsonSerializer.Serialize(accountData);
                var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(accountEndpoint, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using (JsonDocument document = JsonDocument.Parse(responseContent))
                        {
                            JsonElement root = document.RootElement;
                            
                            if (root.TryGetProperty("id", out JsonElement idElement))
                            {
                                string accountId = idElement.GetString();
                                return accountId;
                            }
                            else
                            {
                                throw new Exception("id property not found in account creation response");
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Try to extract ID using regex as a fallback
                        var idMatch = System.Text.RegularExpressions.Regex.Match(responseContent, "\"id\"\\s*:\\s*\"([^\"]+)\"");
                        if (idMatch.Success)
                        {
                            string accountId = idMatch.Groups[1].Value;
                            return accountId;
                        }
                        
                        throw new Exception("Failed to parse account creation response");
                    }
                }
                else
                {
                    throw new Exception($"Failed to create Salesforce account. Status: {response.StatusCode}, Message: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating account: {ex.Message}", ex);
            }
        }

        private async Task<bool> CreateContactAsync(SalesforceAccountDto accountDto, string accountId)
        {
            try
            {
                // Clear existing headers and set fresh ones
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var apiVersion = Environment.GetEnvironmentVariable("SALESFORCE_API_VERSION") ?? "59.0";
                
                // Use the instance URL obtained from the token response
                var contactEndpoint = $"{_instanceUrl}/services/data/v{apiVersion}/sobjects/Contact";
                
                var contactData = new
                {
                    AccountId = accountId,
                    FirstName = accountDto.FirstName,
                    LastName = accountDto.LastName,
                    Email = accountDto.Email,
                    Title = accountDto.JobTitle,
                    MobilePhone = accountDto.MobilePhone
                };

                var jsonContent = JsonSerializer.Serialize(contactData);
                var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(contactEndpoint, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new Exception($"Failed to create Salesforce contact. Status: {response.StatusCode}, Message: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating contact: {ex.Message}", ex);
            }
        }
    }
}
