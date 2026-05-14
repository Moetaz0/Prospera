using MediatR;
using AutoMapper;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;
using Prospera.Domain.Common;
using Prospera.Application.DTOs;
using Prospera.Application.DTOs.Conversations;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.Features.Conversations.Commands;
using Prospera.Application.Features.Recommendations.Commands;

namespace Prospera.Application.Features.Conversations.Handlers;

/// <summary>
/// Handler for general financial chatbot questions
/// Maintains conversation context and provides natural, conversational responses
/// This is NOT a recommendation - just a helpful financial advisor chatbot
/// </summary>
public class AskFinancialChatbotCommandHandler : IRequestHandler<AskFinancialChatbotCommand, ChatbotResponseDto>
{
    private readonly IMediator _mediator;
    private readonly IConversationRepository _conversationRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILlmProviderFactory _llmProviderFactory;
    private readonly IOllamaConfiguration _ollamaConfig;
    private readonly IFinancialDataService _financialDataService;

    public AskFinancialChatbotCommandHandler(
        IMediator mediator,
        IConversationRepository conversationRepository,
        IApplicationDbContext dbContext,
        IMapper mapper,
        ILlmProviderFactory llmProviderFactory,
        IOllamaConfiguration ollamaConfig,
        IFinancialDataService financialDataService)
    {
        _mediator = mediator;
        _conversationRepository = conversationRepository;
        _dbContext = dbContext;
        _mapper = mapper;
        _llmProviderFactory = llmProviderFactory;
        _ollamaConfig = ollamaConfig;
        _financialDataService = financialDataService;
    }

