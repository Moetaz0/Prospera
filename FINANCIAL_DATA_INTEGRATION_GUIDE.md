# Financial Data API Integration Guide

## Overview
The Prospera coaching system now integrates with your FastAPI financial data system to provide **data-driven, context-aware coaching**. Coaching assessments and recommendations are enriched with:

- **Current inflation rates** - Helps advisors recommend inflation-protected investments
- **Exchange rates** - Provides currency context for international investments or remittances
- **Real estate market data** - Supports property investment decisions with current market prices
- **Economic predictions** - GDP growth forecasts inform long-term financial planning
- **Car depreciation schedules** - Accurate asset valuation for vehicle financing decisions

## Architecture

### Integration Points

```
┌─────────────────────────────────────────────────────────┐
│  Prospera Coaching Endpoint                             │
│  POST /api/coaching/users/{userId}/session              │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
        ┌──────────────────────┐
        │  StartCoachingSession │
        │  CommandHandler      │
        └──────────┬───────────┘
                   │
        ┌──────────┴────────────┐
        │                       │
        ▼                       ▼
   ┌─────────────┐      ┌────────────────────┐
   │ User Assets │      │ FinancialDataService │
   │ Liabilities │      │ (FastAPI Client)     │
   └─────────────┘      └────────────────────┘
                               │
        ┌──────────────────────┴──────────────────────────┐
        │                                                  │
        ▼                                                  ▼
   ┌───────────────────┐  ┌──────────────────────────┐
   │ Your FastAPI      │  │ Real-Time Data Sources   │
   │ (localhost:8000)  │  │ - World Bank (Inflation) │
   │                   │  │ - Exchange Rates         │
   │ /api/v1/...       │  │ - Real Estate Markets    │
   └───────────────────┘  │ - LLM Predictions        │
                          └──────────────────────────┘
```

## Configuration

### Step 1: appsettings.json
Add the FastAPI base URL to your `appsettings.json`:

```json
{
  "FinancialDataApi": {
    "Url": "http://localhost:8000/api/v1"
  }
}
```

### Step 2: Running the FastAPI Server
Start your financial data API in a separate terminal:

```bash
cd /path/to/your/fastapi/project
uvicorn main:app --reload
# API will be available at http://localhost:8000
# Swagger docs at http://localhost:8000/docs
```

### Step 3: Starting Prospera
Run Prospera normally:

```bash
dotnet run --project src/API/Prospera.API.csproj
```

## How It Works

### Coaching Session Creation Flow

1. **User creates a coaching session** with their situation, goals, and preferences
2. **Handler extracts country/location info** from preferences (e.g., "Tunisia", "TN", "investing in Tunis")
3. **Handler calls FastAPI endpoints** in parallel to gather:
   - Current inflation rate (for purchasing power context)
   - Exchange rates (for currency/international context)
   - GDP predictions (for economic outlook)
   - Real estate market data (if location mentioned)
4. **LLM prompt is enriched** with this market data
5. **Coaching assessment is generated** with data-aware recommendations
6. **Session is saved** with assessment, action items, and milestones

### Example Coaching Enhancement

**Without Market Data:**
```
Focus on completing your first 25% of action items. You're building momentum!
```

**With Market Data (High Inflation):**
```
Focus on completing your first 25% of action items. You're building momentum!

Market Context: High inflation (7.2%) - prioritize actions that protect/grow your money.
```

### Real-Time Progress Recommendations

When users check progress, they get:
- Real-time inflation updates affecting savings goals
- GDP outlook influencing investment timing
- Dynamic motivational messages based on market conditions

## Available Endpoints

The integration can leverage all 21 of your FastAPI endpoints:

### Inflation Data
- `GET /api/v1/inflation/{code}` - Single country CPI history
- `GET /api/v1/inflation?countries=TN,FR,US` - Multi-country comparison

### Exchange Rates
- `GET /api/v1/exchange/current/{base}` - Live rates for any base currency
- `GET /api/v1/exchange/tnd` - TND vs major currencies
- `GET /api/v1/exchange/history/{code}` - Historical exchange trends
- `GET /api/v1/exchange/purchasing-power/{code}` - CPI-based PPP index

### Real Estate
- `GET /api/v1/real-estate/locations` - Available market locations
- `GET /api/v1/real-estate/tunisia/{location}` - Tunisian city prices
- `GET /api/v1/real-estate/global/{code}` - World Bank credit proxy

### Asset Valuation
- `POST /api/v1/cars/amortization` - Depreciation schedules
- `GET /api/v1/cars/categories` - Car categories and multipliers

### Predictions
- `GET /api/v1/predictions/inflation/{code}` - LLM inflation forecasts
- `GET /api/v1/predictions/exchange-rate/{code}` - FX predictions
- `GET /api/v1/predictions/real-estate/tunisia/{location}` - RE forecasts
- `GET /api/v1/predictions/gdp-growth/{code}` - GDP growth predictions

