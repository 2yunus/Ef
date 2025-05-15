using EF.Server.Data;
using EF.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace EF.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET /api/admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            return StatusCode(500, "An error occurred while fetching users");
        }
    }

    // GET /api/admin/users/export
    [HttpGet("users/export")]
    public async Task<IActionResult> ExportUsersToCsv()
    {
        try
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Id,Username,Email,Role,CreatedAt");

            foreach (var user in users)
            {
                csv.AppendLine($"{user.Id},{user.Username},{user.Email},{user.Role},{user.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"users_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting users to CSV");
            return StatusCode(500, "An error occurred while exporting users");
        }
    }

    // GET /api/admin/analytics
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        try
        {
            // Example: Calculate percentage of positive feedback
            var totalFeedback = await _context.Feedbacks.CountAsync();
            var positiveFeedback = await _context.Feedbacks.CountAsync(f => f.Sentiment == "Positive");

            var analytics = totalFeedback > 0
                ? $"{(positiveFeedback * 100) / totalFeedback}% positive feedback"
                : "No feedback available";

            return Ok(new { analytics });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analytics");
            return StatusCode(500, "An error occurred while fetching analytics");
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            _logger.LogInformation("Fetching dashboard statistics");
            
            var totalUsers = await _context.Users.CountAsync();
            var totalFeedback = await _context.Feedbacks.CountAsync();

            var stats = new
            {
                totalUsers,
                totalFeedback
            };

            _logger.LogInformation("Successfully fetched dashboard statistics");
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard statistics");
            return StatusCode(500, "An error occurred while fetching dashboard statistics");
        }
    }
}