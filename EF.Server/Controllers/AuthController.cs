using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EF.Server.Data;
using EF.Server.Models;
using EF.Server.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace EF.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly AuthService _authService;

    public AuthController(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthController> logger,
        AuthService authService)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for user: {Username}", request.Username);
            _logger.LogInformation("Request data: {@RequestData}", request);

            if (request == null)
            {
                _logger.LogWarning("Registration failed: Request is null");
                return BadRequest("Invalid request");
            }

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                _logger.LogWarning("Registration failed: Missing required fields");
                return BadRequest("Username, email, and password are required");
            }

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                _logger.LogWarning("Registration failed: Username already exists");
                return BadRequest("Username already exists");
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                _logger.LogWarning("Registration failed: Email already exists");
                return BadRequest("Email already exists");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = request.Role ?? "User",
                PasswordHash = _authService.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Username}", user.Username);
            return Ok(new { message = "Registration successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during registration");
            _logger.LogError("Request data: {@RequestData}", request);
            return StatusCode(500, new { message = "An unexpected error occurred during registration" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request?.Email);

            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                _logger.LogWarning("Login failed: Missing required fields");
                return BadRequest(new { message = "Email and password are required" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            try
            {
                var isPasswordValid = _authService.VerifyPassword(request.Password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed: Invalid password for user: {Username}", user.Username);
                    return Unauthorized(new { message = "Invalid credentials" });
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Password verification failed due to invalid hash format");
                return StatusCode(500, new { message = "Password verification failed", details = ex.Message });
            }

            var token = _authService.GenerateJwtToken(user);
            return Ok(new { token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during login");
            return StatusCode(500, new { message = "An unexpected error occurred during login", details = ex.Message });
        }
    }

    public string HashPassword(string password)
    {
        // Generate a salt
        byte[] salt = new byte[16];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(salt);
        }

        // Hash the password with the salt
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);

        // Combine the salt and hash
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        // Convert to Base64
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            // Decode the Base64 string
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Extract the salt
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Hash the input password with the same salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // Compare the hashes
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("The stored password hash is not a valid Base64 string.", ex);
        }
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}