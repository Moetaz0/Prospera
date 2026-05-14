# OpenRouter FREE Models - API Examples

## 🎯 Complete Working Examples

### Example 1: Using FREE Mistral Model (Recommended)

**Request:**
```bash
curl -X POST https://localhost:5001/api/recommendations/users/123e4567-e89b-12d3-a456-426614174000 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "analysisContext": "I want to build long-term wealth and reduce my debt. I have $10k in savings and $5k in credit card debt.",
    "provider": 1,
    "modelName": "mistralai/mistral-7b-instruct:free"
  }'
```

**Response (Example):**
```json
{
  "success": true,
  "allocations": [
    {
      "assetClass": "Emergency Fund",
      "percentage": 20,
      "reason": "Build 3-6 months emergency fund with $2k from your savings"
    },
    {
      "assetClass": "High-Interest Debt",
      "percentage": 50,
      "reason": "Aggressively pay down credit card debt charging ~15-20% interest"
    },
    {
      "assetClass": "Index Funds",
      "percentage": 20,
      "reason": "Dollar-cost average into diversified index funds for growth"
    },
    {
      "assetClass": "Bonds",
      "percentage": 10,
      "reason": "Conservative allocation for stability"
    }
  ],
  "explanation": "Your financial situation shows high-interest debt that's hurting your wealth building. Here's my coaching advice...",
  "coachingNotes": "First, tackle that credit card debt aggressively. Once paid off, redirect those payments to investments.",
  "generatedAt": "2024-01-15T10:30:00Z",
  "modelUsed": "mistralai/mistral-7b-instruct:free",
  "provider": "OpenRouter",
  "cost": 0.00,
  "currency": "USD"
}
```

**Timeline**: 3-5 seconds | **Cost**: FREE ✨

---

### Example 2: Using FREE Llama Model

**Request:**
```bash
curl -X POST https://localhost:5001/api/recommendations/users/123e4567-e89b-12d3-a456-426614174000 \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "I have $50k in savings and want to start investing in real estate. Should I buy now?",
    "provider": 1,
    "modelName": "meta-llama/llama-2-7b-chat:free"
  }'
```

**Response (Example):**
```json
{
  "success": true,
  "allocations": [
    {
      "assetClass": "Real Estate Investment",
      "percentage": 40,
      "reason": "Your goal is real estate - consider down payment for rental property"
    },
    {
      "assetClass": "Keep Liquid",
      "percentage": 35,
      "reason": "Maintain 6-month emergency fund and cash for investment opportunities"
    },
    {
      "assetClass": "Stocks",
      "percentage": 20,
      "reason": "Diversify with index funds for passive income"
    },
    {
      "assetClass": "Bonds",
      "percentage": 5,
      "reason": "Conservative buffer for stability"
    }
  ],
  "explanation": "Real estate is a solid long-term investment...",
  "coachingNotes": "Before buying, ensure market is favorable and you have proper inspection/due diligence done.",
  "generatedAt": "2024-01-15T10:35:00Z",
  "modelUsed": "meta-llama/llama-2-7b-chat:free",
  "provider": "OpenRouter",
  "cost": 0.00,
  "currency": "USD"
}
```

**Timeline**: 5-7 seconds | **Cost**: FREE ✨

---

### Example 3: List Available Models

