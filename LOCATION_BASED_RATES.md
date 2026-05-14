# Location-Based Financial Rates

Your system now automatically applies location-specific financial rates based on user's country! No more manual rate parameter entry needed.

## Overview

The system automatically detects the user's country and applies appropriate rates for:
- **Inflation Impact** - Annual inflation rate (2.0% - 7.0%)
- **Car Depreciation** - Annual depreciation rate (13% - 16%)
- **Real Estate Appreciation** - Annual appreciation rate (1.5% - 5.0%)

---

## How It Works

### 1. User Sets Country
When a user registers or updates their profile, they specify their country (ISO 3166-1 alpha-2 code):
```json
{
  "country": "TN"  // or "US", "DE", "FR", "GB", etc.
}
```

### 2. System Fetches Country Rates Automatically
When requesting asset valuation, the system:
1. Fetches user's profile to get their country code
2. Looks up country-specific rates in `ILocationBasedRatesService`
3. Applies rates automatically to valuation calculation
4. Falls back to defaults if country not found or not set

### 3. Asset Valuation Uses Automatic Rates
No query parameters needed! Just call the endpoint normally:
```
GET /api/Assets/users/{userId}/assets/{assetId}/valuation
```

Instead of:
```
GET /api/Assets/users/{userId}/assets/{assetId}/valuation?inflationRate=7.0&carDepreciationRate=15&realEstateAppreciationRate=2
```

---

## Supported Countries & Rates

### Africa
| Country | Code | Inflation | Car Depreciation | RE Appreciation |
|---------|------|-----------|------------------|-----------------|
| **Tunisia** | TN | 7.0% | 15.0% | 2.0% |

### North America
| Country | Code | Inflation | Car Depreciation | RE Appreciation |
|---------|------|-----------|------------------|-----------------|
| **United States** | US | 3.5% | 15.0% | 3.5% |
| **Canada** | CA | 3.0% | 15.0% | 3.2% |
| **Mexico** | MX | 4.5% | 16.0% | 3.5% |

### South America
| Country | Code | Inflation | Car Depreciation | RE Appreciation |
|---------|------|-----------|------------------|-----------------|
| **Brazil** | BR | 5.5% | 16.0% | 4.0% |

### Europe
| Country | Code | Inflation | Car Depreciation | RE Appreciation |
|---------|------|-----------|------------------|-----------------|
| **Germany** | DE | 2.5% | 14.0% | 3.0% |
| **France** | FR | 2.4% | 14.0% | 2.8% |
| **United Kingdom** | GB | 3.0% | 14.0% | 3.2% |
| **Italy** | IT | 2.6% | 14.0% | 2.5% |
| **Spain** | ES | 2.5% | 14.0% | 2.7% |
| **Netherlands** | NL | 2.3% | 14.0% | 3.1% |
| **Belgium** | BE | 2.4% | 14.0% | 2.9% |
| **Switzerland** | CH | 1.8% | 13.0% | 2.8% |
| **Sweden** | SE | 2.7% | 14.0% | 3.3% |
| **Norway** | NO | 2.5% | 14.0% | 3.0% |

### Oceania
| Country | Code | Inflation | Car Depreciation | RE Appreciation |
|---------|------|-----------|------------------|-----------------|
| **Australia** | AU | 3.8% | 15.0% | 4.0% |
| **New Zealand** | NZ | 3.4% | 15.0% | 3.5% |

### Asia
| Country | Code | Inflation | Car Depreciation | RE Appreciation |
|---------|------|-----------|------------------|-----------------|
| **Japan** | JP | 2.1% | 13.0% | 1.5% |
| **Singapore** | SG | 2.2% | 15.0% | 3.2% |
| **India** | IN | 4.8% | 15.0% | 5.0% |

### Middle East
| Country | Code | Inflation | Car Depreciation | RE Appreciation |
|---------|------|-----------|------------------|-----------------|
| **United Arab Emirates** | AE | 2.0% | 15.0% | 4.0% |

---

## Examples

### Example 1: User from Tunisia (Auto-Detection)

**Setup:**
```csharp
// User profile
user.Country = "TN";  // Tunisia
```

