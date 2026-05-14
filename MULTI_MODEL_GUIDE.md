# Multi-Model Recommendation Endpoint - Setup & Usage Guide

## Overview

The Prospera recommendation endpoint now supports **multiple LLM providers and models**, allowing you to choose between:
- **Ollama** (Local, free, privacy-first)
- **OpenRouter** (Cloud-based, premium models like GPT-4, Claude 3)

This gives you flexibility to balance cost, quality, latency, and privacy based on your needs.

## Quick Start

### Default Behavior (Ollama)
If you don't specify a provider, the system defaults to **Ollama** running on `http://localhost:11434`:

```bash
# No model selection - uses Ollama with llama3.2
POST /api/recommendations/users/{userId}
{
  "analysisContext": "Build long-term wealth with balanced growth"
}
```

### Using Ollama with a Specific Model
```bash
POST /api/recommendations/users/{userId}
{
  "analysisContext": "Build long-term wealth with balanced growth",
  "provider": 0,  # or use enum: Ollama
  "modelName": "mistral"  # or "llama3.2", "neural-chat", etc.
}
```

### Using OpenRouter (Cloud Models)
```bash
POST /api/recommendations/users/{userId}
{
  "analysisContext": "Build long-term wealth with balanced growth",
  "provider": 1,  # or use enum: OpenRouter
  "modelName": "openai/gpt-3.5-turbo"  # or "anthropic/claude-3-opus", etc.
}
```

## Setup Instructions

### 1. Prerequisites

#### For Ollama (Local)
```powershell
# Install Ollama from https://ollama.ai
# Start Ollama server
ollama serve

# In another terminal, pull a model
ollama pull llama3.2
ollama pull mistral  # optional additional model
```

#### For OpenRouter (Cloud)
1. Sign up at https://openrouter.ai
2. Get your API key from the dashboard
3. Fund your account (free credits available for new users)

### 2. Configuration

#### Option A: Using appsettings.json

Create/update `API/appsettings.json`:

```json
{
  "Ollama": {
    "Url": "http://localhost:11434",
    "Model": "llama3.2"
  },
  "OpenRouter": {
    "Url": "https://openrouter.ai/api/v1",
    "ApiKey": "your-api-key-here"
  }
}
```

#### Option B: Using Environment Variables

```powershell
# Ollama configuration
$env:Ollama__Url = "http://localhost:11434"
$env:Ollama__Model = "llama3.2"

# OpenRouter configuration
$env:OpenRouter__Url = "https://openrouter.ai/api/v1"
$env:OpenRouter__ApiKey = "your-api-key-here"

# Then start the API
dotnet run --project API\Prospera.API.csproj
```

### 3. Start Services

#### Terminal 1: MongoDB
```powershell
mongod
```

#### Terminal 2: Redis
```powershell
redis-server
```

#### Terminal 3: Ollama (Optional - only if using Ollama)
```powershell
ollama serve
```

#### Terminal 4: API
```powershell
cd D:\project\agent\Prospera
dotnet run --project API\Prospera.API.csproj
```

Visit: `https://localhost:5001/swagger`

## Available Models

### Get All Available Models

```http
GET /api/recommendations/models
```

Response includes:
- All available Ollama models (if Ollama is running)
- All available OpenRouter models (if API key configured)
- Recommended models for different use cases
- Model details: cost, context window, availability

Example response:
```json
{
  "providers": {
    "Ollama": [
      {
        "modelId": "llama3.2",
        "displayName": "Llama 3.2",
        "provider": "Ollama (Meta)",
        "description": "Latest Llama model",
        "isAvailable": true,
        "costPer1kTokens": null,
        "contextWindow": 8192
      }
    ],
    "OpenRouter": [
      {
        "modelId": "openai/gpt-4-turbo-preview",
        "displayName": "GPT-4 Turbo",
        "provider": "OpenRouter (OpenAI)",
        "description": "Most capable model with 128K context",
        "isAvailable": true,
        "costPer1kTokens": 0.01,
        "contextWindow": 128000
      }
    ]
  },
  "defaultProvider": "Ollama",
  "recommendedModels": [
    {
      "category": "Best for Local/Offline (Recommended)",
      "modelId": "llama3.2",
      "displayName": "Llama 3.2",
      "provider": "Ollama",
      "reason": "State-of-the-art open-source model, runs locally, no cost, good balance of quality and speed"
    }
  ]
}
```

### Ollama Models

**Local (Free, No Costs):**
- `llama3.2` - Latest, most capable
- `mistral` - Fast, efficient 7B model
- `neural-chat` - Optimized for conversation
- `dolphin-mixtral` - High quality mixture of experts (8x7B)

### OpenRouter Models

**High Quality:**
- `openai/gpt-4-turbo-preview` - Most capable ($0.01/1k tokens)
- `openai/gpt-4` - Original GPT-4
- `anthropic/claude-3-opus` - Excellent reasoning
- `anthropic/claude-3-sonnet` - Balanced quality/cost ($0.003/1k)