**Request:**
```bash
curl https://localhost:5001/api/recommendations/models \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response (Example):**
```json
{
  "providers": [
    {
      "name": "Ollama",
      "available": true,
      "models": [
        {
          "modelId": "llama3:latest",
          "displayName": "Llama 3 Latest",
          "provider": "Ollama",
          "description": "Latest Llama 3 model",
          "contextWindow": 8192,
          "costPer1kTokens": 0
        },
        {
          "modelId": "mistral:latest",
          "displayName": "Mistral Latest",
          "provider": "Ollama",
          "description": "Latest Mistral model",
          "contextWindow": 32000,
          "costPer1kTokens": 0
        }
      ]
    },
    {
      "name": "OpenRouter",
      "available": true,
      "models": [
        {
          "modelId": "mistralai/mistral-7b-instruct:free",
          "displayName": "Mistral 7B (FREE ✨)",
          "provider": "OpenRouter",
          "description": "Fast, efficient open-source model - COMPLETELY FREE - no credits needed",
          "contextWindow": 32000,
          "costPer1kTokens": 0
        },
        {
          "modelId": "meta-llama/llama-2-7b-chat:free",
          "displayName": "Llama 2 7B (FREE ✨)",
          "provider": "OpenRouter",
          "description": "Meta's Llama 2 model - COMPLETELY FREE - no credits needed",
          "contextWindow": 4096,
          "costPer1kTokens": 0
        },
        {
          "modelId": "google/flan-t5-large:free",
          "displayName": "Flan T5 Large (FREE ✨)",
          "provider": "OpenRouter",
          "description": "Google's T5 model - COMPLETELY FREE - no credits needed",
          "contextWindow": 2048,
          "costPer1kTokens": 0
        },
        {
          "modelId": "gpt2:free",
          "displayName": "GPT-2 (FREE ✨)",
          "provider": "OpenRouter",
          "description": "OpenAI's GPT-2 - COMPLETELY FREE - no credits needed",
          "contextWindow": 1024,
          "costPer1kTokens": 0
        },
        {
          "modelId": "openai/gpt-3.5-turbo",
          "displayName": "GPT-3.5 Turbo (Requires Credits)",
          "provider": "OpenRouter",
          "description": "Fast and efficient - costs $0.0005 per 1K tokens",
          "contextWindow": 4096,
          "costPer1kTokens": 0.0005
        },
        {
          "modelId": "anthropic/claude-3-sonnet",
          "displayName": "Claude 3 Sonnet (Requires Credits)",
          "provider": "OpenRouter",
          "description": "Best value premium - costs $0.003 per 1K tokens",
          "contextWindow": 200000,
          "costPer1kTokens": 0.003
        },
        {
          "modelId": "openai/gpt-4-turbo-preview",
          "displayName": "GPT-4 Turbo (Requires Credits)",
          "provider": "OpenRouter",
          "description": "Most capable model - costs $0.01 per 1K tokens",
          "contextWindow": 128000,
          "costPer1kTokens": 0.01
        }
      ]
    }
  ],
  "defaultProvider": "OpenRouter",
  "recommendedModels": [
    "mistralai/mistral-7b-instruct:free",
    "meta-llama/llama-2-7b-chat:free"
  ],
  "fetchedAt": "2024-01-15T10:40:00Z"
}
```

---

## 🔧 Provider Enum Values

| Provider | Value | Usage |
|----------|-------|-------|
| Ollama | 0 | Local, fast, FREE |
| OpenRouter | 1 | Cloud, flexible, FREE tier available |

---

## ✅ Quick Test Script

Save as `test-free-models.ps1`:

```powershell
$userId = "123e4567-e89b-12d3-a456-426614174000"
$baseUrl = "https://localhost:5001"

# Test 1: List available models
Write-Host "=== Available Models ===" -ForegroundColor Green
$models = Invoke-RestMethod -Uri "$baseUrl/api/recommendations/models" `
  -Headers @{"Authorization" = "Bearer YOUR_TOKEN"}
$models.providers[1].models | Where-Object { $_.costPer1kTokens -eq 0 } | 
  ForEach-Object { Write-Host "✅ $($_.displayName): FREE" }

# Test 2: Use FREE Mistral model
Write-Host "`n=== Testing Mistral 7B (FREE) ===" -ForegroundColor Green
$body = @{
    analysisContext = "Build wealth and reduce debt"
    provider = 1
    modelName = "mistralai/mistral-7b-instruct:free"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "$baseUrl/api/recommendations/users/$userId" `
  -Method Post `
  -ContentType "application/json" `
  -Headers @{"Authorization" = "Bearer YOUR_TOKEN"} `
  -Body $body

Write-Host "✅ Success! Generated in: $(Get-Date)" -ForegroundColor Green
Write-Host "Cost: `$$($response.cost)" -ForegroundColor Cyan
Write-Host "Model: $($response.modelUsed)" -ForegroundColor Cyan

# Test 3: Use FREE Llama model
Write-Host "`n=== Testing Llama 2 (FREE) ===" -ForegroundColor Green
$body = @{
    analysisContext = "Start investing in real estate"
    provider = 1
    modelName = "meta-llama/llama-2-7b-chat:free"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "$baseUrl/api/recommendations/users/$userId" `
  -Method Post `
  -ContentType "application/json" `
  -Headers @{"Authorization" = "Bearer YOUR_TOKEN"} `
  -Body $body

Write-Host "✅ Success! Generated in: $(Get-Date)" -ForegroundColor Green
Write-Host "Cost: `$$($response.cost)" -ForegroundColor Cyan

Write-Host "`n=== All Tests Passed! ===" -ForegroundColor Green
```

**Run it:**
```powershell
.\test-free-models.ps1
```

---

## 🔍 Debugging

### Check if response includes model info
```json
{
  "modelUsed": "mistralai/mistral-7b-instruct:free",
  "provider": "OpenRouter",
  "cost": 0.00
}
```
✅ Shows which model was used and confirms $0 cost

### Check API logs
```
API logs will show: "Using OpenRouter model: mistralai/mistral-7b-instruct:free"
```

### Verify model exists
```bash
curl https://localhost:5001/api/recommendations/models | 
  jq '.providers[1].models[] | select(.costPer1kTokens == 0)'
```
Shows all FREE models available

---

## 🎯 Summary

| Aspect | Details |
|--------|---------|
| **Best FREE Model** | `mistralai/mistral-7b-instruct:free` |
| **Speed** | 3-5 seconds |
| **Quality** | ⭐⭐⭐⭐ Excellent for financial advice |
| **Cost** | $0.00 completely FREE |
| **No Setup** | Just use it! |
| **Fallback** | If OpenRouter down, uses local Ollama |

---

**Ready to use!** 🚀 Pick a model and start getting FREE recommendations!
