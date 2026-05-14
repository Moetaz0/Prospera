# Model Selection for Recommendation Generation

This document explains how to use the model selection feature when generating investment recommendations via the Prospera API.

## Overview

Users can now specify which AI model to use when generating investment recommendations. If no model is specified, the API will use the default model configured in `appsettings.json`.

## API Usage

### Basic Usage (Default Model)

```bash
POST /api/Recommendations/users/{userId}
Content-Type: application/json

{
  "analysisContext": "I want to invest $50,000 for retirement over 20 years"
}
```

### With Model Selection

```bash
POST /api/Recommendations/users/{userId}
Content-Type: application/json

{
  "analysisContext": "I want to invest $50,000 for retirement over 20 years",
  "modelName": "mixtral-8x7b-32768"
}
```

## Supported Models

### Groq Models
The following models are available via the Groq API (free tier, unlimited access):

- **mixtral-8x7b-32768** - Fast, high-quality reasoning (Recommended for most use cases)
- **llama2-70b-4096** - Powerful open-source LLM
- **llama2-70b-chat-hf** - Chat-optimized Llama 2
- **gemma-7b-it** - Compact, efficient model

### HuggingFace Models
The following models are available via HuggingFace Inference API (free tier with limits):

- **mistralai/Mistral-7B-Instruct-v0.2** - Fast and efficient Mistral variant
- **meta-llama/Llama-2-7b-chat-hf** - 7B chat-optimized Llama 2
- **meta-llama/Llama-2-70b-chat-hf** - 70B powerful Llama 2
- **google/flan-t5-large** - Efficient encoder-decoder model
- **google/flan-t5-xl** - Larger FLAN-T5 variant
- **microsoft/phi-2** - Compact Microsoft model
- **openchat/openchat-3.5** - OpenChat 3.5
- **NousResearch/Nous-Hermes-2-Mixtral-8x7B-DPO** - Advanced Nous Hermes
- **NousResearch/Nous-Hermes-2-SOLAR-10.7B** - SOLAR 10.7B model

## Configuration

The default model and provider are configured in `API/appsettings.json`:

```json
{
  "FreeCloudAI": {
    "Provider": "groq",
    "ApiKey": "",
    "Model": "mixtral-8x7b-32768"
  }
}
```

The `Model` setting is used as the fallback when no `modelName` is provided in the request.

## Recommendation for Model Selection

- **For Most Users**: Use `mixtral-8x7b-32768` (Groq) - Fast, accurate, unlimited free tier
- **For Complex Analysis**: Use `llama2-70b-4096` or `llama2-70b-chat-hf` - More powerful but slightly slower
- **For Efficient Results**: Use `mistralai/Mistral-7B-Instruct-v0.2` (HuggingFace) - Good balance of speed and quality
- **For Budget Optimization**: Use `gemma-7b-it` - Smallest/fastest option

## Error Handling

If you specify an unsupported model name, the API will return a 400 Bad Request response:

```json
{
  "errors": {
    "ModelName": ["Model 'unsupported-model' is not supported. Supported models: gemma-7b-it, ..."]
  }
}
```

## Implementation Details

The model selection feature is implemented through:

1. **Request DTO** (`Prospera.Contracts/DTOs/Recommendations/GenerateInvestmentRecommendationRequest`): Accepts optional `modelName` parameter
2. **Command** (`GenerateRecommendationCommand`): Carries the `modelName` through the application layer
3. **Validator** (`GenerateRecommendationCommandValidator`): Validates the model name against the list of supported models
4. **Handler** (`GenerateRecommendationCommandHandler`): Uses the specified model or falls back to the configured default
5. **Controller** (`RecommendationsController`): Maps the request DTO to the command

## Example Request with cURL

```bash
curl -X POST https://api.prospera.local/api/Recommendations/users/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "analysisContext": "I am planning to retire in 25 years and want a balanced growth strategy",
    "modelName": "llama2-70b-4096"
  }'
```

## Response

Both requests (with or without model selection) return the same recommendation response:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "allocation": "45% Stocks, 30% Bonds, 15% Real Estate, 7% Crypto, 3% Cash",
  "explanation": "This allocation is tailored for your balanced investment approach...",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

## Monitoring and Debugging

When generating recommendations, check the application logs for:

- Selected model name (from command)
- Effective model used (specified or default)
- AI service response time and status
- Any fallback allocations due to AI service errors

## Future Enhancements

Potential improvements to the model selection feature:

- Model availability check endpoint
- Model performance metrics (speed, accuracy) per model
- User preference saving for model selection
- A/B testing different models for same user
- Ensemble recommendations using multiple models
- Cost estimation for model usage
