# Asset Valuation & Projections

Your system now calculates advanced asset valuations including inflation impact, car amortization, and real estate appreciation. Get 1-year projections for all your assets!

## Overview

The system automatically calculates:

- **Inflation Impact** - How much purchasing power your money/assets lose over 1 year
- **Car Depreciation** - How much your car depreciates (default: 15% annually)
- **Real Estate Appreciation** - How much your property appreciates (default: 3% annually)
- **Projected Values** - Full valuation summary for 1 year ahead

---

## Supported Asset Types

### With Special Calculations

| Type | Calculation | Default Rate | Example |
|------|-------------|--------------|---------|
| **Car** | Depreciation/Amortization | 15% per year | Car worth $30,000 → $25,500 in 1 year |
| **RealEstate** | Appreciation | 3% per year | Property worth $500,000 → $515,000 in 1 year |

### With Inflation-Only Calculations

| Type | Effect |
|------|--------|
| **Cash** | Purchasing power decreases with inflation |
| **Stock** | Purchasing power affected by inflation |
| **Bond** | Purchasing power affected by inflation |
| **Crypto** | Purchasing power affected by inflation |
| **Other** | Purchasing power affected by inflation |

---

## API Endpoint

### Get Asset Valuation

```
GET /api/Assets/users/{userId}/assets/{assetId}/valuation
```

**Headers:**
```
Authorization: Bearer YOUR_TOKEN
```

**Query Parameters (all optional):**
```
?inflationRate=3.5
&carDepreciationRate=15
&realEstateAppreciationRate=3
```

---

## Examples

### Example 1: Car Asset Valuation

**Request:**
```bash
curl -X GET "http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000/assets/660e8400-e29b-41d4-a716-446655440001/valuation" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response (Car worth $30,000):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "name": "2023 Toyota Camry",
  "type": "Car",
  "currentValue": 30000,
  "projectedValueAfterInflation": 28950,
  "inflationLoss": 1050,
  "projectedValueCar": 25500,
  "carDepreciation": 4500,
  "carDepreciationRate": 15,
  "projectionSummary": "Your car worth $30,000.00 will depreciate to $25,500.00 in 1 year (15.0% annual depreciation). Additionally, due to 3.5% inflation, the purchasing power of $30,000.00 will be $28,950.00.",
  "calculatedAt": "2024-01-15T10:30:00Z"
}
```

**Interpretation:**
- Current car value: **$30,000**
- Value after 1 year (15% depreciation): **$25,500** (loses $4,500)
- Purchasing power after inflation: **$28,950** (loses $1,050 to inflation)
- **Total value loss in 1 year: $4,500 depreciation + $1,050 inflation = $5,550**

### Example 2: Real Estate Asset Valuation

**Request:**
```bash
curl -X GET "http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000/assets/770e8400-e29b-41d4-a716-446655440002/valuation" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response (Real Estate worth $500,000):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "name": "Primary Residence",
  "type": "RealEstate",
  "currentValue": 500000,
  "projectedValueAfterInflation": 482903.23,
  "inflationLoss": 17096.77,
  "projectedValueRealEstate": 515000,
  "realEstateAppreciation": 15000,
  "realEstateAppreciationRate": 3,
  "projectionSummary": "Your real estate worth $500,000.00 is projected to appreciate to $515,000.00 in 1 year (3.0% annual appreciation). This growth outpaces inflation (3.5%), providing real wealth building.",
  "calculatedAt": "2024-01-15T10:30:00Z"
}
```

**Interpretation:**
- Current property value: **$500,000**
- Value after 1 year (3% appreciation): **$515,000** (gains $15,000)
- Purchasing power after inflation: **$482,903** (loses $17,096 to inflation)
- **Net gain: $15,000 appreciation - $17,096 inflation = -$2,096 net loss**
- BUT: Real asset grows while cash loses value → wealth building!

### Example 3: Cash Asset Valuation

