# Financial API Integration Setup

This document describes the integration between the Prospera backend and the Python Financial-API for car valuations and inflation predictions.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Prospera Backend (.NET)                   │
│  - Asset Management (Cash, Stocks, Bonds, RE, Crypto, Cars) │
│  - Inflation Predictions                                    │
│  - Valuation Calculations                                   │
└────────────────┬────────────────────────────────────────────┘
                 │ HTTP/JSON
                 │ Port 8000
                 ▼
┌─────────────────────────────────────────────────────────────┐
│              Python Financial-API (FastAPI)                  │
│  - Car Depreciation (amortization schedules)                │
│  - Inflation Predictions (trend analysis)                   │
│  - Exchange Rates & Market Data                             │
│  - Real Estate Growth Projections                           │
└─────────────────────────────────────────────────────────────┘
```

## Prerequisites

### .NET Backend Requirements
- .NET 8.0 or later
- MongoDB (configured in appsettings.json)
- Redis (optional, for caching)

### Python API Requirements
- Python 3.10+
- pip package manager
- FastAPI
- Uvicorn

## Configuration

### 1. Backend Configuration (appsettings.json)

The following configuration has been added:

```json
{
  "FinancialDataApi": {
    "Url": "http://localhost:8000",
    "Timeout": 30,
    "Enabled": true
  }
}
```

**Configuration Options:**
- **Url**: Base URL of the Python financial-api (default: `http://localhost:8000`)
- **Timeout**: Request timeout in seconds (default: 30)
- **Enabled**: Toggle API integration on/off (default: true)

**Files Updated:**
- `API/appsettings.json`
- `API/appsettings.Development.json`
- `API/appsettings.example.json`

### 2. Python API Configuration

The Python API uses environment variables in `.env`:

```env
# Cache settings
CACHE_DB_PATH=./cache.db
CACHE_TTL_STOCK=3600
CACHE_TTL_WORLDBANK=86400
CACHE_TTL_IMF=86400
CACHE_TTL_PREDICTION=43200
HTTP_TIMEOUT=30

# Data storage
DATA_DIR=./data

# OpenRouter LLM (for predictions)
OPENROUTER_API_KEY=your-key-here
OPENROUTER_MODEL=meta-llama/llama-3.3-70b-instruct:free
```

## Running the Services

### Starting the Python Financial-API

```bash
# Navigate to the financial-api directory
cd C:\Users\GIGABYTE\financial-api

# Install dependencies (if not already installed)
pip install -r requirements.txt

# Start the API with auto-reload
uvicorn main:app --reload

# Or without auto-reload for production
uvicorn main:app --host 0.0.0.0 --port 8000
```

The API will be available at:
- **Base URL**: `http://localhost:8000`
- **Swagger Docs**: `http://localhost:8000/docs`
- **ReDoc**: `http://localhost:8000/redoc`
- **Health Check**: `http://localhost:8000/health`

### Starting the .NET Backend

```bash
# Navigate to the Prospera API directory
cd d:\project\agent\Prospera\API

# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Or with hot reload
dotnet watch run
```

The API will be available at:
- **Base URL**: `http://localhost:5147`
- **Swagger Docs**: `http://localhost:5147/swagger`

## Available Endpoints

### Car Valuation
When you create a Car asset or retrieve its valuation:

**Frontend:**
```
PUT /api/Assets/users/{userId}/assets/{id}/valuation
Query: inflationRate (optional), carDepreciationRate (optional)
```

**Backend → Python API:**
```
POST /api/v1/cars/amortization
{
  "purchase_price": 25000,
  "purchase_year": 2020,
  "target_year": 2026,
  "car_category": "sedan",
  "country": "US",
  "annual_mileage_km": 15000
}
```

**Response includes:**
- Current market value
- Depreciation percentage
- Inflation-adjusted value
- Yearly depreciation breakdown

### Inflation Predictions
```
GET /api/FinancialMetrics/inflation/{countryCode}/predictions
Query: yearsAhead (1-20, default: 5)
```

**Backend → Python API:**
```
GET /api/v1/inflation/predict/US?years=5
```

**Response includes:**
- Current inflation rate
- Yearly predictions with confidence scores
- Prediction method details
- Historical context notes

### Current Inflation Rate
```
GET /api/FinancialMetrics/inflation/{countryCode}
```

Fetches the latest inflation rate from World Bank API (with caching).

## Integration Points

### 1. Car Asset Valuation (`GetAssetValuationQueryHandler.cs`)

When a car asset is retrieved:
1. Handler detects `AssetType.Car`
2. Calls `ICarValuationApiService.GetCarValuationAsync()`
3. Python API returns depreciation schedule
4. Backend applies the rates to calculate projections
5. Falls back to regional defaults if API is unavailable

```csharp
if (asset.Type == Prospera.Domain.Enums.AssetType.Car && user != null)
{
    var carValuation = await _carValuationApiService.GetCarValuationAsync(
        purchasePrice: asset.CurrentValue,
        purchaseYear: asset.CreatedAt.Year,
        targetYear: DateTime.UtcNow.Year + 1,
        category: "sedan",
        country: user.Country ?? "US");

    if (carValuation != null)
    {
        carDepreciationRate = (decimal)carValuation.TotalDepreciationPercent / 100m;
    }
}
```

