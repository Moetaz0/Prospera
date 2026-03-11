# 🚀 Prospera API - Complete Testing Guide

## ✅ Configuration Status

### Credentials Configured:
- ✅ **Alpha Vantage API Key**: `E6Z36BIEYHY3F4U5`
- ✅ **JWT Secret Key**: `ProsperaprojectSecureJWTKeyFor.NET8ApplicationWithHS256Encryption2024`
- ⏳ **Ollama**: Run locally on `http://localhost:11434`
- ⏳ **Stripe API Keys**: Add when ready

---

## 🎯 Pre-Launch Requirements

### 1. **Start Required Services**

#### Start Ollama (for AI Recommendations)
```bash
# Open a new terminal and run:
ollama serve
# or if you have ollama installed: 
ollama run llama3.2
```

#### Start Redis (for Caching)
```bash
# Option 1: Using Docker
docker run -d -p 6379:6379 redis:latest

# Option 2: Using Windows Subsystem for Linux (WSL)
redis-server

# Option 3: Using Redis from Windows installed version
redis-server
```

#### MongoDB (Already configured)
- Connection: `mongodb+srv://Prospera:Slm3laykom1@prospera.t0ctx82.mongodb.net/`
- No action needed - connection string is configured

---

## 🔧 Launch the API

### From Command Line:
```bash
cd D:\project\agent\Prospera

# Clean build (recommended first time)
dotnet clean
dotnet build

# Run the API
dotnet run --project API

# Or with hot reload enabled
dotnet watch run --project API
```

### Expected Output:
```
[INF] Starting Prospera API
[INF] Application started. Press Ctrl+C to shut down.
[INF] Hosting environment: Development
[INF] Content root path: D:\project\agent\Prospera\API
```

### Access the API:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:7001`
- **Swagger UI** (Auto-opens): `https://localhost:7001/swagger`

---

## 📖 Swagger UI Testing

### 1. **Open Swagger Documentation**
- Navigate to: `https://localhost:7001`
- All endpoints are documented with:
  - Request schemas
  - Response schemas
  - HTTP status codes
  - Example values

### 2. **Test Endpoints in Swagger UI**

Click the **"Try it out"** button on any endpoint to test directly in the browser.

---

## 🧪 Complete API Testing Workflow

### Phase 1: User Management

#### 1.1 Create a User
```
POST /api/users
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "john.doe@example.com"
}

Expected Response (201 Created):
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "riskProfile": "Moderate",
  "netWorth": 0,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### 1.2 Get User Dashboard
```
GET /api/users/{userId}

Expected Response (200 OK):
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "riskProfile": "Moderate",
  "netWorth": 50000,
  "assets": [],
  "liabilities": [],
  "financialMetrics": null,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### 1.3 Update User Profile
```
PUT /api/users/{userId}
Content-Type: application/json

{
  "fullName": "John Doe Updated",
  "email": "john.newemail@example.com",
  "riskProfile": "Aggressive"
}

Expected Response (200 OK): Updated user details
```

---

### Phase 2: Assets Management

#### 2.1 Add Asset
```
POST /api/assets/users/{userId}
Content-Type: application/json

{
  "name": "Tesla Stock",
  "currentValue": 15000,
  "type": "Stock"
}

Expected Response (201 Created):
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "name": "Tesla Stock",
  "currentValue": 15000,
  "type": "Stock",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "createdAt": "2024-01-15T10:35:00Z"
}
```

#### 2.2 Get All User Assets
```
GET /api/assets/users/{userId}

Expected Response (200 OK):
[
  {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "name": "Tesla Stock",
    "currentValue": 15000,
    "type": "Stock",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "createdAt": "2024-01-15T10:35:00Z"
  }
]
```

#### 2.3 Valid Asset Types
```
- Cash
- Stock
- Bond
- RealEstate
- Crypto
- Other
```

---

### Phase 3: Liabilities Management

#### 3.1 Add Liability (Debt)
```
POST /api/liabilities/users/{userId}
Content-Type: application/json

{
  "name": "Mortgage",
  "amount": 250000,
  "type": "Mortgage"
}

Expected Response (201 Created):
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "name": "Mortgage",
  "amount": 250000,
  "type": "Mortgage",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "createdAt": "2024-01-15T10:40:00Z"
}
```

#### 3.2 Valid Liability Types
```
- CreditCard
- Loan
- Mortgage
- PersonalDebt
- Other
```

---

### Phase 4: Transactions

