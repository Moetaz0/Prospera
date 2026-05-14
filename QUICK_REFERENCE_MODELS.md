# Quick Reference - Multi-Model Endpoints

## ⚠️ IMPORTANT: Use Correct Model IDs

The model IDs must match exactly what your provider supports. See `YOUR_MODELS_AVAILABLE.md` for models available in your environment.

## Available Models Endpoint

### Get All Models
```
GET /api/recommendations/models
```

**Response includes:**
- **Ollama**: Models currently available on your machine
  - Currently: `llama3:latest` (and any others you've pulled)
- **OpenRouter**: Models available with your API key
  - See `YOUR_MODELS_AVAILABLE.md` for confirmed list

### Example Call
```bash
curl "https://localhost:5001/api/recommendations/models"
```

Response shows real available models with:
- Exact model IDs to use
- Display names
- Provider info
- Cost information
- Recommended models

---

## Generate Recommendation - With Model Selection

### Endpoint
```
POST /api/recommendations/users/{userId}
```

### Request Body

```json
{
  "analysisContext": "Your investment goal",
  "provider": 0,
  "modelName": "llama3:latest",
  "customEndpoint": null
}
```

### Parameters

| Parameter | Type | Required | Default | Notes |
|-----------|------|----------|---------|-------|
| analysisContext | string | Yes | - | Your investment goal/context |
| provider | enum | No | Ollama (0) | 0=Ollama, 1=OpenRouter |
| modelName | string | No | Config default | **Must be exact model ID from `/models`** |
| customEndpoint | string | No | null | Override provider endpoint |

### Response

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "allocation": "40% Stocks, 30% Bonds, 15% Real Estate, 10% Crypto, 5% Cash",
  "explanation": "This allocation is your personalized roadmap...",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

---

## Quick Examples - USE EXACT MODEL IDS

### ✅ Local Ollama (Always Works - FREE)
```bash
curl -X POST "https://localhost:5001/api/recommendations/users/{userId}" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "Build long-term wealth",
    "provider": 0,
    "modelName": "llama3:latest"
  }'
```

### ✅ Cloud GPT-3.5 (Fast & Cheap - ~$0.001)
```bash
curl -X POST "https://localhost:5001/api/recommendations/users/{userId}" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "Build long-term wealth",
    "provider": 1,
    "modelName": "openai/gpt-3.5-turbo"
  }'
```

### ✅ Cloud Claude 3 Sonnet (Best Value - ~$0.004)
```bash
curl -X POST "https://localhost:5001/api/recommendations/users/{userId}" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "Build long-term wealth",
    "provider": 1,
    "modelName": "anthropic/claude-3-sonnet"
  }'
```

### ✅ Cloud GPT-4 (Best Quality - ~$0.012)
```bash
curl -X POST "https://localhost:5001/api/recommendations/users/{userId}" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "Build long-term wealth",
    "provider": 1,
    "modelName": "openai/gpt-4-turbo-preview"
  }'
```

### ❌ Default (No Model Specified)
```bash
curl -X POST "https://localhost:5001/api/recommendations/users/{userId}" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "Build long-term wealth"
  }'
# Uses provider=Ollama + configured default model
```

---

## Getting Actual Model IDs (IMPORTANT!)

### Always Call /models First
```bash
# Get currently available models
GET /api/recommendations/models

# Response contains actual model IDs you can use
# Copy model IDs from response - don't guess!
```

### Why This Matters
- **Old Docs**: Suggested `llama3.2`, `gpt-4` 
- **Actual IDs**: Must be `llama3:latest`, `openai/gpt-4-turbo-preview`
- **Error**: Using wrong ID = 400 Validation Error
- **Solution**: Use exact IDs from `/models` endpoint

---

## Correct Model ID Formats

### Ollama Models
Use format: `{model-name}:latest` or `{model-name}:{tag}`

Examples:
- ✅ `llama3:latest`
- ✅ `mistral:latest`
- ❌ `llama3.2` (wrong - no tag)
- ❌ `mistral` (wrong - no tag)

### OpenRouter Models
Use format: `{provider}/{model-name}`

Examples:
- ✅ `openai/gpt-3.5-turbo`
- ✅ `anthropic/claude-3-sonnet`
- ✅ `mistralai/Mistral-7B-Instruct-v0.2`
- ❌ `gpt-3.5-turbo` (wrong - missing provider)
- ❌ `claude-3-sonnet` (wrong - missing provider)

---

## Model Selection Guide

| Use Case | Provider | Model | Why |
|----------|----------|-------|-----|
| **Development** | Ollama | llama3:latest | Free, always available |
| **Testing** | Ollama | Any local model | Fast iteration, no costs |
| **Budget Production** | OpenRouter | openai/gpt-3.5-turbo | Cheapest cloud ($0.0005/k tokens) |
| **Best Value** | OpenRouter | anthropic/claude-3-sonnet | Best quality/cost ($0.003/k tokens) |
| **Best Quality** | OpenRouter | openai/gpt-4-turbo-preview | Most capable ($0.01/k tokens) |
| **Privacy** | Ollama | Any | Runs locally, no data sent |

---

## Configuration

### 1. Ollama Setup
```powershell
# Already running? Check:
curl http://localhost:11434/api/tags

# If not, start it:
ollama serve

# List available models:
ollama list

# Pull more models:
ollama pull mistral:latest
ollama pull neural-chat:latest
```

### 2. OpenRouter Setup
- Sign up: https://openrouter.ai
- Get API key: https://openrouter.ai/account/keys
- Add to `appsettings.json`:
```json
{
  "OpenRouter": {
    "Url": "https://openrouter.ai/api/v1",
    "ApiKey": "sk-or-your-actual-key"
  }
}
```
Or set environment variable:
```powershell
$env:OpenRouter__ApiKey = "sk-or-your-actual-key"
```

### 3. Start Services
```powershell
# Terminal 1
mongod

# Terminal 2
redis-server

# Terminal 3 (if using Ollama)
ollama serve

# Terminal 4
dotnet run --project API\Prospera.API.csproj
```

---

## Error Handling

| Error | Cause | Solution |
|-------|-------|----------|
| "Model 'X' is not supported" | Wrong model ID format | Use exact ID from `/models` endpoint |
| "Unable to connect to Ollama" | Ollama not running | Start: `ollama serve` |
| "OpenRouter API Key invalid" | Wrong/missing key | Check key at https://openrouter.ai |
| "Model not found" | Typo in model name | Copy-paste from `/models` response |

---

## Testing Workflow

### 1. Check Available Models
```bash
curl -X GET "https://localhost:5001/api/recommendations/models" | jq '.providers'
```

### 2. Pick a Model
Copy the exact `modelId` from response

### 3. Test Generation
```bash
curl -X POST "https://localhost:5001/api/recommendations/users/{userId}" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "Build long-term wealth",
    "provider": 0,
    "modelName": "llama3:latest"
  }'
```

### 4. Verify Success
- Status should be 201 or 200
- Response should include `allocation` and `explanation`
- If 400 error, check model ID format

---

## Testing in Swagger

1. Open: `https://localhost:5001/swagger`
2. Expand **Recommendations** section
3. Click **GET /api/recommendations/models**
   - Execute
   - Copy a modelId from response
4. Click **POST /api/recommendations/users/{userId}**
   - Enter userId
   - Paste copied modelId in `modelName` field
   - Set `provider` (0=Ollama, 1=OpenRouter)
   - Click Execute
   - See recommendation

---

## Performance Estimates

| Model | Provider | Speed | Quality | Cost/k tokens |
|-------|----------|-------|---------|---------------|
| llama3:latest | Ollama | 10-15s | Good | Free |
| gpt-3.5-turbo | OpenRouter | 3-5s | Good | $0.0005 |
| claude-3-sonnet | OpenRouter | 5-8s | Excellent | $0.003 |
| gpt-4-turbo | OpenRouter | 8-12s | Excellent | $0.01 |

---

## Key Takeaways

1. ✅ **Always get /models first** to see what's available
2. ✅ **Use exact model IDs** from the response
3. ✅ **Start with Ollama** for free testing
4. ✅ **Use OpenRouter** only when you need cloud quality
5. ✅ **Monitor spending** at https://openrouter.ai/account/billing

---

## Documentation

- **YOUR_MODELS_AVAILABLE.md** - Your current environment setup
- **MULTI_MODEL_GUIDE.md** - Detailed setup guide
- **OPENROUTER_FIX_SUMMARY.md** - What was fixed
- **OPENROUTER_VALIDATION_FIX.md** - Detailed troubleshooting

---

**Status**: ✅ Production Ready  
**Last Updated**: Model ID format fixed  
**Recommendation**: See YOUR_MODELS_AVAILABLE.md for your setup