### 2. Inflation Predictions (`GetInflationPredictionQueryHandler.cs`)

When predictions are requested:
1. Handler receives country code
2. Fetches historical data from World Bank/IMF
3. Calls `IInflationPredictionApiService.GetInflationPredictionsAsync()`
4. Python API performs trend analysis + mean reversion
5. Returns yearly predictions with confidence scores

```csharp
var predictions = await _inflationApiService.GetInflationPredictionsAsync(
    countryCode: "US",
    yearsAhead: 5);
```

## Dependency Injection

Services are registered in `Infrastructure/DependencyInjection.cs`:

```csharp
// Python Financial-API Services
services.AddHttpClient<ICarValuationApiService, CarValuationApiService>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("http://localhost:8000");
        client.Timeout = TimeSpan.FromSeconds(30);
    });

services.AddHttpClient<IInflationPredictionApiService, InflationPredictionApiService>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("http://localhost:8000");
        client.Timeout = TimeSpan.FromSeconds(30);
    });
```

## Error Handling

Both services implement graceful degradation:

### Car Valuations
- If Python API is unavailable, uses regional depreciation defaults
- Logs warning but continues with fallback rates
- Returns valuation with "Regional Default" source

### Inflation Predictions
- If Python API is unavailable, generates simple linear projections
- Uses historical average with mean reversion
- Fallback predictions clamped between 0.5% and 15%

## Monitoring & Debugging

### Health Checks
```bash
# Check Python API health
curl http://localhost:8000/health

# Check Python API status
curl http://localhost:8000/
```

### Logs
- **Backend**: Check `Logs/prospera-{date}.txt`
- **Python API**: Console output from uvicorn

### Swagger Documentation
- **Backend**: `http://localhost:5147/swagger`
- **Python API**: `http://localhost:8000/docs`

## Performance Considerations

### Caching
- **Inflation Rates**: Cached for 90 days (World Bank data updates quarterly)
- **Car Categories**: Hardcoded, no caching needed
- **Predictions**: Cached for 12 hours (TTL: 43200 seconds)

### Timeouts
- All HTTP client calls timeout after 30 seconds
- Safe for slow network conditions
- Falls back gracefully on timeout

### Rate Limiting
- World Bank API: No rate limit (public data)
- IMF API: No rate limit (public data)
- OpenRouter (LLM): Rate limits depend on tier (monitor via headers)

## Troubleshooting

### Python API Not Responding
**Error**: `Failed to connect to financial-api`

**Solutions:**
1. Verify Python API is running: `curl http://localhost:8000/health`
2. Check firewall settings
3. Verify correct port in appsettings.json
4. Check Python API logs for startup errors

### Car Valuation Returns Null
**Cause**: Python API unavailable or invalid parameters

**Solution**: Check backend logs for `CarValuationApiService` errors. System falls back to regional defaults.

### Inflation Predictions Timeout
**Cause**: World Bank API is slow or unreachable

**Solution**: Predictions use cached data. Wait 90 days for new data, or increase timeout in appsettings.json.

## Testing

### Test Car Valuation Endpoint
```bash
# Get car asset valuation
curl -X GET "http://localhost:5147/api/Assets/users/{userId}/assets/{assetId}/valuation" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Test Inflation Predictions
```bash
# Get 5-year inflation predictions for USA
curl -X GET "http://localhost:5147/api/FinancialMetrics/inflation/US/predictions?yearsAhead=5" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Get 10-year predictions for Tunisia
curl -X GET "http://localhost:5147/api/FinancialMetrics/inflation/TN/predictions?yearsAhead=10" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Test Python API Directly
```bash
# Car amortization
curl -X POST "http://localhost:8000/api/v1/cars/amortization" \
  -H "Content-Type: application/json" \
  -d '{
    "purchase_price": 25000,
    "purchase_year": 2020,
    "target_year": 2026,
    "car_category": "sedan",
    "country": "US"
  }'

# Inflation predictions
curl "http://localhost:8000/api/v1/inflation/predict/US?years=5"

# Historical inflation
curl "http://localhost:8000/api/v1/inflation/US"
```

## Supported Countries

Car depreciation rates are available for:
- **Africa**: TN (Tunisia), MA (Morocco), DZ (Algeria)
- **North America**: US (USA), CA (Canada), MX (Mexico)
- **South America**: BR (Brazil)
- **Europe**: DE, FR, GB, IT, ES, NL, BE, CH, SE, NO
- **Oceania**: AU, NZ
- **Asia**: JP, SG, IN
- **Middle East**: AE

Inflation data is available for 190+ countries via World Bank API.

## Future Enhancements

- [ ] Implement response caching in backend
- [ ] Add webhook support for real-time updates
- [ ] Support custom car categories
- [ ] Machine learning models for long-term predictions
- [ ] Multi-currency support
- [ ] Real-time market data streaming

---

**Last Updated**: May 2026
**Version**: 1.0.0
