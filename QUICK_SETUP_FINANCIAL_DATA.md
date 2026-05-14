# Quick Setup: Financial Data Integration

## 🚀 Get Started in 3 Steps

### Step 1: Configure appsettings.json
Add this to your `appsettings.json`:

```json
{
  "FinancialDataApi": {
    "Url": "http://localhost:8000/api/v1"
  }
}
```

### Step 2: Start FastAPI Server
```bash
# In a new terminal, navigate to your FastAPI project
cd /path/to/financial/data/api
uvicorn main:app --reload
```

### Step 3: Run Prospera
```bash
# In Visual Studio, press F5 or run:
dotnet run --project src/API/Prospera.API.csproj
```

## ✅ Verify Integration

### Test 1: Create a Coaching Session
```bash
curl -X POST http://localhost:5000/api/coaching/users/550e8400-e29b-41d4-a716-446655440000/session \
  -H "Content-Type: application/json" \
  -d '{
    "currentSituation": "I save $500/month and want to invest",
    "goal": "Build wealth with inflation protection",
    "preferences": "Tunisia, interested in real estate",
    "provider": 0,
    "modelName": "mistralai/mistral-7b-instruct:free"
  }'
```

**Expected Result:** Coaching assessment includes:
- Current Tunisian inflation rate
- Real estate market data for Tunis
- Exchange rate context
- Inflation-aware action items

### Test 2: Check Progress
```bash
curl http://localhost:5000/api/coaching/users/550e8400-e29b-41d4-a716-446655440000/session/{sessionId}/progress
```

**Expected Result:** Recommendation includes market context:
```
"nextRecommendation": "🚀 Great start! You're building momentum. Keep the energy going! 
| Market Context: Inflation at 6.8% - factor this into your savings goals. 
Economic outlook: Moderate growth expected in next quarter"
```

## 🔧 Configuration Options

### Timeout Adjustment
Edit `DependencyInjection.cs` to change HTTP timeout:

```csharp
// Default is 10 seconds
services.AddHttpClient<IFinancialDataService, FinancialDataService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15); // Increase if FastAPI is slow
    });
```

### FastAPI URL for Production
```json
{
  "FinancialDataApi": {
    "Url": "https://your-production-fastapi.com/api/v1"
  }
}
```

## 📊 What's Enriched Now

### Coaching Session Creation
✅ Assessment includes inflation context
✅ Action items prioritized by economic conditions
✅ Milestones aligned with market opportunities

### Progress Tracking
✅ Real-time inflation insights
✅ GDP outlook affects recommendations
✅ Market-aware coaching messages

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| "HttpRequestException: No connection" | Make sure FastAPI runs on localhost:8000 |
| "timeout" in logs | FastAPI is slow; increase timeout in DependencyInjection.cs |
| Missing market data in coaching | Check appsettings.json has correct "Url" |
| No country/location detected | Add "Tunisia", "TN", or "Tunis" to preferences string |

## 📚 Full Documentation
See `FINANCIAL_DATA_INTEGRATION_GUIDE.md` for:
- Complete architecture diagram
- All 21 FastAPI endpoints
- Error handling details
- Performance optimization
- Future enhancement ideas

## 🎯 What This Enables

Your coaching system now:

1. **Understands economic context** - Real inflation data informs advice
2. **Provides data-driven recommendations** - Based on actual market conditions
3. **Personalizes by location** - Tunisia-specific insights for Tunisian users
4. **Predicts market trends** - LLM-powered forecasts in coaching
5. **Adapts to market changes** - Recommendations update as data changes

Example: When inflation is high, coaching prioritizes wealth-protection actions. When GDP outlook is strong, it encourages growth investments.
