using EF.Server.Data;
using EF.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace EF.Server.Services;

public class FeedbackService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FeedbackService> _logger;

    private static readonly string[] ValidCategories = new[]
    {
        "workplace",
        "team",
        "management",
        "process",
        "other"
    };

    private static readonly string[] ValidSentiments = new[]
    {
        "Positive",
        "Neutral",
        "Negative"
    };

    public FeedbackService(ApplicationDbContext context, ILogger<FeedbackService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool success, string message, Feedback? feedback)> SubmitFeedbackAsync(
        string content,
        bool isAnonymous,
        string category,
        string sentiment,
        int? userId = null)
    {
        try
        {
            _logger.LogInformation("Attempting to submit feedback. Category: {Category}, Sentiment: {Sentiment}, IsAnonymous: {IsAnonymous}", 
                category, sentiment, isAnonymous);

            // Validate category
            if (!ValidCategories.Contains(category.ToLower()))
            {
                _logger.LogWarning("Invalid category: {Category}", category);
                return (false, $"Invalid category. Must be one of: {string.Join(", ", ValidCategories)}", null);
            }

            // Validate sentiment
            if (!ValidSentiments.Contains(sentiment))
            {
                _logger.LogWarning("Invalid sentiment: {Sentiment}", sentiment);
                return (false, $"Invalid sentiment. Must be one of: {string.Join(", ", ValidSentiments)}", null);
            }

            // Validate content
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Content is empty or whitespace");
                return (false, "Feedback content cannot be empty", null);
            }

            if (content.Length < 10)
            {
                _logger.LogWarning("Content is too short: {Length} characters", content.Length);
                return (false, "Feedback content must be at least 10 characters long", null);
            }

            var feedback = new Feedback
            {
                Content = content,
                IsAnonymous = isAnonymous,
                Category = category,
                Sentiment = sentiment,
                UserId = isAnonymous ? 0 : userId ?? 0
            };

            _logger.LogInformation("Creating new feedback entry: {@Feedback}", feedback);

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Feedback saved successfully with ID: {Id}", feedback.Id);
            return (true, "Feedback submitted successfully", feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback. Content: {Content}, Category: {Category}, Sentiment: {Sentiment}", 
                content, category, sentiment);
            return (false, "Failed to submit feedback", null);
        }
    }

    public async Task<object> GetAnalyticsAsync()
    {
        var totalFeedback = await _context.Feedbacks.CountAsync();
        var sentimentCounts = await _context.Feedbacks
            .GroupBy(f => f.Sentiment)
            .Select(g => new { Sentiment = g.Key, Count = g.Count() })
            .ToListAsync();

        var categoryCounts = await _context.Feedbacks
            .GroupBy(f => f.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        return new
        {
            TotalFeedback = totalFeedback,
            SentimentDistribution = sentimentCounts,
            CategoryDistribution = categoryCounts
        };
    }
} 