## Code Structure

### Key Files

1. **IFinancialDataService.cs** (`Application/Common/Interfaces/`)
   - Interface with all 21 endpoint methods
   - DTOs for responses (InflationData, ExchangeRateData, RealEstateData, etc.)

2. **FinancialDataService.cs** (`Infrastructure/ExternalServices/FinancialData/`)
   - HttpClient implementation
   - Parallel requests for efficiency
   - Error handling and fallback behavior
   - Logging for diagnostics

3. **StartCoachingSessionCommandHandler.cs** (Updated)
   - `GatherFinancialContextAsync()` - Parallel API calls
   - `ExtractCountryCodeFromPreferences()` - Smart country detection
   - `BuildCoachingPromptWithFinancialData()` - Enriched prompts

4. **CoachingProgressHandlers.cs** (Updated)
   - Context-aware recommendations
   - Inflation insights in progress messages
   - Market-driven action prioritization

## Error Handling

The system gracefully handles FastAPI unavailability:

```csharp
// If FastAPI is down, coaching still works with default context
try {
    var inflation = await _financialDataService.GetInflationDataAsync("TN", cancellationToken);
} catch (Exception ex) {
    _logger.LogWarning("Financial data unavailable: {Message}", ex.Message);
    // Coaching continues without market data enrichment
}
```

## Testing the Integration

### Manual Test: Create a Coaching Session

```bash
curl -X POST http://localhost:5000/api/coaching/users/550e8400-e29b-41d4-a716-446655440000/session \
  -H "Content-Type: application/json" \
  -d '{
    "currentSituation": "I have $50,000 in savings and own a property in Tunis",
    "goal": "Diversify investments and build long-term wealth",
    "preferences": "Tunisia, TND, interested in real estate",
    "provider": 0,
    "modelName": "mistralai/mistral-7b-instruct:free"
  }'
```

**Response will include:**
- Personalized assessment using current Tunisian inflation data
- Real estate insights for Tunis market
- Exchange rate context for diversification
- Specific action items based on economic outlook

### Manual Test: Get Progress with Market Context

```bash
curl http://localhost:5000/api/coaching/users/550e8400-e29b-41d4-a716-446655440000/session/{sessionId}/progress
```

**Response will include:**
- Current progress percentage
- Action items status
- **Market-aware recommendations** with inflation/GDP context
- Next milestone

## Troubleshooting

### FastAPI Connection Issues

**Symptom:** Coaching works but missing market data

```
Check:
1. FastAPI is running: http://localhost:8000/docs should load
2. Configuration URL is correct in appsettings.json
3. Firewall allows localhost:8000 access
4. Check logs for timeout errors (default 10 second timeout)
```

### Country Code Detection Failure

**Symptom:** Real estate or exchange data not enriching properly

```
The handler looks for:
- Country codes: TN, FR, US, GB, DE, IT, ES, NL, BE, CH, AT, PL, CZ, SE, NO, DK, FI, PT, GR, TR
- Country names: TUNISIA, FRANCE, UNITED STATES, etc.
- City names: TUNIS, SFAX, SOUSSE, MONASTIR, BIZERTE, KAIROUAN, GABÈS, TOZEUR, GAFSA, HAMMAMET

Make sure preferences include one of these identifiers.
```

## Performance Considerations

### Parallel Requests
All financial data calls run in parallel using `Task.WhenAll()`:

```csharp
var inflationTask = _financialDataService.GetInflationDataAsync("TN", cancellationToken);
var exchangeTask = _financialDataService.GetTndExchangeRatesAsync(cancellationToken);
var gdpTask = _financialDataService.GetGdpPredictionAsync("TN", cancellationToken);

await Task.WhenAll(inflationTask, exchangeTask, gdpTask);
```

**Expected response time:** 500-2000ms depending on FastAPI performance

### Caching Opportunity
Consider caching daily:
- Inflation rates (change infrequently)
- Exchange rates (update hourly)
- GDP predictions (update quarterly)

Add Redis caching to `FinancialDataService.cs` for production.

## Future Enhancements

1. **Multi-currency coaching** - Generate recommendations for users in different countries
2. **Risk analysis** - Include volatility/risk data in action item prioritization
3. **Automated alerts** - Notify users when inflation/exchange rates trigger thresholds
4. **Portfolio simulation** - Use real market data to simulate coaching scenarios
5. **Comparative analysis** - Benchmark user progress against market performance

## Support

For issues:
1. Check FastAPI logs: `http://localhost:8000/docs`
2. Check Prospera logs in Output window (Build pane)
3. Verify IFinancialDataService is registered in DependencyInjection
4. Ensure FinancialDataApi:Url is configured in appsettings.json