**Budget-Friendly:**
- `openai/gpt-3.5-turbo` - Fast, affordable ($0.0005/1k tokens)
- `mistralai/mistral-7b-instruct` - Open-source via cloud ($0.00014/1k)

## Example Requests

### Example 1: Generate with Ollama (Local)

```bash
curl -X POST "https://localhost:5001/api/recommendations/users/123e4567-e89b-12d3-a456-426614174000" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "I want to build long-term wealth while managing my mortgage. Looking for balanced growth with some real estate stability.",
    "provider": 0,
    "modelName": "llama3.2"
  }'
```

Response:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "allocation": "40% Stocks, 30% Bonds, 15% Real Estate, 10% Crypto, 5% Cash",
  "explanation": "This allocation is your personalized roadmap for building wealth with a balanced approach to growth and protection. With positive market momentum in your favor, this diversified mix positions you for success...",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### Example 2: Generate with OpenRouter (Cloud - GPT-4)

```bash
curl -X POST "https://localhost:5001/api/recommendations/users/123e4567-e89b-12d3-a456-426614174000" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "I want to build long-term wealth while managing my mortgage. Looking for balanced growth with some real estate stability.",
    "provider": 1,
    "modelName": "openai/gpt-4-turbo-preview"
  }'
```

### Example 3: Using Default (No Model Specified)

```bash
curl -X POST "https://localhost:5001/api/recommendations/users/123e4567-e89b-12d3-a456-426614174000" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "I want to build long-term wealth while managing my mortgage."
  }'
# Uses Ollama with llama3.2 by default
```

## Performance & Cost Comparison

| Model | Provider | Speed | Quality | Cost | Privacy |
|-------|----------|-------|---------|------|---------|
| Llama 3.2 | Ollama | Fast | High | Free | Local |
| Mistral 7B | Ollama | Very Fast | Good | Free | Local |
| Claude 3 Sonnet | OpenRouter | Medium | Very High | Low | Cloud |
| GPT-3.5 Turbo | OpenRouter | Fast | High | Very Low | Cloud |
| GPT-4 Turbo | OpenRouter | Medium | Excellent | Medium | Cloud |

**Recommendation for Most Users:**
- **Development/Testing**: Use Ollama + llama3.2 (free, local, fast)
- **Production Quality**: Use OpenRouter + Claude 3 Sonnet (balanced)
- **Maximum Quality**: Use OpenRouter + GPT-4 Turbo (best recommendations)

## Troubleshooting

### Error: "Unable to connect to Ollama"
**Solution:**
1. Ensure Ollama is running: `ollama serve`
2. Check URL in appsettings.json (default: `http://localhost:11434`)
3. If custom URL, verify it's accessible

### Error: "OpenRouter API Key invalid"
**Solution:**
1. Verify API key from https://openrouter.ai
2. Check it's set in appsettings.json: `"ApiKey": "your-key-here"`
3. Ensure account has sufficient credits

### Error: "Model not found"
**Solution:**
1. For Ollama: Pull the model first `ollama pull llama3.2`
2. For OpenRouter: Check the model ID format (e.g., `openai/gpt-4`)
3. Call `/api/recommendations/models` to see available options

### Recommendation quality seems low
**Solution:**
1. Try a different model from the recommendations list
2. Use OpenRouter models for higher quality (GPT-4, Claude 3)
3. Ensure you provide good context in `analysisContext`

## API Enum Values

```csharp
public enum LlmProvider
{
    Ollama = 0,
    OpenRouter = 1
}
```

Use either the numeric value or the name when sending requests.

## Advanced Usage

### Custom Ollama Endpoint
If running Ollama on a different machine:

```json
{
  "analysisContext": "...",
  "provider": 0,
  "modelName": "mistral",
  "customEndpoint": "http://192.168.1.100:11434"
}
```

### Batch Operations
Generate recommendations with multiple providers for comparison:

```bash
# Generate with Ollama
POST /api/recommendations/users/{userId}
{ "analysisContext": "...", "provider": 0, "modelName": "llama3.2" }

# Generate with OpenRouter
POST /api/recommendations/users/{userId}
{ "analysisContext": "...", "provider": 1, "modelName": "openai/gpt-3.5-turbo" }

# Compare allocations and explanations
GET /api/recommendations/users/{userId}
```

## Integration with Coaching

The recommendation endpoint integrates with the Prospera coaching system:
- **Coaching Prompts**: Recommendations are generated with coaching language
- **Holistic Guidance**: Considers both assets and liabilities
- **Action-Oriented**: Includes specific, motivational guidance
- **Mentoring Tone**: Uses encouraging, mentor-like language

All models use the same coaching-focused prompt structure, so quality varies mainly by model capability.

## Next Steps

1. **Test with Swagger UI**: Navigate to `/swagger` and try the endpoints
2. **Monitor Costs**: Track OpenRouter usage in the dashboard
3. **Experiment**: Compare quality/speed of different models
4. **Optimize**: Choose the model that best fits your use case
5. **Deploy**: Use Ollama for production if privacy is priority, OpenRouter for maximum quality

---

**Last Updated**: Multi-Model Support Release
**Status**: ✅ Ready for Production
