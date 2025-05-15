using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using EF.Server.Models;
using EF.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EF.Server.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private const int SaltSize = 16;
    private const int HashSize = 20;
    private const int Iterations = 100000;

    public AuthService(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool success, string message, string? token)> RegisterAsync(string username, string email, string password, string role)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
            return (false, "Email already exists", null);

        if (await _context.Users.AnyAsync(u => u.Username == username))
            return (false, "Username already exists", null);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            Role = role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return (true, "Registration successful", token);
    }

    public async Task<(bool success, string message, string? token)> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null)
            return (false, "Invalid email or password", null);

        if (!VerifyPassword(password, user.PasswordHash))
            return (false, "Invalid email or password", null);

        var token = GenerateJwtToken(user);
        return (true, "Login successful", token);
    }

    public string HashPassword(string password)
    {
        try
        {
            _logger.LogInformation("Hashing password");
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with the salt
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Combine the salt and hash
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Convert to base64 string
            string hashedPassword = Convert.ToBase64String(hashBytes);
            _logger.LogInformation("Password hashed successfully");
            return hashedPassword;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw;
        }
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            _logger.LogInformation("Verifying password");
            
            // First try to verify with the new format
            try
            {
                // Convert the stored hash back to bytes
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                // Extract the salt
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Hash the input password with the same salt
                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Compare the hashes
                bool result = true;
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != hash[i])
                    {
                        result = false;
                        break;
                    }
                }

                _logger.LogInformation("Password verification result: {Result}", result);
                return result;
            }
            catch (FormatException)
            {
                // If the hash is not in the new format, try the old format
                _logger.LogInformation("Password hash is not in the new format, trying old format");
                
                // For old passwords, just do a simple comparison
                // This is not secure, but it allows users to log in and then update their password
                bool result = password == hashedPassword;
                _logger.LogInformation("Old format password verification result: {Result}", result);
                
                if (result)
                {
                    // If the old format works, update the password to the new format
                    _logger.LogInformation("Updating password to new format");
                    var user = _context.Users.FirstOrDefault(u => u.PasswordHash == hashedPassword);
                    if (user != null)
                    {
                        user.PasswordHash = HashPassword(password);
                        _context.SaveChanges();
                        _logger.LogInformation("Password updated to new format");
                    }
                }
                
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            throw;
        }
    }

    public string GenerateJwtToken(User user)
    {
        try
        {
            _logger.LogInformation("Generating JWT token for user: {Username}", user.Username);

            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration");
            var keyBytes = Encoding.UTF8.GetBytes(key);
            
            // Ensure the key is at least 32 bytes (256 bits) for HS256
            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException("JWT Key must be at least 32 bytes (256 bits) for HS256");
            }

            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("JWT token generated successfully for user: {Username}", user.Username);
            
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user: {Username}", user.Username);
            throw;
        }
    }
} 