using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.DTOs.Coaching;
using Prospera.Application.Features.Coaching.Commands;

namespace Prospera.API.Controllers;

/// <summary>
/// Endpoints for AI-powered financial coaching
/// Not just recommendations, but real coaching with accountability and action plans
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CoachingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CoachingController> _logger;

    public CoachingController(
        IMediator mediator,
        ILogger<CoachingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Start a new coaching session
    /// Provides personalized coaching with assessment, action items, and milestones
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">Coaching session request with situation and goals</param>
    /// <returns>Coaching session with personalized action plan</returns>
    /// <response code="201">Coaching session created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("users/{userId}/session")]
    [ProducesResponseType(typeof(CoachingSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartCoachingSession(
        Guid userId,
        StartCoachingSessionRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting coaching session for user: {UserId} with goal: {Goal}",
            userId,
            request.Goal);

        try
        {
            var command = new StartCoachingSessionCommand
            {
                UserId = userId,
                CurrentSituation = request.CurrentSituation,
                Goal = request.Goal,
                Preferences = request.Preferences,
                Provider = request.Provider,
                ModelName = request.ModelName
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetCoachingSession), new { userId, sessionId = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting coaching session for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get a specific coaching session
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="sessionId">The coaching session ID</param>
    /// <returns>Coaching session details</returns>
    /// <response code="200">Coaching session found</response>
    /// <response code="404">Session or user not found</response>
    [HttpGet("users/{userId}/session/{sessionId}")]
    [ProducesResponseType(typeof(CoachingSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCoachingSession(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting coaching session {SessionId} for user {UserId}", sessionId, userId);

        try
        {
            var command = new GetCoachingProgressCommand { UserId = userId, SessionId = sessionId };
            var result = await _mediator.Send(command, cancellationToken);

            // Return progress info in response
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Coaching session not found: {SessionId}", sessionId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coaching session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Get coaching session progress and action items
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="sessionId">The coaching session ID</param>
    /// <returns>Progress information with pending and completed action items</returns>
    /// <response code="200">Progress retrieved successfully</response>
    /// <response code="404">Session not found</response>
    [HttpGet("users/{userId}/session/{sessionId}/progress")]
    [ProducesResponseType(typeof(CoachingProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCoachingProgress(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting coaching progress for session: {SessionId}", sessionId);

        try
        {
            var command = new GetCoachingProgressCommand { UserId = userId, SessionId = sessionId };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Coaching session not found: {SessionId}", sessionId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coaching progress: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Mark an action item as completed
    /// Updates progress and provides next recommendations
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="sessionId">The coaching session ID</param>
    /// <param name="request">Action item completion request</param>
    /// <returns>Updated progress information</returns>
    /// <response code="200">Action item completed successfully</response>
    /// <response code="404">Session or action item not found</response>
    [HttpPost("users/{userId}/session/{sessionId}/action-items/complete")]
    [ProducesResponseType(typeof(CoachingProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteActionItem(
        Guid userId,
        Guid sessionId,
        CompleteActionItemRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Completing action item {ActionItemId} in session {SessionId}",
            request.ActionItemId,
            sessionId);

        try
        {
            var command = new CompleteCoachingActionCommand
            {
                UserId = userId,
                SessionId = sessionId,
                ActionItemId = request.ActionItemId,
                Notes = request.Notes
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Session or action item not found");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing action item: {ActionItemId}", request.ActionItemId);
            throw;
        }
    }

    /// <summary>
    /// Get coaching history for a user
    /// View all past and current coaching sessions
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>All coaching sessions and history</returns>
    /// <response code="200">Coaching history retrieved successfully</response>
    [HttpGet("users/{userId}/history")]
    [ProducesResponseType(typeof(CoachingHistoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoachingHistory(
        Guid userId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting coaching history for user: {UserId}", userId);

        try
        {
            var command = new GetCoachingHistoryCommand { UserId = userId };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coaching history for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Example coaching session endpoint for documentation
    /// </summary>
    [HttpGet("example")]
    [ProducesResponseType(typeof(CoachingSessionDto), StatusCodes.Status200OK)]
    public IActionResult GetExample()
    {
        var example = new CoachingSessionDto
        {
            Id = Guid.NewGuid(),
            SessionTitle = "Coaching Session - January 15, 2024",
            Goal = "Build emergency fund of $10,000",
            Assessment = "You have a strong income but lack a structured savings plan. Your current savings rate suggests you can allocate $1,500/month to building your emergency fund.",
            CoachingMessage = "Here's what I want you to know: Financial security isn't about being rich—it's about having a safety net that lets you sleep at night. Your emergency fund is the foundation of everything. In 7 months, with consistent action, you'll have complete peace of mind. Let's make this happen.",
            ActionItems = new List<ActionItemDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Open a dedicated high-yield savings account",
                    Description = "Research and open an account at a bank offering 4-5% APY. Examples: Marcus, Ally, or CIT Bank. Transfer $100 from checking to start.",
                    PriorityLevel = 1,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(3),
                    ImplementationTips = "This takes 15 minutes online. You can open an account in one sitting."
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Set up automatic monthly transfer",
                    Description = "Schedule an automatic transfer of $1,500 on payday to your emergency fund. Set it and forget it.",
                    PriorityLevel = 1,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    ImplementationTips = "Most banks let you set this up in their app in 5 minutes. Pick a day right after you get paid."
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Track your progress weekly",
                    Description = "Every Sunday, check your emergency fund balance and celebrate the progress. This builds momentum.",
                    PriorityLevel = 2,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                }
            },
            Milestones = new List<MilestoneDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Foundation Built",
                    TargetProgressPercentage = 25,
                    Description = "You have $2,500 saved. This is your starting victory—you've proven you can do this.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Halfway There",
                    TargetProgressPercentage = 50,
                    Description = "You have $5,000 saved. You're officially building real security. Celebrate this.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Complete Emergency Fund",
                    TargetProgressPercentage = 100,
                    Description = "You have $10,000. You're financial protected. You've won here. Now let's build wealth.",
                    CreatedAt = DateTime.UtcNow
                }
            },
            ProgressPercentage = 0m,
            CoachingMethodology = "SMART Goals + Behavioral Accountability",
            IsActive = true,
            StartDate = DateTime.UtcNow,
            ModelUsed = "mistralai/mistral-7b-instruct:free",
            Provider = "OpenRouter"
        };

        return Ok(example);
    }
}