**Request (no parameters needed):**
```bash
curl -X GET "http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000/assets/660e8400-e29b-41d4-a716-446655440001/valuation" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Automatic Rates Applied:**
- Inflation: 7.0%
- Car Depreciation: 15.0%
- Real Estate Appreciation: 2.0%

**Response (Car worth $20,000):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "name": "2023 Mercedes C-Class",
  "type": "Car",
  "currentValue": 20000,
  "projectedValueAfterInflation": 18598.13,
  "inflationLoss": 1401.87,
  "projectedValueCar": 17000,
  "carDepreciation": 3000,
  "carDepreciationRate": 15,
  "projectionSummary": "Your car worth $20,000.00 will depreciate to $17,000.00 in 1 year (15.0% annual depreciation). Additionally, due to 7.0% inflation, the purchasing power of $20,000.00 will be $18,598.13.",
  "calculatedAt": "2024-01-15T10:30:00Z"
}
```

### Example 2: User from Germany (Auto-Detection)

**Setup:**
```csharp
// User profile
user.Country = "DE";  // Germany
```

**Request (no parameters needed):**
```bash
curl -X GET "http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000/assets/770e8400-e29b-41d4-a716-446655440002/valuation" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Automatic Rates Applied:**
- Inflation: 2.5%
- Car Depreciation: 14.0%
- Real Estate Appreciation: 3.0%

**Response (Real Estate worth $500,000):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "name": "Berlin Apartment",
  "type": "RealEstate",
  "currentValue": 500000,
  "projectedValueAfterInflation": 487804.88,
  "inflationLoss": 12195.12,
  "projectedValueRealEstate": 515000,
  "realEstateAppreciation": 15000,
  "realEstateAppreciationRate": 3,
  "projectionSummary": "Your real estate worth $500,000.00 is projected to appreciate to $515,000.00 in 1 year (3.0% annual appreciation). This growth outpaces inflation (2.5%), providing real wealth building.",
  "calculatedAt": "2024-01-15T10:30:00Z"
}
```

### Example 3: Override Still Possible (Admin Feature)

If you need to override automatic rates for special cases, you can still pass query parameters:

```bash
curl -X GET "http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000/assets/880e8400-e29b-41d4-a716-446655440003/valuation?inflationRate=5.0" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Behavior:**
- `inflationRate=5.0` → Uses provided value (5.0%)
- Car Depreciation → Uses Tunisia default (15.0%)
- Real Estate Appreciation → Uses Tunisia default (2.0%)

---

## API Reference

### Getting All Available Countries

**Request:**
```bash
curl -X GET "http://localhost:5000/api/Locations/available-rates" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
[
  {
    "countryCode": "TN",
    "countryName": "Tunisia",
    "region": "Africa",
    "inflationRate": 7.0,
    "carDepreciationRate": 15.0,
    "realEstateAppreciationRate": 2.0
  },
  {
    "countryCode": "US",
    "countryName": "United States",
    "region": "North America",
    "inflationRate": 3.5,
    "carDepreciationRate": 15.0,
    "realEstateAppreciationRate": 3.5
  },
  // ... more countries
]
```

### Setting User's Country

**Request:**
```bash
curl -X PATCH "http://localhost:5000/api/Users/{userId}" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "country": "TN"
  }'
```

---

## Rate Update Schedule

Financial rates are calibrated based on:
- **Inflation**: World Bank / National statistics agencies (annual)
- **Car Depreciation**: Average market depreciation by region
- **Real Estate Appreciation**: Historical property market data

Rates are updated quarterly when significant economic changes occur. Current rates were last updated: **2024-01-15**

---

## Implementation Details

### Service: `ILocationBasedRatesService`

Located in: `Prospera.Application.Common.Interfaces`

**Methods:**
```csharp
// Get rates by ISO country code
LocationBasedRates GetRatesByCountry(string countryCode);

// Get rates by country name
LocationBasedRates GetRatesByCountryName(string countryName);

// Get all supported locations
IEnumerable<LocationRateMapping> GetAllAvailableLocations();

// Check if country is supported
bool IsCountrySupported(string countryCode);
```

### User Entity

Added property to `Prospera.Domain.Entities.User`:
```csharp
/// <summary>
/// ISO 3166-1 alpha-2 country code (e.g., "US", "DE", "TN")
/// Used for automatic location-based financial rates
/// </summary>
public string? Country { get; private set; }

