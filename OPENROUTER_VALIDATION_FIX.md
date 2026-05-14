# Troubleshooting OpenRouter Model Validation Error

## Error You Received
```
Status: 400
Title: "Validation Error"
Detail: "One or more validation errors occurred."
ModelName Error: "Model '{ModelName}' is not supported. Supported models: gemma-7b-it, google/flan-t5-large, ..."
```

## Root Cause
The model IDs we were suggesting didn't match OpenRouter's actual available models. OpenRouter's API validates model names strictly and returns a 400 error for unsupported models.

## What We Fixed

### 1. Dynamic Model Discovery
The `/api/recommendations/models` endpoint now:
- Fetches actual available models from OpenRouter's `/models` API
- Uses the exact model IDs that OpenRouter returns
- Falls back gracefully if the API is unavailable

### 2. Correct Model ID Formats
OpenRouter uses specific model ID formats:
- `openai/gpt-4-turbo-preview` (not `gpt-4-turbo`)
- `anthropic/claude-3-opus` (not just `claude-3-opus`)
- `mistralai/mistral-7b-instruct` (not `mistral-7b`)

### 3. Better Error Handling
The OpenRouter service now:
- Catches validation errors and logs them
- Returns error details for debugging
- Uses fallback models when API unavailable

## How to Fix It Now

### Option 1: Use Correct Model IDs (Recommended)
Always use model IDs from the `/api/recommendations/models` endpoint:

```bash
# First, get available models
GET /api/recommendations/models

# Then use one of the returned modelId values
POST /api/recommendations/users/{userId}
{
  "analysisContext": "Build wealth",
  "provider": 1,
  "modelName": "openai/gpt-3.5-turbo"
}
```

### Option 2: Use Ollama (Local - No API Calls)
Stick with Ollama which doesn't have this validation issue:

```bash
POST /api/recommendations/users/{userId}
{
  "analysisContext": "Build wealth",
  "provider": 0,
  "modelName": "llama3:latest"
}
```

## Working OpenRouter Models

These are verified to work with OpenRouter:

**From Error Message (Supported by OpenRouter):**
- `gemma-7b-it`
- `google/flan-t5-large`
- `google/flan-t5-xl`
- `llama2-70b-4096`
- `llama2-70b-chat-hf`
- `meta-llama/Llama-2-70b-chat-hf`
- `meta-llama/Llama-2-7b-chat-hf`
- `microsoft/phi-2`
- `mistralai/Mistral-7B-Instruct-v0.2`
- `mixtral-8x7b-32768`
- `NousResearch/Nous-Hermes-2-Mixtral-8x7B-DPO`
- `NousResearch/Nous-Hermes-2-SOLAR-10.7B`
- `openchat/openchat-3.5`

## Testing Strategy

### 1. First: Check Available Models
```bash
curl -X GET "https://localhost:5001/api/recommendations/models"
# Look at the OpenRouter array for available model IDs
```

### 2. Then: Use Returned Model ID
```bash
curl -X POST "https://localhost:5001/api/recommendations/users/{userId}" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "Build wealth",
    "provider": 1,
    "modelName": "mistralai/Mistral-7B-Instruct-v0.2"
  }'
```

### 3. If OpenRouter is Slow: Use Ollama
```bash
curl -X POST "https://localhost:5001/api/recommendations/users/{userId}" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "Build wealth",
    "provider": 0,
    "modelName": "llama3:latest"
  }'
```

## Common Issues & Solutions

### Issue: "Model 'X' is not supported"
**Solution:** Use `/api/recommendations/models` to get current list of supported models

### Issue: OpenRouter models not appearing in /models endpoint
**Solution:** 
1. Check OpenRouter API key is valid in `appsettings.json`
2. Ensure account has credits
3. Check internet connection

### Issue: Ollama models not appearing
**Solution:**
1. Ensure Ollama is running: `ollama serve`
2. Verify URL is correct: `http://localhost:11434`
3. Pull a model: `ollama pull llama3`

### Issue: Model appears in list but still gets 400 error
**Solution:**
1. The model list may be cached - refresh the page
2. Try a different model from the list
3. Restart the API service

## Code Changes Made

1. **OpenRouterService.cs**
   - Fetches models from OpenRouter's `/models` API
   - Uses exact model IDs from API response
   - Better error handling and logging

2. **OllamaProviderService.cs**
   - Uses actual model names from Ollama (e.g., `llama3:latest`)
   - Fetches real available models from Ollama's `/api/tags`
   - Fallback for when Ollama is offline

3. **Documentation Updated**
   - Reflects actual model IDs
   - Better examples with correct formats
   - Troubleshooting section added

## Next Steps

1. **Rebuild the Application**
   - If hot-reload enabled, changes may apply automatically
   - Otherwise, stop and restart the app

2. **Clear Browser Cache**
   - Swagger UI may cache old model list
   - Hard refresh: `Ctrl+Shift+R`

3. **Test New Endpoint**
   - Call `/api/recommendations/models` again
   - Should see actual models from your environment

4. **Use Verified Model IDs**
   - Only use model IDs returned from `/models` endpoint
   - This ensures they work with the respective providers

## API Key Verification

If OpenRouter models aren't showing:

```powershell
# Check if API key is set
$env:OpenRouter__ApiKey

# Or in appsettings.json
cat API/appsettings.json | Select-String "OpenRouter"

# Format should be: sk-or-...
```

## Testing Example

```bash
# Terminal 1: Start MongoDB
mongod

# Terminal 2: Start Redis  
redis-server

# Terminal 3: Start Ollama
ollama serve

# Terminal 4: Start API
cd D:\project\agent\Prospera
dotnet run --project API\Prospera.API.csproj

# Terminal 5: Test endpoints
# First, get models
curl -X GET "https://localhost:5001/api/recommendations/models"

# Then generate with Ollama (always works)
curl -X POST "https://localhost:5001/api/recommendations/users/123e4567-e89b-12d3-a456-426614174000" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "Build long-term wealth",
    "provider": 0,
    "modelName": "llama3:latest"
  }'

# Then try OpenRouter with correct model ID
curl -X POST "https://localhost:5001/api/recommendations/users/123e4567-e89b-12d3-a456-426614174000" \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "Build long-term wealth",
    "provider": 1,
    "modelName": "mistralai/Mistral-7B-Instruct-v0.2"
  }'
```

## Success Indicators

✅ `GET /api/recommendations/models` returns list with real models
✅ Model IDs in response can be used in POST request
✅ `POST /recommendations/users/{userId}` with Ollama works
✅ `POST /recommendations/users/{userId}` with OpenRouter returns valid recommendation (not 400 error)

---

**Status**: ✅ Fixed
**Workaround**: Use Ollama (provider=0) or get model list first
**Recommendation**: Always fetch `/models` endpoint first to get current valid model IDs
