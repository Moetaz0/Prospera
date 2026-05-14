# Setting Up Model Credentials

This guide explains how to configure your API credentials for Groq, HuggingFace, and other AI model providers.

## Quick Start

### 1. Get API Keys

#### **Groq (Recommended)** 🚀
- **Website:** https://console.groq.com/keys
- **Cost:** FREE (unlimited free tier)
- **Speed:** 30-40x faster than Ollama
- **Sign-up:** No credit card required

Steps:
1. Visit https://console.groq.com
2. Click "Sign in" or "Sign up"
3. Create account (Google/email)
4. Go to "API Keys" section
5. Click "Create New API Key"
6. Copy the key (starts with `gsk_`)

#### **HuggingFace (Alternative)** 🤗
- **Website:** https://huggingface.co/settings/tokens
- **Cost:** FREE (limited tier)
- **Advantage:** Access to 100K+ models
- **Sign-up:** No credit card required

Steps:
1. Visit https://huggingface.co
2. Sign up or log in
3. Go to Settings > Access Tokens
4. Click "New token"
5. Select "Read" access
6. Copy the token

---

## Configuration

### Development Environment (Recommended)

**File Location:**
```
C:\Users\GIGABYTE\AppData\Roaming\Microsoft\UserSecrets\prospera-api-secrets\secrets.json
```

**Update with your credentials:**

```json
{
  "FreeCloudAI:Provider": "groq",
  "FreeCloudAI:ApiKey": "gsk_YOUR_ACTUAL_KEY_HERE",
  "FreeCloudAI:Model": "mixtral-8x7b-32768"
}
```

**Example with real (obfuscated) key:**
```json
{
  "FreeCloudAI:Provider": "groq",
  "FreeCloudAI:ApiKey": "gsk_abc123def456ghi789jkl",
  "FreeCloudAI:Model": "mixtral-8x7b-32768"
}
```

### Production Environment

**Use Environment Variables:**

```powershell
# PowerShell
$env:FreeCloudAI__Provider = "groq"
$env:FreeCloudAI__ApiKey = "gsk_YOUR_KEY"
$env:FreeCloudAI__Model = "mixtral-8x7b-32768"
```

Or in your hosting platform (Azure, AWS, etc.):

**Azure App Service:**
```
Configuration → Application settings → New application setting
Name: FreeCloudAI__ApiKey
Value: gsk_YOUR_KEY
```

**Docker Environment:**
```dockerfile
ENV FreeCloudAI__ApiKey="gsk_YOUR_KEY"
ENV FreeCloudAI__Provider="groq"
ENV FreeCloudAI__Model="mixtral-8x7b-32768"
```

---

## Configuration Structure

### appsettings.json (Non-Sensitive)
```json
{
  "FreeCloudAI": {
    "Provider": "groq",
    "ApiKey": "",
    "Model": "mixtral-8x7b-32768"
  }
}
```

**Leave ApiKey empty** - it will be loaded from user secrets/environment variables.

### User Secrets (Sensitive - Local Development)
```json
{
  "FreeCloudAI:Provider": "groq",
  "FreeCloudAI:ApiKey": "gsk_YOUR_ACTUAL_KEY",
  "FreeCloudAI:Model": "mixtral-8x7b-32768"
}
```

---

## Supported Providers and Models

### Groq

**Provider Name:** `groq`

**Available Models:**
- `mixtral-8x7b-32768` ⭐ Recommended (fastest, unlimited free)
- `llama2-70b-4096` (powerful, slightly slower)
- `llama2-70b-chat-hf` (chat optimized)
- `gemma-7b-it` (compact, efficient)

**Example Configuration:**
```json
{
  "FreeCloudAI:Provider": "groq",
  "FreeCloudAI:ApiKey": "gsk_YOUR_KEY",
  "FreeCloudAI:Model": "mixtral-8x7b-32768"
}
```

### HuggingFace

**Provider Name:** `huggingface`

**Available Models:**
- `mistralai/Mistral-7B-Instruct-v0.2`
- `meta-llama/Llama-2-7b-chat-hf`
- `meta-llama/Llama-2-70b-chat-hf`
- `google/flan-t5-large`
- `google/flan-t5-xl`
- `microsoft/phi-2`
- `openchat/openchat-3.5`

**Example Configuration:**
```json
{
  "FreeCloudAI:Provider": "huggingface",
  "FreeCloudAI:ApiKey": "hf_YOUR_TOKEN",
  "FreeCloudAI:Model": "mistralai/Mistral-7B-Instruct-v0.2"
}
```

---

## Security Best Practices

✅ **DO:**
- Store API keys in user secrets (local development)
- Use environment variables in production
- Rotate keys regularly
- Use read-only/restricted tokens when possible
- Keep secrets.json in .gitignore (already done)

❌ **DON'T:**
- Commit API keys to version control
- Share keys in emails/chat/documents
- Use production keys in development
- Commit appsettings.json with real keys

---

## Testing Your Setup

### 1. Verify Configuration is Loaded

Run this test in Visual Studio:

```csharp
// In a controller or startup code
var configuration = serviceProvider.GetRequiredService<IConfiguration>();
var provider = configuration["FreeCloudAI:Provider"];
var apiKey = configuration["FreeCloudAI:ApiKey"];
var model = configuration["FreeCloudAI:Model"];

Console.WriteLine($"Provider: {provider}");
Console.WriteLine($"API Key present: {!string.IsNullOrEmpty(apiKey)}");
Console.WriteLine($"Model: {model}");
```

### 2. Test API Call

Make a recommendation request:

```bash
curl -X POST http://localhost:5000/api/Recommendations/users/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -d '{
    "analysisContext": "I want to invest $50,000 for retirement",
    "modelName": "mixtral-8x7b-32768"
  }'
```

Expected response (200 OK):
```json
{
  "id": "...",
  "userId": "...",
  "allocation": "45% Stocks, 30% Bonds, 15% Real Estate, 7% Crypto, 3% Cash",
  "explanation": "...",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### 3. Check Logs

Look for these log messages indicating successful API calls:
```
INFO: FreeCloudAI service initialized with provider: groq
INFO: Generating recommendation with model: mixtral-8x7b-32768
```

---

## Troubleshooting

### ❌ 401 Unauthorized
**Problem:** Invalid or expired API key
```
FreeCloudAI API returned 401 Unauthorized
```
**Solution:**
1. Verify API key is correct
2. Check key hasn't expired
3. Regenerate key if needed

### ❌ 429 Rate Limited
**Problem:** Too many requests
```
FreeCloudAI API returned 429 Too Many Requests
```
**Solution:**
1. Groq: Upgrade to paid plan or wait
2. HuggingFace: Wait for rate limit reset

### ❌ Configuration Not Loading
**Problem:** Keys show as empty
```
FreeCloudAI:ApiKey is empty
```
**Solution:**
1. Check secrets.json exists at correct path
2. Restart Visual Studio
3. Run `dotnet user-secrets list` to verify

---

## Key Locations

**Your Secrets File:**
```
C:\Users\GIGABYTE\AppData\Roaming\Microsoft\UserSecrets\prospera-api-secrets\secrets.json
```

**Application Config:**
```
D:\project\agent\Prospera\API\appsettings.json
```

**Development Config (if exists):**
```
D:\project\agent\Prospera\API\appsettings.Development.json
```

---

## Next Steps

1. ✅ Get API key from Groq (https://console.groq.com/keys)
2. ✅ Add to your secrets.json file
3. ✅ Restart Visual Studio
4. ✅ Run your application
5. ✅ Make a test request to verify it works

Need help? Check the application logs in the Output window for detailed error messages.
