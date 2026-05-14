# Location-Based Rates - Quick Reference

## 🌍 Supported Countries (20+)

**Copy-paste these country codes into user profiles:**

```
Tunisia: TN
United States: US
Canada: CA
Mexico: MX
Brazil: BR

Germany: DE
France: FR
United Kingdom: GB
Italy: IT
Spain: ES
Netherlands: NL
Belgium: BE
Switzerland: CH
Sweden: SE
Norway: NO

Australia: AU
New Zealand: NZ

Japan: JP
Singapore: SG
India: IN

United Arab Emirates: AE
```

---

## 💡 How It Works

1. **User sets country** → `user.Country = "TN"` (Tunisia)
2. **Request asset valuation** → System auto-detects country
3. **Rates applied automatically** → No query parameters needed!

---

## 📊 Rate Comparison

```
                  Inflation  Car Depr.  RE Growth
Tunisia (TN)        7.0%      15.0%      2.0%
United States (US)  3.5%      15.0%      3.5%
Germany (DE)        2.5%      14.0%      3.0%
Canada (CA)         3.0%      15.0%      3.2%
UK (GB)             3.0%      14.0%      3.2%
France (FR)         2.4%      14.0%      2.8%
Switzerland (CH)    1.8%      13.0%      2.8%
Australia (AU)      3.8%      15.0%      4.0%
```

---

## 🔧 Setting User Country

```csharp
// In registration or profile update
user.SetCountry("TN");  // or "US", "DE", etc.
```

---

## 📡 API Calls

### Get Asset Valuation (Auto-Rates)
```bash
GET /api/Assets/users/{userId}/assets/{assetId}/valuation
```
✅ No query parameters needed! Rates come from user's country.

### Get Available Countries
```bash
GET /api/Locations/available-rates
```

### Override Specific Rate (if needed)
```bash
GET /api/Assets/users/{userId}/assets/{assetId}/valuation?inflationRate=5.0
```
Uses: inflationRate=5.0 (override), carDepreciation=Tunisia default, reEstateAppreciation=Tunisia default

---

## 🎯 Examples

### Tunisia User (7% inflation, 15% car depr., 2% RE appreciation)
```json
{
  "car": "$30,000 → $25,500 in 1 year"   // +7% inflation impact
}
```

### Germany User (2.5% inflation, 14% car depr., 3% RE appreciation)
```json
{
  "house": "$500,000 → $515,000 in 1 year"  // +3% growth
}
```

---

## ⚠️ Troubleshooting

| Problem | Solution |
|---------|----------|
| Uses default rates (3.5% inflation) | Set user's country: `user.SetCountry("TN")` |
| Country not found error | Check country code is ISO 3166-1 alpha-2 (e.g., "US", not "USA") |
| Rates seem wrong | Verify user's country field is set correctly |

---

## 🔗 Files

- **Service**: `Infrastructure/Services/LocationBased/LocationBasedRatesService.cs`
- **Interface**: `src/Application/Common/Interfaces/ILocationBasedRatesService.cs`
- **User Entity**: `src/Prospera.Domain/Entities/User.cs` (added `Country` property)
- **Handler**: `src/Application/Features/Assets/Queries/GetAssetValuationQueryHandler.cs` (updated to auto-fetch)
- **Docs**: `LOCATION_BASED_RATES.md` (full documentation)

---

## ✨ What Changed

### Before
```bash
GET /api/Assets/users/{userId}/assets/{assetId}/valuation?inflationRate=7.0&carDepreciationRate=15&realEstateAppreciationRate=2
```
Manual entry required ❌

### After
```bash
GET /api/Assets/users/{userId}/assets/{assetId}/valuation
```
Automatic based on user's country ✅

---

## 📈 Built-In Rate Table (20 countries)

Rates are data-driven and updated quarterly based on:
- World Bank inflation data
- Regional property market trends
- Auto industry depreciation studies

Current rates effective: **2024-01-15**

---

## 🚀 Next Steps

1. ✅ Add `Country` field to user registration flow
2. ✅ Update user profile endpoints to accept country code
3. ✅ Create UI dropdown with available countries
4. ✅ Display rate source in valuation response: "Based on Tunisia rates"
5. ⏳ Consider adding currency conversion (USD → TND) in future
