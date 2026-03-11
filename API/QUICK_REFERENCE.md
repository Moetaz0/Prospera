# 🎯 Prospera API - Quick Reference Card

## 🔑 Credentials Summary

| Service | Value | Status |
|---------|-------|--------|
| **Alpha Vantage API** | `E6Z36BIEYHY3F4U5` | ✅ Configured |
| **JWT Secret Key** | `ProsperaprojectSecureJWT...` (64 chars) | ✅ Configured |
| **MongoDB** | Connection string in appsettings | ✅ Configured |
| **Redis** | `localhost:6379` | ⏳ Start service |
| **Ollama** | `http://localhost:11434` | ⏳ Start service |
| **Stripe** | Ready for keys | ⏳ Add when ready |

---

## 🚀 Quick Launch Commands

```bash
# Terminal 1: Start Ollama
ollama serve

# Terminal 2: Start Redis (Docker)
docker run -d -p 6379:6379 redis:latest

# Terminal 3: Start API
cd D:\project\agent\Prospera
dotnet run --project API

# Access Swagger UI
https://localhost:7001
```

---

## 📝 API Endpoints Summary

### Users
```
POST   /api/users                    Create user
GET    /api/users/{id}              Get user dashboard
PUT    /api/users/{id}              Update user
DELETE /api/users/{id}              Delete user
```

### Assets
```
POST   /api/assets/users/{userId}                        Add asset
GET    /api/assets/users/{userId}                        List assets
GET    /api/assets/users/{userId}/assets/{id}           Get asset
PUT    /api/assets/users/{userId}/assets/{id}           Update asset
DELETE /api/assets/users/{userId}/assets/{id}           Delete asset
```

### Liabilities
```
POST   /api/liabilities/users/{userId}                        Add liability
GET    /api/liabilities/users/{userId}                        List liabilities
GET    /api/liabilities/users/{userId}/liabilities/{id}      Get liability
PUT    /api/liabilities/users/{userId}/liabilities/{id}      Update liability
DELETE /api/liabilities/users/{userId}/liabilities/{id}      Delete liability
```

### Transactions
```
POST   /api/transactions/users/{userId}                        Record transaction
GET    /api/transactions/users/{userId}?skip=0&take=50       List transactions (paginated)
GET    /api/transactions/users/{userId}/transactions/{id}    Get transaction
PUT    /api/transactions/users/{userId}/transactions/{id}    Update transaction
DELETE /api/transactions/users/{userId}/transactions/{id}    Delete transaction
```

### Financial Metrics
```
GET /api/financialmetrics/users/{userId}                Get all metrics
GET /api/financialmetrics/users/{userId}/savings-rate   Get savings rate
GET /api/financialmetrics/users/{userId}/liquidity-ratio  Get liquidity ratio
GET /api/financialmetrics/users/{userId}/debt-ratio     Get debt ratio
```

### Recommendations
```
POST   /api/recommendations/users/{userId}              Generate recommendation
GET    /api/recommendations/users/{userId}              List recommendations
GET    /api/recommendations/users/{userId}/recommendations/{id}  Get recommendation
GET    /api/recommendations/users/{userId}/latest       Get latest recommendation
```

---

## 🧪 Essential Test Cases (in order)

### 1️⃣ Create a User
```bash
curl -X POST https://localhost:7001/api/users \
  -H "Content-Type: application/json" \
  -d '{"fullName":"John Doe","email":"john@example.com"}' \
  --insecure
```

**Copy the returned `id` - you'll need it for next steps!**

### 2️⃣ Add an Asset
```bash
curl -X POST https://localhost:7001/api/assets/users/{USER_ID} \
  -H "Content-Type: application/json" \
  -d '{"name":"Tesla Stock","currentValue":15000,"type":"Stock"}' \
  --insecure
```

### 3️⃣ Add a Liability
```bash
curl -X POST https://localhost:7001/api/liabilities/users/{USER_ID} \
  -H "Content-Type: application/json" \
  -d '{"name":"Mortgage","amount":250000,"type":"Mortgage"}' \
  --insecure
```

### 4️⃣ Record a Transaction
```bash
curl -X POST https://localhost:7001/api/transactions/users/{USER_ID} \
  -H "Content-Type: application/json" \
  -d '{"amount":5000,"type":"Income","description":"Salary"}' \
  --insecure
```

### 5️⃣ Get Financial Metrics
```bash
curl -X GET https://localhost:7001/api/financialmetrics/users/{USER_ID} \
  --insecure
```

### 6️⃣ Generate AI Recommendation
```bash
curl -X POST https://localhost:7001/api/recommendations/users/{USER_ID} \
  -H "Content-Type: application/json" \
  -d '{"analysisContext":"I want to maximize returns"}' \
  --insecure
```

### 7️⃣ Get User Dashboard
```bash
curl -X GET https://localhost:7001/api/users/{USER_ID} \
  --insecure
```

---

## 📊 Enum Values Reference

### Asset Types
- `Cash`
- `Stock`
- `Bond`
- `RealEstate`
- `Crypto`
- `Other`

### Liability Types
- `CreditCard`
- `Loan`
- `Mortgage`
- `PersonalDebt`
- `Other`

### Transaction Types
- `Income`
- `Expense`
- `Investment`
- `DebtPayment`

### Risk Profiles
- `Conservative`
- `Moderate`
- `Aggressive`

---

## ✅ Success Response Examples

### 201 Created (User)
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "John Doe",
  "email": "john@example.com",
  "riskProfile": "Moderate",
  "netWorth": 0,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### 200 OK (Metrics)
```json
{
  "savingsRate": 0.25,
  "liquidityRatio": 0.15,
  "debtRatio": 0.65
}
```

### 404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "User not found"
}
```

---

## 🔐 Security Features Enabled

✅ HTTPS/TLS required  
✅ CORS properly configured  
✅ JWT authentication ready  
✅ Global exception handling  
✅ Input validation  
✅ Structured logging  
✅ SQL injection prevention (EF Core)  

---

## 🆘 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| Port 7001 already in use | Change port in `launchSettings.json` |
| MongoDB connection fails | Check credentials and IP whitelist |
| Redis connection fails | Run `redis-cli ping` - should return `PONG` |
| Ollama not responding | Run `ollama serve` in separate terminal |
| SSL certificate error | Use `--insecure` flag in curl for testing |

---

## 📱 Testing Tools

### Browser
- Swagger UI: `https://localhost:7001`

### Command Line
```bash
curl --version
curl -X GET https://localhost:7001/api/users/xxx --insecure
```

### Postman
- Import environment with `base_url` = `https://localhost:7001`
- Create requests using endpoint list above

### Visual Studio
- Set breakpoints in controllers
- Debug with F5
- View request/response in Network tab

---

## 🎓 Architecture Notes

- **Controllers**: Thin routing layer
- **MediatR**: Commands and Queries (ready to implement)
- **Contracts**: DTOs for API boundaries
- **Application**: Business logic layer
- **Domain**: Entities and business rules
- **Infrastructure**: Data access and external services

---

## 📚 Full Documentation

For comprehensive testing guide, see: `API\TESTING_GUIDE.md`

---

**Ready to test! Start the services and hit the API 🚀**
