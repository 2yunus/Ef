using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EF.Server.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using EF.Server.Data;
using EF.Server.Models;

namespace EF.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly FeedbackService _feedbackService;
    private readonly ILogger<FeedbackController> _logger;
    private readonly ApplicationDbContext _context;

    public FeedbackController(FeedbackService feedbackService, ILogger<FeedbackController> logger, ApplicationDbContext context)
    {
        _feedbackService = feedbackService;
        _logger = logger;
        _context = context;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackRequest request)
    {
        try
        {
            _logger.LogInformation("Received feedback submission request: {@Request}", request);

            if (request == null)
            {
                _logger.LogWarning("Feedback request is null");
                return BadRequest(new { message = "Invalid request" });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation("User ID from token: {UserId}", userId);
            
            var (success, message, feedback) = await _feedbackService.SubmitFeedbackAsync(
                request.Content,
                request.IsAnonymous,
                request.Category,
                request.Sentiment,
                request.IsAnonymous ? null : userId
            );

            if (!success)
            {
                _logger.LogWarning("Feedback submission failed: {Message}", message);
                return BadRequest(new { message });
            }

            _logger.LogInformation("Feedback submitted successfully: {@Feedback}", feedback);
            return Ok(new { message, feedback });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback. Request: {@Request}", request);
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetFeedbacks()
    {
        try
        {
            // Get the current user's ID from the claims
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                _logger.LogWarning("No user ID found in claims");
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            _logger.LogInformation("User ID from token: {UserId}", userId);

            // Get the current user
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (currentUser == null)
            {
                _logger.LogWarning("User not found in database: {UserId}", userId);
                return NotFound("User not found");
            }

            // Log all claims for debugging
            _logger.LogInformation("All claims for user:");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("Claim Type: {Type}, Value: {Value}", claim.Type, claim.Value);
            }

            // Check if user has Admin or Manager role using the full claim type
            var roleClaim = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            _logger.LogInformation("Role claim value: {Role}", roleClaim);

            if (roleClaim != "Admin" && roleClaim != "Manager")
            {
                _logger.LogWarning("User {UserId} does not have required role. Current role: {Role}", userId, roleClaim);
                return Forbid();
            }

            // Return all feedbacks
            var feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {Count} feedbacks for user {UserId}", feedbacks.Count, userId);
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching feedbacks");
            return StatusCode(500, "An error occurred while fetching feedbacks");
        }
    }

    [HttpGet("team")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetTeamFeedback()
    {
        try
        {
            // Get the current user's ID from the claims
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                _logger.LogWarning("No user ID found in claims");
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            _logger.LogInformation("User ID from token: {UserId}", userId);

            // Get the current user
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (currentUser == null)
            {
                _logger.LogWarning("User not found in database: {UserId}", userId);
                return NotFound("User not found");
            }

            // Log all claims for debugging
            _logger.LogInformation("All claims for user:");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("Claim Type: {Type}, Value: {Value}", claim.Type, claim.Value);
            }

            // Check if user has Admin or Manager role using the full claim type
            var roleClaim = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            _logger.LogInformation("Role claim value: {Role}", roleClaim);

            if (roleClaim != "Admin" && roleClaim != "Manager")
            {
                _logger.LogWarning("User {UserId} does not have required role. Current role: {Role}", userId, roleClaim);
                return Forbid();
            }

            // Return all feedbacks
            var feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {Count} feedbacks for user {UserId}", feedbacks.Count, userId);
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching team feedback");
            return StatusCode(500, "An error occurred while fetching team feedback");
        }
    }

    [HttpGet("analytics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAnalytics()
    {
        try
        {
            var analytics = await _feedbackService.GetAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }
}

public class FeedbackRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Content { get; set; } = string.Empty;

    public bool IsAnonymous { get; set; }

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Sentiment { get; set; } = string.Empty;
} 