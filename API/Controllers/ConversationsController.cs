using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.DTOs;
using Prospera.Application.DTOs.Conversations;
using Prospera.Application.Features.Conversations.Commands;
using Prospera.Domain.Interfaces;

namespace Prospera.API.Controllers;

/// <summary>
/// Financial conversational chatbot endpoints
/// Users can have ongoing conversations about finance, markets, and investing
/// This is different from recommendations - it's general financial advice chatbot
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(
        IMediator mediator,
        IConversationRepository conversationRepository,
        ILogger<ConversationsController> logger)
    {
        _mediator = mediator;
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Ask a general financial question
    /// Creates a new conversation or continues an existing one
    /// This is a chatbot interface - not a recommendation request
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">Question and optional conversation ID</param>
    /// <returns>Chatbot response message</returns>
    /// <response code="200">Response provided successfully</response>
    /// <response code="400">Invalid request</response>
    [HttpPost("users/{userId}/ask")]
    [ProducesResponseType(typeof(ChatbotResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AskQuestion(
        Guid userId,
        [FromBody] AskChatbotRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { message = "Question cannot be empty" });

        _logger.LogInformation(
            "User {UserId} asking financial question: {Question}",
            userId,
            request.Question.Substring(0, Math.Min(50, request.Question.Length)));

        try
        {
            var command = new AskFinancialChatbotCommand
            {
                UserId = userId,
                Question = request.Question,
                ConversationId = request.ConversationId,
                Topic = request.Topic,
                Provider = request.Provider,
                ModelName = request.ModelName
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Conversation not found");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get a conversation by ID with full message history
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="conversationId">The conversation ID</param>
    /// <returns>Conversation with all messages</returns>
    /// <response code="200">Conversation found</response>
    /// <response code="404">Conversation not found</response>
    [HttpGet("users/{userId}/{conversationId}")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversation(Guid userId, Guid conversationId)
    {
        _logger.LogInformation("Fetching conversation {ConversationId} for user {UserId}", conversationId, userId);

        try
        {
            var conversation = await _conversationRepository.GetByIdAsync(conversationId, userId);
            if (conversation == null)
                return NotFound(new { message = $"Conversation {conversationId} not found" });

            var dto = new ConversationDto
            {
                Id = conversation.Id,
                UserId = conversation.UserId,
                Title = conversation.Title,
                Topic = conversation.Topic,
                Messages = conversation.Messages
                    .Select(m => new ConversationMessageDto
                    {
                        Id = m.Id,
                        SenderType = m.SenderType,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt,
                        MessageType = m.MessageType
                    })
                    .ToList(),
                MessageCount = conversation.Messages.Count,
                StartedAt = conversation.StartedAt,
                LastMessageAt = conversation.LastMessageAt,
                IsActive = conversation.IsActive,
                Summary = conversation.Summary,
                RecommendationIds = conversation.RecommendationIds
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversation {ConversationId}", conversationId);
            throw;
        }
    }

    /// <summary>
    /// Get all conversations for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="topic">Optional topic filter</param>
    /// <returns>List of conversations</returns>
    /// <response code="200">Conversations retrieved</response>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(List<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserConversations(Guid userId, [FromQuery] string? topic = null)
    {
        _logger.LogInformation("Fetching conversations for user {UserId}", userId);

        try
        {
            var conversations = await _conversationRepository.GetUserConversationsAsync(userId, topic);

            var dtos = conversations
                .Select(c => new ConversationDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Title = c.Title,
                    Topic = c.Topic,
                    MessageCount = c.Messages.Count,
                    StartedAt = c.StartedAt,
                    LastMessageAt = c.LastMessageAt,
                    IsActive = c.IsActive,
                    Summary = c.Summary,
                    RecommendationIds = c.RecommendationIds
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching conversations for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get active conversations for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of active conversations</returns>
    /// <response code="200">Active conversations retrieved</response>
    [HttpGet("users/{userId}/active")]
    [ProducesResponseType(typeof(List<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveConversations(Guid userId)
    {
        _logger.LogInformation("Fetching active conversations for user {UserId}", userId);

        try
        {
            var conversations = await _conversationRepository.GetActiveConversationsAsync(userId);

            var dtos = conversations
                .Select(c => new ConversationDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Topic = c.Topic,
                    MessageCount = c.Messages.Count,
                    StartedAt = c.StartedAt,
                    LastMessageAt = c.LastMessageAt,
                    IsActive = c.IsActive,
                    Summary = c.Summary,
                    RecommendationIds = c.RecommendationIds
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active conversations for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Close a conversation
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="conversationId">The conversation ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Conversation closed</response>
    /// <response code="404">Conversation not found</response>
    [HttpPost("users/{userId}/{conversationId}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseConversation(Guid userId, Guid conversationId)
    {
        _logger.LogInformation("Closing conversation {ConversationId} for user {UserId}", conversationId, userId);

        try
        {
            var conversation = await _conversationRepository.GetByIdAsync(conversationId, userId);
            if (conversation == null)
                return NotFound(new { message = "Conversation not found" });

            await _conversationRepository.CloseAsync(conversationId);
            return Ok(new { message = "Conversation closed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing conversation {ConversationId}", conversationId);
            throw;
        }
    }

    /// <summary>
    /// Generate investment recommendation within a conversation
    /// This integrates recommendation generation into the conversation flow
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="conversationId">The conversation ID</param>
    /// <param name="request">Recommendation request with optional analysis context</param>
    /// <returns>Generated recommendation integrated into conversation</returns>
    /// <response code="200">Recommendation generated successfully</response>
    /// <response code="404">Conversation not found</response>
    [HttpPost("users/{userId}/{conversationId}/recommendation")]
    [ProducesResponseType(typeof(RecommendationInConversationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateRecommendationInConversation(
        Guid userId,
        Guid conversationId,
        [FromBody] GenerateRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Generating recommendation in conversation {ConversationId} for user {UserId}",
            conversationId,
            userId);

        try
        {
            var command = new GenerateRecommendationInConversationCommand
            {
                UserId = userId,
                ConversationId = conversationId,
                AnalysisContext = request.AnalysisContext,
                Provider = request.Provider,
                ModelName = request.ModelName
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Conversation not found");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendation in conversation for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Search conversations by query
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="query">Search query</param>
    /// <returns>Matching conversations</returns>
    /// <response code="200">Search results</response>
    [HttpGet("users/{userId}/search")]
    [ProducesResponseType(typeof(List<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchConversations(Guid userId, [FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Query cannot be empty" });

        _logger.LogInformation("Searching conversations for user {UserId} with query: {Query}", userId, query);

        try
        {
            var conversations = await _conversationRepository.SearchAsync(userId, query);

            var dtos = conversations
                .Select(c => new ConversationDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Topic = c.Topic,
                    MessageCount = c.Messages.Count,
                    StartedAt = c.StartedAt,
                    LastMessageAt = c.LastMessageAt,
                    Summary = c.Summary
                })
                .ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching conversations");
            throw;
        }
    }
}