**Request:**
```bash
curl -X GET "http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000/assets/880e8400-e29b-41d4-a716-446655440003/valuation" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response (Cash $10,000):**
```json
{
  "id": "880e8400-e29b-41d4-a716-446655440003",
  "name": "Savings Account",
  "type": "Cash",
  "currentValue": 10000,
  "projectedValueAfterInflation": 9661.84,
  "inflationLoss": 338.16,
  "projectionSummary": "Your Cash asset worth $10,000.00 will have a purchasing power of $9,661.84 in 1 year due to 3.5% inflation (loss: $338.16).",
  "calculatedAt": "2024-01-15T10:30:00Z"
}
```

**Interpretation:**
- Cash today: **$10,000**
- Purchasing power in 1 year: **$9,661.84** (loses $338.16 to inflation)
- **Recommendation:** Invest cash or keep in inflation-protected accounts!

---

## Custom Rates

You can override default rates:

### Car with 20% depreciation (used/luxury car):
```bash
curl -X GET "http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000/assets/660e8400-e29b-41d4-a716-446655440001/valuation?carDepreciationRate=20" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Real Estate with 4.5% appreciation (hot market):
```bash
curl -X GET "http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000/assets/770e8400-e29b-41d4-a716-446655440002/valuation?realEstateAppreciationRate=4.5" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Custom inflation rate (2.5%):
```bash
curl -X GET "http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000/assets/880e8400-e29b-41d4-a716-446655440003/valuation?inflationRate=2.5" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Calculation Formulas

### Car Depreciation
```
Projected Value = Current Value × (1 - Rate/100)^Years
Example: $30,000 × (1 - 15/100)^1 = $30,000 × 0.85 = $25,500
```

### Real Estate Appreciation
```
Projected Value = Current Value × (1 + Rate/100)^Years
Example: $500,000 × (1 + 3/100)^1 = $500,000 × 1.03 = $515,000
```

### Inflation-Adjusted Value
```
Adjusted Value = Current Value / (1 + Rate/100)^Years
Example: $10,000 / (1 + 3.5/100)^1 = $10,000 / 1.035 = $9,661.84
```

---

## Default Rates

| Factor | Default | Typical Range |
|--------|---------|---------------|
| **Inflation** | 3.5% | 2% - 5% |
| **Car Depreciation** | 15% | 10% - 20% |
| **Real Estate Appreciation** | 3% | 2% - 5% |

---

## JavaScript/TypeScript Usage

```typescript
interface AssetValuationResponse {
  id: string;
  name: string;
  type: 'Car' | 'RealEstate' | 'Cash' | 'Stock' | 'Bond' | 'Crypto' | 'Other';
  currentValue: number;
  projectedValueAfterInflation: number;
  inflationLoss: number;
  
  // Car-specific
  projectedValueCar?: number;
  carDepreciation?: number;
  carDepreciationRate?: number;
  
  // Real Estate-specific
  projectedValueRealEstate?: number;
  realEstateAppreciation?: number;
  realEstateAppreciationRate?: number;
  
  projectionSummary: string;
  calculatedAt: string;
}

async function getAssetValuation(
  userId: string,
  assetId: string,
  token: string,
  options?: {
    inflationRate?: number;
    carDepreciationRate?: number;
    realEstateAppreciationRate?: number;
  }
): Promise<AssetValuationResponse> {
  const params = new URLSearchParams();
  if (options?.inflationRate) params.append('inflationRate', String(options.inflationRate));
  if (options?.carDepreciationRate) params.append('carDepreciationRate', String(options.carDepreciationRate));
  if (options?.realEstateAppreciationRate) params.append('realEstateAppreciationRate', String(options.realEstateAppreciationRate));

  const response = await fetch(
    `http://localhost:5000/api/Assets/users/${userId}/assets/${assetId}/valuation?${params}`,
    {
      headers: { 'Authorization': `Bearer ${token}` }
    }
  );

  if (!response.ok) throw new Error('Failed to get valuation');
  return response.json();
}

// Usage
const valuation = await getAssetValuation(
  'userId',
  'assetId',
  'token',
  {
    carDepreciationRate: 20,
    realEstateAppreciationRate: 4
  }
);

console.log(`Car value in 1 year: $${valuation.projectedValueCar?.toFixed(2)}`);
console.log(`Real estate value in 1 year: $${valuation.projectedValueRealEstate?.toFixed(2)}`);
```

---

## Integration in Recommendations

The valuation system is used by:
- **Recommendation Engine** - Factors in asset depreciation/appreciation when calculating optimal allocation
- **Financial Metrics** - Shows real vs nominal wealth growth
- **Portfolio Analysis** - Identifies assets losing value and suggests rebalancing

---

## Future Enhancements

- [ ] Multi-year projections (5-year, 10-year)
- [ ] Historical valuation tracking
- [ ] Asset-specific depreciation curves
- [ ] Market-based valuation updates
- [ ] Tax impact calculations
- [ ] Maintenance cost projections (for vehicles)
- [ ] Rental income projections (for real estate)
- [ ] Insurance cost estimates