#### 4.1 Record Transaction
```
POST /api/transactions/users/{userId}
Content-Type: application/json

{
  "amount": 5000,
  "type": "Income",
  "description": "Monthly salary"
}

Expected Response (201 Created):
{
  "id": "880e8400-e29b-41d4-a716-446655440003",
  "amount": 5000,
  "date": "2024-01-15T10:45:00Z",
  "type": "Income",
  "description": "Monthly salary",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "createdAt": "2024-01-15T10:45:00Z"
}
```

#### 4.2 Transaction Types
```
- Income
- Expense
- Investment
- DebtPayment
```

#### 4.3 Get User Transactions (with Pagination)
```
GET /api/transactions/users/{userId}?skip=0&take=50

Expected Response (200 OK):
[
  { transaction details... }
]
```

---

### Phase 5: Financial Metrics

#### 5.1 Calculate Financial Metrics
```
GET /api/financialmetrics/users/{userId}

Expected Response (200 OK):
{
  "savingsRate": 0.25,
  "liquidityRatio": 0.15,
  "debtRatio": 0.65
}
```

#### 5.2 Get Individual Metrics
```
GET /api/financialmetrics/users/{userId}/savings-rate
GET /api/financialmetrics/users/{userId}/liquidity-ratio
GET /api/financialmetrics/users/{userId}/debt-ratio
```

---

### Phase 6: AI Recommendations

#### 6.1 Generate Investment Recommendation
```
POST /api/recommendations/users/{userId}
Content-Type: application/json

{
  "analysisContext": "I want to maximize returns with moderate risk"
}

Expected Response (201 Created):
{
  "id": "990e8400-e29b-41d4-a716-446655440004",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "suggestedAllocation": "40% Stocks, 30% Bonds, 20% Real Estate, 10% Crypto",
  "explanation": "Based on your risk profile and financial metrics...",
  "createdAt": "2024-01-15T10:50:00Z"
}
```

#### 6.2 Get Latest Recommendation
```
GET /api/recommendations/users/{userId}/latest

Expected Response (200 OK): Latest recommendation details
```

---

## 🧬 Using cURL for Testing

### Create User (cURL)
```bash
curl -X POST https://localhost:7001/api/users \
  -H "Content-Type: application/json" \
  -d "{\"fullName\":\"Jane Smith\",\"email\":\"jane@example.com\"}" \
  --insecure
```

### Add Asset (cURL)
```bash
curl -X POST https://localhost:7001/api/assets/users/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"Savings Account\",\"currentValue\":5000,\"type\":\"Cash\"}" \
  --insecure
```

### Get Metrics (cURL)
```bash
curl -X GET https://localhost:7001/api/financialmetrics/users/550e8400-e29b-41d4-a716-446655440000 \
  --insecure
```

---

## 📊 Postman Testing

### 1. Import Environment Variables
```json
{
  "id": "prospera-env",
  "name": "Prospera Dev",
  "values": [
    {
      "key": "base_url",
      "value": "https://localhost:7001",
      "enabled": true
    },
    {
      "key": "user_id",
      "value": "550e8400-e29b-41d4-a716-446655440000",
      "enabled": true
    }
  ]
}
```

### 2. Create Postman Collection
Use the URLs above and import them into Postman for organized testing.

---

## ✅ Testing Checklist

### Users Endpoints
- [ ] `POST /api/users` - Create user
- [ ] `GET /api/users/{id}` - Get user dashboard
- [ ] `PUT /api/users/{id}` - Update user
- [ ] `DELETE /api/users/{id}` - Delete user

### Assets Endpoints
- [ ] `POST /api/assets/users/{userId}` - Add asset
- [ ] `GET /api/assets/users/{userId}` - List assets
- [ ] `GET /api/assets/users/{userId}/assets/{id}` - Get asset
- [ ] `PUT /api/assets/users/{userId}/assets/{id}` - Update asset
- [ ] `DELETE /api/assets/users/{userId}/assets/{id}` - Delete asset

### Liabilities Endpoints
- [ ] `POST /api/liabilities/users/{userId}` - Add liability
- [ ] `GET /api/liabilities/users/{userId}` - List liabilities
- [ ] `GET /api/liabilities/users/{userId}/liabilities/{id}` - Get liability
- [ ] `PUT /api/liabilities/users/{userId}/liabilities/{id}` - Update liability
- [ ] `DELETE /api/liabilities/users/{userId}/liabilities/{id}` - Delete liability

### Transactions Endpoints
- [ ] `POST /api/transactions/users/{userId}` - Record transaction
- [ ] `GET /api/transactions/users/{userId}` - List transactions
- [ ] `GET /api/transactions/users/{userId}/transactions/{id}` - Get transaction
- [ ] `PUT /api/transactions/users/{userId}/transactions/{id}` - Update transaction
- [ ] `DELETE /api/transactions/users/{userId}/transactions/{id}` - Delete transaction

