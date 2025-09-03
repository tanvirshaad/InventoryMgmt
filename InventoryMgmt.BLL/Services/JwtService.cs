using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using InventoryMgmt.DAL.EF.TableModels;
using DotNetEnv;

namespace InventoryMgmt.BLL.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        ClaimsPrincipal? ValidateToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Try to get JWT key from environment variables first, then fall back to configuration
            var jwtKey = Env.GetString("JWT_KEY") ?? 
                _configuration["Jwt:Key"] ?? 
                "YourSuperSecretKeyHere12345678901234567890";
                
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? "")
            };

            // Try to get JWT issuer and audience from environment variables first, then fall back to configuration
            var issuer = Env.GetString("JWT_ISSUER") ?? 
                _configuration["Jwt:Issuer"] ?? 
                "InventoryMgmt";
                
            var audience = Env.GetString("JWT_AUDIENCE") ?? 
                _configuration["Jwt:Audience"] ?? 
                "InventoryMgmtUsers";
                
            // Try to get JWT expiry days from environment variables first, then fall back to configuration
            var expiryDays = Env.GetString("JWT_EXPIRY_DAYS") ?? 
                _configuration["Jwt:ExpiryInDays"] ?? 
                "7";
                
            var expiryDaysInt = int.Parse(expiryDays);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(expiryDaysInt),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = issuer,
                Audience = audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Try to get JWT key from environment variables first, then fall back to configuration
            var jwtKey = Env.GetString("JWT_KEY") ?? 
                _configuration["Jwt:Key"] ?? 
                "YourSuperSecretKeyHere12345678901234567890";
                
            var key = Encoding.ASCII.GetBytes(jwtKey);
            
            // Try to get JWT issuer and audience from environment variables first, then fall back to configuration
            var issuer = Env.GetString("JWT_ISSUER") ?? 
                _configuration["Jwt:Issuer"] ?? 
                "InventoryMgmt";
                
            var audience = Env.GetString("JWT_AUDIENCE") ?? 
                _configuration["Jwt:Audience"] ?? 
                "InventoryMgmtUsers";

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