public void SetCountry(string? countryCode)
{
    Country = countryCode;
}
```

### Query Handler

`GetAssetValuationQueryHandler` now:
1. Checks if user has a country set
2. Fetches rates from `ILocationBasedRatesService`
3. Uses location rates as defaults
4. Allows query parameters to override specific rates if needed
5. Falls back to hardcoded defaults if country not supported

---

## Best Practices

### For End Users
- ✅ **Set your country** during registration or profile setup for automatic rates
- ✅ **Trust automatic rates** unless you have specialized needs
- ✅ **Review projections** to understand how location affects your wealth

### For API Consumers
- ✅ **Always set user country** when creating users
- ✅ **Cache available locations** to provide UI dropdown (call `/api/Locations/available-rates` once at app load)
- ✅ **Display rate source** to users: "Rates calculated for Tunisia: 7.0% inflation"
- ✅ **Allow country change** in user settings

### For Administrators
- ✅ **Update rates quarterly** when economic conditions change
- ✅ **Add new countries** by extending `LocationBasedRatesService`
- ✅ **Monitor outliers** - if projection seems wrong, verify user's country is set correctly
- ✅ **Provide override option** for special cases (war, economic crisis, currency devaluation)

---

## Troubleshooting

### Problem: Asset valuations use default rates (3.5% inflation)

**Cause:** User's country field is not set or contains invalid country code

**Solution:**
1. Check user's profile: `SELECT id, country FROM users WHERE id = '{userId}'`
2. Set country if null: `user.SetCountry("TN")`
3. Verify country code is ISO 3166-1 alpha-2 format

### Problem: KeyNotFoundException for unsupported country

**Cause:** User's country is not in supported list

**Solution:**
1. Check supported countries: `GET /api/Locations/available-rates`
2. Either:
   - Change user's country to supported one
   - Add new country to `LocationBasedRatesService`
   - Request admin to add country support

### Problem: Query parameter overrides aren't working

**Cause:** All three rates must be overridden; partial overrides use location-based defaults

**Solution:**
- Provide all three query parameters to override: `?inflationRate=5&carDepreciationRate=16&realEstateAppreciationRate=4`
- If you only override one, others still come from location-based service

---

## Future Enhancements

- [ ] Dynamic rate updates from external APIs (World Bank, national banks)
- [ ] Regional subdivision support (California vs Texas, Berlin vs Bavaria)
- [ ] Custom rates per user (premium feature)
- [ ] Rate history and projections
- [ ] Notification when rates change significantly
- [ ] Multi-currency support with exchange rate impact

---

## Integration in Recommendations

The valuation system is used by:
- **Asset Valuation Endpoint** - Returns 1-year projections with location-based rates
- **Recommendation Engine** - Considers location-specific asset depreciation/appreciation in portfolio optimization
- **Financial Metrics** - Shows real vs nominal wealth growth accounting for regional inflation

---

## Code Example: Using Location-Based Rates

```csharp
// 1. When creating a new user, set their country
var user = new User("John Doe", "john@example.com");
user.SetCountry("TN");  // Tunisia
await _userRepository.AddAsync(user);

// 2. When requesting asset valuation, rates are automatic
var query = new GetAssetValuationQuery
{
    UserId = user.Id,
    AssetId = assetId
    // No need to specify inflationRate, carDepreciationRate, realEstateAppreciationRate
};
var valuation = await _mediator.Send(query);
// Automatically uses: Inflation 7%, Car Depreciation 15%, RE Appreciation 2%

// 3. Can still override if needed
var queryWithOverride = new GetAssetValuationQuery
{
    UserId = user.Id,
    AssetId = assetId,
    InflationRate = 8.0m  // Override just inflation
    // carDepreciationRate and realEstateAppreciationRate will use Tunisia defaults
};
var valuationWithOverride = await _mediator.Send(queryWithOverride);
```

---

## Geographic Regions Summary

- **20+ countries** supported across 5 continents
- **Inflation range:** 1.8% (Switzerland) to 7.0% (Tunisia)
- **Car depreciation range:** 13% (Switzerland/Japan) to 16% (Brazil/Mexico)
- **Real estate appreciation range:** 1.5% (Japan) to 5.0% (India)

Users can now get accurate, location-aware asset valuations without technical knowledge! 🌍💰