### Financial Metrics Endpoints
- [ ] `GET /api/financialmetrics/users/{userId}` - Get all metrics
- [ ] `GET /api/financialmetrics/users/{userId}/savings-rate` - Get savings rate
- [ ] `GET /api/financialmetrics/users/{userId}/liquidity-ratio` - Get liquidity ratio
- [ ] `GET /api/financialmetrics/users/{userId}/debt-ratio` - Get debt ratio

### Recommendations Endpoints
- [ ] `POST /api/recommendations/users/{userId}` - Generate recommendation
- [ ] `GET /api/recommendations/users/{userId}` - List recommendations
- [ ] `GET /api/recommendations/users/{userId}/recommendations/{id}` - Get recommendation
- [ ] `GET /api/recommendations/users/{userId}/latest` - Get latest recommendation

---

## 🔍 Error Handling Tests

### Test 404 Not Found
```bash
curl -X GET https://localhost:7001/api/users/00000000-0000-0000-0000-000000000000 \
  --insecure
```

Expected Response:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "User not found"
}
```

### Test Validation Error (400)
```bash
curl -X POST https://localhost:7001/api/users \
  -H "Content-Type: application/json" \
  -d "{\"fullName\":\"\",\"email\":\"invalid-email\"}" \
  --insecure
```

Expected Response:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "fullName": ["Full name is required"],
    "email": ["Invalid email format"]
  }
}
```

---

## 📈 Performance Testing

### Load Testing with Apache Bench
```bash
ab -n 100 -c 10 https://localhost:7001/api/users/550e8400-e29b-41d4-a716-446655440000
```

### Load Testing with Wrk
```bash
wrk -t4 -c100 -d30s https://localhost:7001/api/users/550e8400-e29b-41d4-a716-446655440000
```

---

## 🔐 Security Testing

### 1. HTTPS/TLS
- ✅ API enforces HTTPS in production
- ✅ Swagger UI available over HTTPS
- ✅ JWT tokens can be added to Authorization header

### 2. CORS Testing
```javascript
// From browser console on different domain
fetch('https://localhost:7001/api/users', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    fullName: 'Test User',
    email: 'test@example.com'
  })
})
.then(r => r.json())
.then(console.log)
```

### 3. Rate Limiting (Future Enhancement)
- Consider adding API rate limiting middleware
- Suggested: 100 requests per minute per IP

---

## 🚨 Common Issues & Troubleshooting

### Issue: "Connection refused" on localhost:7001
**Solution:**
- Ensure no other service is using port 7001
- Check firewall settings
- Try different port in `launchSettings.json`

### Issue: "MongoDB connection timeout"
**Solution:**
- Verify MongoDB Atlas credentials
- Check network connectivity
- Ensure IP is whitelisted in MongoDB Atlas

### Issue: "Redis connection failed"
**Solution:**
- Ensure Redis is running: `redis-cli ping` should return `PONG`
- Check Redis is accessible on localhost:6379

### Issue: "Ollama not responding"
**Solution:**
- Run: `ollama serve` in separate terminal
- Ensure port 11434 is accessible
- Model `llama3.2` should be downloaded first

### Issue: "JWT validation error"
**Solution:**
- Ensure JWT key in `appsettings.json` is ≥ 32 characters
- Current key is 64+ characters ✅

---

## 📞 Support & Next Steps

### 1. **Add Stripe Integration**
When you have Stripe API keys:
```json
"Stripe": {
  "SecretKey": "sk_test_YOUR_ACTUAL_KEY",
  "PublishableKey": "pk_test_YOUR_ACTUAL_KEY"
}
```

### 2. **Implement MediatR Commands**
Replace TODO comments in controllers with actual command handlers.

### 3. **Deploy to Production**
- Update connection strings for production databases
- Use environment variables for secrets
- Enable HTTPS with proper certificates
- Disable Swagger in production
- Enable detailed logging

---

## ✨ Quick Start Summary

```bash
# 1. Start Ollama
ollama serve

# 2. Start Redis
docker run -d -p 6379:6379 redis:latest

# 3. Navigate to project
cd D:\project\agent\Prospera

# 4. Run API
dotnet run --project API

# 5. Open Swagger UI
https://localhost:7001

# 6. Test endpoints using Swagger or cURL
# See examples above
```

---

**You're all set! 🚀 The Prospera API is ready for testing.**
