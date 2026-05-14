# 🚀 Quick Start - Running Prospera with Coaching System

## What's New ✨
Your Prospera app now has a **financial coaching system** that acts like a real coach - providing personalized guidance for both assets AND liabilities!

## Prerequisites (5 min setup)

### Install if needed:
```powershell
# MongoDB - for data storage
# Download from: https://www.mongodb.com/try/download/community

# Redis - for caching  
# Download from: https://github.com/microsoftarchive/redis/releases (Windows)
# Or: choco install redis (if you have Chocolatey)

# Ollama (OPTIONAL - for enhanced AI coaching)
# Download from: https://ollama.ai
```

## Start Everything (in separate terminals)

### Terminal 1: MongoDB
```powershell
mongod
```

### Terminal 2: Redis
```powershell
redis-server
```

### Terminal 3: Ollama (optional - for AI coaching)
```powershell
ollama serve
```

### Terminal 4: Your Prospera API
```powershell
cd D:\project\agent\Prospera
dotnet run --project API\Prospera.API.csproj
```

Or just hit **F5** in Visual Studio!

## ✅ Verify It's Running

Once started, you should see:
```
info: Prospera.API[0]
      Prospera API is running

Now listening on: https://localhost:5001
```

Then visit: **https://localhost:5001/swagger**

## 🎯 Test the Coaching System

### Get Asset Coaching (requires asset in system)
```
GET /api/assets/{assetId}/valuation?userId={userId}
```

Expected response includes:
```json
{
  "coachingAdvice": {
    "motivation": "Great job building your Stock position!",
    "actions": ["Review holdings regularly", ...],
    "strategies": ["Diversify within Stock", ...],
    "behavioralTips": ["Avoid emotional decisions", ...],
    "successTraits": ["Patience", "Discipline", ...],
    "pitfallsToAvoid": ["Chasing performance", ...],
    "goalsReminder": "This helps you build wealth systematically",
    "nextSteps": ["Schedule portfolio review", ...]
  }
}
```

### Get Debt Coaching
```
GET /api/liabilities/{liabilityId}/coaching?userId={userId}&includeCoaching=true
```

### Get Recommendations with Coaching
```
POST /api/recommendations/generate
{
  "userId": "your-user-id",
  "analysisContext": "Build wealth for retirement",
  "modelName": "llama3.2"
}
```

## 🛠 Troubleshooting

| Issue | Solution |
|-------|----------|
| `Address already in use` | Another app on port 5001, or restart VS |
| `MongoDB connection refused` | Start mongod first |
| `Redis connection refused` | Start redis-server first |
| `Build failed` | Run: `dotnet clean && dotnet build` |
| `Hot reload not working` | Rebuild solution: Ctrl+Shift+B |

## 📚 What Changed

### New Files Created:
- ✅ `Infrastructure\Services\CoachingService.cs` - The coaching engine
- ✅ `src\Application\Common\Interfaces\ICoachingService.cs` - Coaching interface
- ✅ `src\Application\Features\Liabilities\Queries\GetLiabilityCoachingQuery.cs` - Debt coaching

### Modified Files:
- 📝 `GetAssetValuationQueryHandler.cs` - Now generates coaching advice
- 📝 `GenerateRecommendationCommandHandler.cs` - Enhanced with coaching prompts
- 📝 `LiabilityDto.cs` - Added coaching and financial fields
- 📝 `DependencyInjection.cs` - Registered coaching service

### Key Features:
1. **Asset Coaching** - Personalized guidance per asset
2. **Liability Coaching** - Debt reduction strategies  
3. **Recommendation Coaching** - Holistic portfolio advice
4. **Fallback Mode** - Works even without AI (template-based)

## 🤖 AI Coaching (Optional but Cool!)

With Ollama running, coaching becomes AI-powered:
- Personalized based on financial profile
- Contextual, not generic advice
- Better recommendations

Without Ollama:
- Template-based coaching (still good!)
- General best practices
- Motivation and action items
- No AI required - always works

## 🔗 API Documentation

Once running, full API docs at: **https://localhost:5001/swagger**

All endpoints available including:
- Asset management with valuations
- Liability tracking with coaching
- Investment recommendations
- Financial metrics

## 📖 More Info

For detailed setup & architecture, see: **COACHING_SETUP_GUIDE.md**

## 🎉 You're All Set!

Your coaching system is ready to:
- 💪 Motivate users on their financial journey
- 📊 Provide personalized guidance for each asset
- 🎯 Help users eliminate debt strategically  
- 📈 Recommend diversified portfolios
- 💡 Offer behavioral coaching tips

Happy coding! 🚀

