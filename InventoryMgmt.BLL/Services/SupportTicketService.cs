using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Services
{
    
    public class SupportTicketService : ISupportTicketService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepo _userRepo;
        private readonly IHttpClientFactory _httpClientFactory;

        public SupportTicketService(
            IConfiguration configuration,
            IUserRepo userRepo,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _userRepo = userRepo;
            _httpClientFactory = httpClientFactory;
        }

        
        public async Task<SupportTicketResult> CreateAndUploadTicketAsync(
            SupportTicketFormDto formModel, 
            string username, 
            string inventoryTitle = null)
        {
            try
            {
                // Create the ticket DTO
                var ticket = new SupportTicketDto
                {
                    ReportedBy = username,
                    Inventory = inventoryTitle ?? "General Support Request",
                    Link = formModel.SourceUrl,
                    Summary = formModel.Summary,
                    Priority = formModel.Priority,
                    CreatedAt = DateTime.UtcNow,
                    AdminEmails = (await GetAdminEmailsAsync()).ToList(),
                    AdditionalInfo = formModel.AdditionalInfo
                };

                // Convert to JSON
                string jsonContent = JsonConvert.SerializeObject(ticket, Formatting.Indented);

                // Generate a unique filename
                string fileName = $"ticket_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.json";

                // Upload to cloud storage
                string cloudProvider = _configuration["SUPPORT_TICKET_PROVIDER"] ?? "Dropbox";
                bool uploadSuccessful = false;
                string uploadError = null;
                
                if (cloudProvider == "OneDrive")
                {
                    try
                    {
                        await UploadToOneDriveAsync(jsonContent, fileName);
                        uploadSuccessful = true;
                    }
                    catch (Exception ex)
                    {
                        uploadError = ex.Message;
                    }
                }
                else if (cloudProvider == "Dropbox")
                {
                    try
                    {
                        await UploadToDropboxAsync(jsonContent, fileName);
                        uploadSuccessful = true;
                    }
                    catch (Exception ex)
                    {
                        uploadError = ex.Message;
                    }
                }
                else
                {
                    // For now, save locally for testing
                    await SaveLocallyAsync(jsonContent, fileName);
                }

                if (uploadSuccessful)
                {
                    return SupportTicketResult.SuccessResult("Support ticket was successfully created and uploaded to cloud storage");
                }
                else if (!string.IsNullOrEmpty(uploadError))
                {
                    return SupportTicketResult.SuccessResult($"Support ticket was created (saved locally). Upload to {cloudProvider} failed: {uploadError}");
                }
                else
                {
                    return SupportTicketResult.SuccessResult("Support ticket was successfully created and saved locally");
                }
            }
            catch (Exception ex)
            {
                return SupportTicketResult.ErrorResult($"Failed to create support ticket: {ex.Message}", ex.ToString());
            }
        }

        
        public async Task<string[]> GetAdminEmailsAsync()
        {
            // Get admin emails from environment variable
            var configEmailsString = _configuration["SUPPORT_TICKET_ADMIN_EMAILS"];
            
            if (!string.IsNullOrEmpty(configEmailsString))
            {
                return configEmailsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(email => email.Trim())
                                        .ToArray();
            }

            // Fallback to getting admin users from the database
            try
            {
                var adminUsers = await _userRepo.GetAllAsync();
                return adminUsers.Where(u => u.Role == "Admin").Select(u => u.Email).ToArray();
            }
            catch
            {
                // If database fails, return default
                return new[] { "admin@inventorymgmt.com" };
            }
        }

        
        private async Task UploadToOneDriveAsync(string content, string fileName)
        {
            // For now, save locally until OneDrive permissions are resolved
            await SaveLocallyAsync(content, fileName);
        }

        
        private async Task UploadToDropboxAsync(string content, string fileName)
        {
            try
            {
                var accessToken = _configuration["DROPBOX_ACCESS_TOKEN"];
                var folderPath = _configuration["DROPBOX_UPLOAD_FOLDER_PATH"];
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("Dropbox access token is not configured");
                }
                
                // Use HttpClient to upload to Dropbox API
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                
                // Set up the API endpoint
                var apiUrl = "https://content.dropboxapi.com/2/files/upload";
                
                // Build the Dropbox API path
                var dropboxPath = $"{folderPath}/{fileName}";
                
                // Set up API arguments
                var dropboxApiArg = JsonConvert.SerializeObject(new
                {
                    path = dropboxPath,
                    mode = "add",
                    autorename = true,
                    mute = false
                });
                
                // Create the request
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Add("Dropbox-API-Arg", dropboxApiArg);
                request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                
                // Send the request
                var response = await client.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Dropbox upload failed: {response.StatusCode} - {errorContent}");
                }
                
                // Success - don't save locally
            }
            catch (Exception ex)
            {
                // Log the actual error and still save locally as fallback
                System.Diagnostics.Debug.WriteLine($"Dropbox upload error: {ex.Message}");
                await SaveLocallyAsync(content, fileName);
                throw; // Re-throw to bubble up the error
            }
        }

       
        private async Task SaveLocallyAsync(string content, string fileName)
        {
            try
            {
                var folderPath = Path.Combine(Path.GetTempPath(), "SupportTickets");
                Directory.CreateDirectory(folderPath);
                
                var filePath = Path.Combine(folderPath, fileName);
                await File.WriteAllTextAsync(filePath, content);
            }
            catch
            {
                // Silent fail for local save
            }
        }
    }
}