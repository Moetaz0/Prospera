# Application Layer Setup Guide

To complete the build and fully enable the Infrastructure layer, the Application layer needs to define the following entities.

## Missing Entities

### 1. Budget Entity
```csharp
// src/Application/Entities/Budget.cs
using Prospera.Domain.Common;
using Prospera.Domain.Enums;

namespace Prospera.Domain.Entities;

public class Budget : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Category { get; private set; }
    public decimal MonthlyLimit { get; private set; }
    public string Currency { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; }

    private Budget() { }

    public static Budget Create(Guid userId, string category, decimal limit, string currency)
    {
        return new Budget
        {
            UserId = userId,
            Category = category,
            MonthlyLimit = limit,
            Currency = currency,
            StartDate = DateTime.UtcNow,
            IsActive = true
        };
    }
}
```

### 2. Debt Entity
```csharp
// src/Application/Entities/Debt.cs
using Prospera.Domain.Common;
using Prospera.Domain.Enums;

namespace Prospera.Domain.Entities;

public class Debt : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public decimal Amount { get; private set; }
    public decimal InterestRate { get; private set; }
    public LiabilityType Type { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public bool IsActive { get; private set; }

    private Debt() { }

    public static Debt Create(Guid userId, string name, decimal amount, decimal interestRate, LiabilityType type)
    {
        return new Debt
        {
            UserId = userId,
            Name = name,
            Amount = amount,
            InterestRate = interestRate,
            Type = type,
            StartDate = DateTime.UtcNow,
            IsActive = true
        };
    }
}
```

### 3. Investment Entity
```csharp
// src/Application/Entities/Investment.cs
using Prospera.Domain.Common;
using Prospera.Domain.Enums;

namespace Prospera.Domain.Entities;

public class Investment : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public decimal Amount { get; private set; }
    public decimal CurrentValue { get; private set; }
    public AssetType Type { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public decimal ReturnPercentage { get; private set; }

    private Investment() { }

    public static Investment Create(Guid userId, string name, decimal amount, AssetType type)
    {
        return new Investment
        {
            UserId = userId,
            Name = name,
            Amount = amount,
            CurrentValue = amount,
            Type = type,
            PurchaseDate = DateTime.UtcNow,
            ReturnPercentage = 0
        };
    }
}
```

### 4. SavingsGoal Entity
```csharp
// src/Application/Entities/SavingsGoal.cs
using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

public class SavingsGoal : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public DateTime TargetDate { get; private set; }
    public string Currency { get; private set; }
    public bool IsActive { get; private set; }

    private SavingsGoal() { }

    public static SavingsGoal Create(Guid userId, string name, decimal targetAmount, DateTime targetDate, string currency)
    {
        return new SavingsGoal
        {
            UserId = userId,
            Name = name,
            TargetAmount = targetAmount,
            CurrentAmount = 0,
            TargetDate = targetDate,
            Currency = currency,
            IsActive = true
        };
    }
}
```

### 5. BankAccount Entity
```csharp
// src/Application/Entities/BankAccount.cs
using Prospera.Domain.Common;

namespace Prospera.Domain.Entities;

public class BankAccount : BaseEntity
{
    public Guid UserId { get; private set; }
    public string AccountName { get; private set; }
    public string AccountNumber { get; private set; }
    public string BankName { get; private set; }
    public decimal Balance { get; private set; }
    public string Currency { get; private set; }
    public string StripeAccountId { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsActive { get; private set; }

    private BankAccount() { }

    public static BankAccount Create(
        Guid userId,
        string accountName,
        string accountNumber,
        string bankName,
        string currency)
    {
        return new BankAccount
        {
            UserId = userId,
            AccountName = accountName,
            AccountNumber = accountNumber,
            BankName = bankName,
            Balance = 0,
            Currency = currency,
            IsVerified = false,
            IsActive = true
        };
    }
}
```

## Repository Interfaces

These interfaces should also be created in the Application layer:

```csharp
// src/Application/Interfaces/IBudgetRepository.cs
namespace Prospera.Application.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id);
    Task<IEnumerable<Budget>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task DeleteAsync(Guid id);
}

// Similar interfaces for:
// - IDebtRepository
// - IInvestmentRepository
// - ISavingsGoalRepository
// - IBankAccountRepository
```

## Once These Are Created

1. The build will complete successfully
2. All configurations in Infrastructure will be recognized
3. The Database migrations can be created and applied
4. Full application development can proceed

## Current Build Status

✅ Domain Layer Infrastructure: COMPLETE
⏳ Application Layer Entities: AWAITING
✅ Dependency Injection: READY
✅ Database Context: READY
✅ Identity & Authentication: READY
✅ External Services: READY

---

For questions about the Infrastructure implementation, refer to `COMPLETE_IMPLEMENTATION_REPORT.md`.
