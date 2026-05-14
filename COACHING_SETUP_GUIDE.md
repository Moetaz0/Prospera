# Prospera Financial Coaching System - Setup & Run Guide

## Overview
Your Prospera application has been enhanced with a comprehensive financial coaching system that acts like a real financial coach, providing personalized guidance for both assets and liabilities.

## What Was Fixed

### Issue: DependencyInjection Registration Error
**Problem**: The application was throwing an `AggregateException` during startup because `CoachingService` was trying to inject `IAiRecommendationService` which wasn't registered in the DI container.

**Solution**: 
- Made `IAiRecommendationService` optional in `CoachingService` constructor
- Added template-based fallback coaching when AI is unavailable
- Service gracefully degrades if AI service is not available

### Key Changes Made:

1. **CoachingService** (`Infrastructure\Services\CoachingService.cs`)
   - Implements `ICoachingService`
   - Optional AI integration (works with or without)
   - Template-based fallback for all coaching scenarios
   - Provides coaching for assets and liabilities

2. **Enhanced DTOs**
   - `LiabilityDto`: Now includes interest rate, monthly payment, projected amount, and coaching advice
   - `AssetValuationDto`: Already had `CoachingAdviceDto` property for comprehensive guidance

3. **New Query Handler**
   - `GetLiabilityCoachingQuery`: Generates personalized debt coaching

4. **Enhanced Handlers**
   - `GetAssetValuationQueryHandler`: Now generates coaching advice for each asset
   - `GenerateRecommendationCommandHandler`: Enhanced with coaching philosophy in AI prompts

## Features

### Coaching System Capabilities

1. **Asset Coaching**
   - Personalized guidance based on asset type, value, and performance
   - Motivation and encouragement
   - Specific action items and strategies
   - Success traits and pitfalls to avoid
   - Next steps

2. **Liability (Debt) Coaching**
   - Empathetic, supportive debt reduction strategies
   - Payoff plans and acceleration techniques
   - Behavioral tips for debt management
   - Milestone tracking and celebration

3. **Recommendation Coaching**
   - Holistic portfolio recommendations considering assets AND liabilities
   - Mentoring approach rather than algorithmic suggestions
   - Emphasis on sustainable financial habits

## Running the Application

### Prerequisites
- .NET 8 SDK
- Visual Studio 2026 (or later)
- MongoDB (for data persistence)
- Redis (for caching)
- Optional: Ollama (for enhanced AI coaching)

### Setup Steps

1. **Configure appsettings.json**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "mongodb://localhost:27017"
     },
     "Redis": {
       "Configuration": "localhost:6379"
     },
     "Ollama": {
       "Url": "http://localhost:11434",
       "Model": "llama3.2"
     }
   }
   ```

2. **Start Dependencies**
   ```powershell
   # MongoDB
   mongod

   # Redis
   redis-server

   # Ollama (optional, for enhanced coaching)
   ollama serve
   ```

3. **Build Solution**
   ```powershell
   dotnet build
   ```

4. **Run API**
   ```powershell
   dotnet run --project API\Prospera.API.csproj
   ```

   Or via Visual Studio:
   - F5 or Debug > Start Debugging

### API Endpoints

#### Asset Valuation with Coaching
```
GET /api/assets/{assetId}/valuation?userId={userId}
```
Returns asset valuation with coaching advice

#### Liability Coaching
```
GET /api/liabilities/{liabilityId}/coaching?userId={userId}&includeCoaching=true
```
Returns liability details with coaching advice

#### Investment Recommendations with Coaching
```
POST /api/recommendations/generate
Body: {
  "userId": "guid",
  "analysisContext": "Build wealth for retirement"
}
```
Returns personalized investment allocation with coaching context

## How Coaching Works

### Without AI (Default - Always Works)
- Uses template-based coaching
- Provides general best practices
- Motivation and actionable steps
- Works offline

### With AI (When Ollama Available)
- Personalized coaching based on financial profile
- Contextual guidance
- More sophisticated recommendations
- Better experience but optional

## Troubleshooting

### "Can't connect to MongoDB"
- Ensure MongoDB is running: `mongod`
- Check connection string in appsettings.json

### "Can't connect to Redis"
- Ensure Redis is running: `redis-server`
- Check Redis configuration in appsettings.json

### "Ollama not found"
- This is optional - coaching still works without it
- To enable: Download Ollama from ollama.ai and run `ollama serve`

### DI Resolution Issues
- Clean and rebuild: `dotnet clean && dotnet build`
- Clear bin/obj folders manually
- Restart Visual Studio

## Architecture Overview

```
API Layer (Controllers)
    ↓
MediatR Query Handlers
    ↓
Application Services
    - CoachingService (NEW)
    - AssetValuationService
    - ExternalRatesService
    ↓
Infrastructure
    - Repositories
    - ExternalServices (AI, MarketData)
    - Database Context
    ↓
Domain Models
```

## Key Files Modified/Created

- `Infrastructure\Services\CoachingService.cs` (NEW)
- `src\Application\Common\Interfaces\ICoachingService.cs` (NEW)
- `src\Application\Features\Liabilities\Queries\GetLiabilityCoachingQuery.cs` (NEW)
- `src\Application\Features\Assets\Queries\GetAssetValuationQueryHandler.cs` (MODIFIED)
- `src\Application\Features\Recommendations\Commands\GenerateRecommendationCommandHandler.cs` (MODIFIED)
- `Infrastructure\DependencyInjection.cs` (MODIFIED)
- `src\Application\DTOs\LiabilityDto.cs` (MODIFIED)
- `src\Application\DTOs\AssetValuationDto.cs` (MODIFIED)

## Testing the Coaching System

### Asset Coaching Test
```csharp
// Call the asset valuation endpoint
GET /api/assets/{assetId}/valuation?userId={userId}

// Response will include:
{
  "id": "...",
  "name": "Tesla Stock",
  "type": "Stock",
  "currentValue": 5000,
  "projectedValue": 5150,
  "coachingAdvice": {
    "motivation": "Great job building your Stock position!",
    "actions": ["Review holdings regularly", ...],
    "strategies": ["Diversify within Stock if concentrated", ...],
    "behavioralTips": ["Avoid emotional decision-making", ...],
    ...
  }
}
```

### Liability Coaching Test
```csharp
// Call the liability coaching endpoint
GET /api/liabilities/{liabilityId}/coaching?userId={userId}&includeCoaching=true

// Response will include debt coaching advice
```

## Next Steps

1. Deploy to a staging environment
2. Test with real financial data
3. Customize coaching prompts for your target audience
4. Monitor coaching generation performance
5. Gather user feedback on coaching advice quality
6. Consider integrating with more AI models

## Support

For issues or questions:
- Check build logs: `dotnet build -v diag`
- Review application logs in `logs/` folder
- Consult error details in Debug output window