    public async Task<ChatbotResponseDto> Handle(AskFinancialChatbotCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get or create conversation
            Conversation conversation;
            if (request.ConversationId.HasValue)
            {
                conversation = await _conversationRepository.GetByIdAsync(request.ConversationId.Value, request.UserId);
                if (conversation == null)
                    throw new KeyNotFoundException($"Conversation {request.ConversationId} not found");
            }
            else
            {
                var topic = request.Topic ?? InferTopic(request.Question);
                conversation = new Conversation(request.UserId, topic, request.Question);
            }

            // Add user message
            var userMessage = new ConversationMessage("User", request.Question, "Question");
            conversation.AddMessage(userMessage);

            // Smart detection: if this is a recommendation request, route to recommendation pipeline
            if (IsRecommendationRequest(request.Question))
            {
                try
                {
                    var recommendationCommand = new GenerateRecommendationCommand
                    {
                        UserId = request.UserId,
                        AnalysisContext = request.Question,
                        Provider = (LlmProvider)request.Provider,
                        ModelName = request.ModelName
                    };

                    var recommendation = await _mediator.Send(recommendationCommand, cancellationToken);

                    conversation.AddRecommendation(recommendation.Id);

                    var recommendationMessageText = BuildRecommendationResponse(recommendation);
                    var recommendationMessage = new ConversationMessage("AI", recommendationMessageText, "Recommendation")
                    {
                        RecommendationId = recommendation.Id
                    };

                    conversation.AddMessage(recommendationMessage);

                    if (request.ConversationId.HasValue)
                    {
                        await _conversationRepository.UpdateAsync(conversation);
                    }
                    else
                    {
                        await _conversationRepository.AddAsync(conversation);
                    }

                    return new ChatbotResponseDto
                    {
                        ConversationId = conversation.Id,
                        MessageId = recommendationMessage.Id,
                        Response = recommendationMessageText,
                        CreatedAt = recommendationMessage.CreatedAt,
                        MessageType = "Recommendation",
                        IsRecommendation = true,
                        RecommendationId = recommendation.Id
                    };
                }
                catch (Exception recEx)
                {
                    // If recommendation generation fails, fall back to conversational response
                    var fallbackMessage = $"I'd like to help with a detailed recommendation for your portfolio, but I encountered a temporary issue. Let me provide some insights instead:\n\n{GenerateFallbackResponse(request.Question, conversation)}";
                    var fallbackAiMessage = new ConversationMessage("AI", fallbackMessage, "Response");
                    conversation.AddMessage(fallbackAiMessage);

                    if (request.ConversationId.HasValue)
                    {
                        await _conversationRepository.UpdateAsync(conversation);
                    }
                    else
                    {
                        await _conversationRepository.AddAsync(conversation);
                    }

                    return new ChatbotResponseDto
                    {
                        ConversationId = conversation.Id,
                        MessageId = fallbackAiMessage.Id,
                        Response = fallbackMessage,
                        CreatedAt = fallbackAiMessage.CreatedAt,
                        MessageType = "Response",
                        IsRecommendation = false,
                        RecommendationId = null
                    };
                }
            }

            // Build chatbot prompt with conversation context
            var chatbotPrompt = BuildChatbotPrompt(request.Question, conversation);

            // Get LLM provider and model
            var provider = await _llmProviderFactory.GetProviderAsync((LlmProvider)request.Provider);
            var model = !string.IsNullOrWhiteSpace(request.ModelName)
                ? request.ModelName
                : GetDefaultModel((LlmProvider)request.Provider);

            // Call LLM for response
            string aiResponse;
            try
            {
                aiResponse = await provider.GenerateResponseAsync(chatbotPrompt, model, cancellationToken);
            }
            catch (Exception ex)
            {
                aiResponse = GenerateFallbackResponse(request.Question, conversation);
            }

            // Add AI response to conversation
            var aiMessage = new ConversationMessage("AI", aiResponse, "Response");
            conversation.AddMessage(aiMessage);

            // Save conversation
            if (request.ConversationId.HasValue)
            {
                await _conversationRepository.UpdateAsync(conversation);
            }
            else
            {
                await _conversationRepository.AddAsync(conversation);
            }

            // Return the response with conversation context
            return new ChatbotResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = aiMessage.Id,
                Response = aiResponse,
                CreatedAt = aiMessage.CreatedAt,
                MessageType = "Response",
                IsRecommendation = false,
                RecommendationId = null
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private string BuildChatbotPrompt(string userQuestion, Conversation conversation)
    {
        var conversationHistory = string.Join("\n",
            conversation.Messages
                .TakeLast(5)
                .Select(m => $"{m.SenderType}: {m.Content}"));

        return $@"You are a friendly and knowledgeable FINANCIAL ADVISOR chatbot.
Your role is to answer general financial questions, provide market insights, and help users understand financial concepts.
You are NOT a portfolio advisor - that's a separate service where users request recommendations explicitly.
You're here to educate and discuss finance naturally, NOT to generate investment recommendations.

IMPORTANT: This is a general Q&A conversation, NOT a recommendation service.
- Do NOT provide portfolio allocation percentages
- Do NOT recommend specific asset allocations
- Do NOT create investment recommendations
- Simply answer the question conversationally and educationally

CONVERSATION CONTEXT:
{conversationHistory}

USER'S LATEST QUESTION: {userQuestion}

RESPONSE GUIDELINES:
- Provide clear, conversational answers (not formal reports)
- Use real examples and analogies when helpful
- Explain concepts in simple terms
- Acknowledge when something is complex and break it down
- Ask follow-up questions if needed to better help the user
- Stay friendly and encouraging
- If discussing markets, mention that past performance isn't indicative of future results
- Keep responses concise but informative (2-4 paragraphs)

Respond naturally as a financial advisor chatbot would:";
    }

    private string InferTopic(string question)
    {
        var lowerQ = question.ToLower();

        if (lowerQ.Contains("stock") || lowerQ.Contains("equity") || lowerQ.Contains("market"))
            return "Market";
        else if (lowerQ.Contains("portfolio") || lowerQ.Contains("allocat"))
            return "Portfolio";
        else if (lowerQ.Contains("save") || lowerQ.Contains("emergency") || lowerQ.Contains("fund"))
            return "Savings";
        else if (lowerQ.Contains("invest") || lowerQ.Contains("return"))
            return "Investment";
        else if (lowerQ.Contains("debt") || lowerQ.Contains("loan") || lowerQ.Contains("credit"))
            return "Debt";
        else if (lowerQ.Contains("tax") || lowerQ.Contains("inflation"))
            return "Economics";
        else if (lowerQ.Contains("bitcoin") || lowerQ.Contains("crypto"))
            return "Crypto";
        else
            return "General";
    }

    private bool IsRecommendationRequest(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            return false;

        var lowerQ = question.ToLower();
        var recommendationKeywords = new[]
        {
            "recommend",
            "allocation",
            "portfolio",
            "rebalance",
            "analyze",
            "analysis",
            "suggest",
            "strategy",
            "should i",
            "what should i",
            "risk profile",
            "asset mix",
            "diversif",
            "invest",
            "buy",
            "sell"
        };

        return recommendationKeywords.Any(keyword => lowerQ.Contains(keyword));
    }

    private string BuildRecommendationResponse(InvestmentRecommendationDto recommendation)
    {
        if (!string.IsNullOrWhiteSpace(recommendation.Explanation))
            return recommendation.Explanation;

        return "I generated a personalized recommendation based on your profile and request. Review the allocation and let me know if you want adjustments.";
    }

    private string GenerateFallbackResponse(string question, Conversation conversation)
    {
        var topic = conversation.Topic;

        return $@"I'd be happy to help with your question about {topic}!

Based on what you're asking: ""{question}""

Here are some general insights:
- Financial decisions should be based on your personal situation and goals
- Diversification is important for managing risk
- It's good to educate yourself before making investment decisions
- Consider consulting with a financial professional for personalized advice

Is there a specific aspect of {topic} you'd like to explore further? I can provide more detailed information on particular topics.";
    }

    private string GetDefaultModel(LlmProvider provider)
    {
        return provider switch
        {
            LlmProvider.Ollama => _ollamaConfig.Model,
            LlmProvider.OpenRouter => "openai/gpt-3.5-turbo",
            _ => _ollamaConfig.Model
        };
    }
}
