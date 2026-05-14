# Assets & Liabilities - String Type Format

Your endpoints now accept **string types** instead of numeric values. This makes the API more user-friendly and easier to use.

## Asset Types (as Strings)

```
- Cash
- Stock
- Bond
- RealEstate
- Crypto
- Other
```

## Liability Types (as Strings)

```
- CreditCard
- Loan
- Mortgage
- PersonalDebt
- Other
```

---

## Examples

### Add Asset with String Type

**Before (Old - Numeric):**
```bash
curl -X POST http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "My Savings Account",
    "currentValue": 5000,
    "type": 0
  }'
```

**Now (New - String):** ✅
```bash
curl -X POST http://localhost:5000/api/Assets/users/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "My Savings Account",
    "currentValue": 5000,
    "type": "Cash"
  }'
```

### Add Liability with String Type

**Before (Old - Numeric):**
```bash
curl -X POST http://localhost:5000/api/Liabilities/users/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "Credit Card Debt",
    "amount": 2500,
    "type": 0
  }'
```

**Now (New - String):** ✅
```bash
curl -X POST http://localhost:5000/api/Liabilities/users/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "Credit Card Debt",
    "amount": 2500,
    "type": "CreditCard"
  }'
```

---

## All Asset Examples

### Add Cash
```json
{
  "name": "Checking Account",
  "currentValue": 10000,
  "type": "Cash"
}
```

### Add Stock
```json
{
  "name": "Apple Stock",
  "currentValue": 50000,
  "type": "Stock"
}
```

### Add Bond
```json
{
  "name": "Treasury Bond",
  "currentValue": 25000,
  "type": "Bond"
}
```

### Add Real Estate
```json
{
  "name": "Primary Residence",
  "currentValue": 500000,
  "type": "RealEstate"
}
```

### Add Crypto
```json
{
  "name": "Bitcoin Holdings",
  "currentValue": 75000,
  "type": "Crypto"
}
```

### Add Other
```json
{
  "name": "Art Collection",
  "currentValue": 15000,
  "type": "Other"
}
```

---

## All Liability Examples

### Credit Card Debt
```json
{
  "name": "Chase Credit Card",
  "amount": 5000,
  "type": "CreditCard"
}
```

### Personal Loan
```json
{
  "name": "Bank Personal Loan",
  "amount": 25000,
  "type": "Loan"
}
```

### Mortgage
```json
{
  "name": "Home Mortgage",
  "amount": 350000,
  "type": "Mortgage"
}
```

### Personal Debt
```json
{
  "name": "Money Owed to Friend",
  "amount": 1000,
  "type": "PersonalDebt"
}
```

### Other Liability
```json
{
  "name": "Student Loan",
  "amount": 50000,
  "type": "Other"
}
```

---

## Changes Summary

| What | Before | Now |
|------|--------|-----|
| **Asset Type** | `0, 1, 2, 3, 4, 5` (numbers) | `"Cash", "Stock", "Bond", etc.` (strings) |
| **Liability Type** | `0, 1, 2, 3, 4` (numbers) | `"CreditCard", "Loan", "Mortgage", etc.` (strings) |
| **Readability** | ❌ Hard to remember what 0 means | ✅ Clear and descriptive |
| **API Docs** | ❌ Requires enum mapping | ✅ Self-documenting |

---

## Error Handling

If you send an invalid type string:

```json
{
  "name": "My Asset",
  "currentValue": 1000,
  "type": "InvalidType"
}
```

The API will still accept it (for flexibility), but it will be stored as-is. For validation, the command handlers can implement type checking if needed.

---

## JavaScript/TypeScript Examples

### JavaScript Fetch

```javascript
const userId = '550e8400-e29b-41d4-a716-446655440000';
const token = 'YOUR_TOKEN';

// Add Asset
const assetResponse = await fetch(
  `http://localhost:5000/api/Assets/users/${userId}`,
  {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      name: 'My Stock Portfolio',
      currentValue: 50000,
      type: 'Stock'
    })
  }
);

const asset = await assetResponse.json();
console.log('Asset created:', asset);

// Add Liability
const liabilityResponse = await fetch(
  `http://localhost:5000/api/Liabilities/users/${userId}`,
  {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      name: 'Credit Card',
      amount: 5000,
      type: 'CreditCard'
    })
  }
);

const liability = await liabilityResponse.json();
console.log('Liability created:', liability);
```

### TypeScript with Type Safety

```typescript
interface AddAssetRequest {
  name: string;
  currentValue: number;
  type: 'Cash' | 'Stock' | 'Bond' | 'RealEstate' | 'Crypto' | 'Other';
}

interface AddLiabilityRequest {
  name: string;
  amount: number;
  type: 'CreditCard' | 'Loan' | 'Mortgage' | 'PersonalDebt' | 'Other';
}

async function addAsset(userId: string, token: string, asset: AddAssetRequest) {
  const response = await fetch(
    `http://localhost:5000/api/Assets/users/${userId}`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(asset)
    }
  );
  return response.json();
}

// Usage
addAsset('550e8400-e29b-41d4-a716-446655440000', 'token', {
  name: 'Savings Account',
  currentValue: 10000,
  type: 'Cash'
});
```

---

## Benefits

✅ **More Readable** - String names are self-documenting
✅ **Easier Integration** - Frontend developers don't need to map enum numbers
✅ **Better API Documentation** - Swagger/OpenAPI will show string examples
✅ **Reduced Errors** - Developers won't get confused by numeric enum values
✅ **Future Proof** - Easier to add/remove types without breaking numbering
