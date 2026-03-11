# Prospera Application Layer

## Overview

The Application layer implements the CQRS (Command Query Responsibility Segregation) pattern using MediatR, with FluentValidation for input validation and AutoMapper for DTO mapping.

This layer orchestrates business logic operations while maintaining strict separation from Infrastructure and API layers.

## Architecture

### Technology Stack
- **MediatR** v12.2.0 - CQRS implementation
- **FluentValidation** v11.9.2 - Input validation
- **AutoMapper** v13.0.1 - Object mapping
- **.NET 8.0** - Target framework

### Core Components

#### 1. **Commands** (State-Changing Operations)
- `CreateUserCommand` - Create new user
- `AddAssetCommand` - Add asset to user
- `AddTransactionCommand` - Record transaction
- `GenerateInvestmentRecommendationCommand` - Generate AI recommendations

Each command has:
- Command definition (IRequest<T>)
- Handler (IRequestHandler<,>)
- Validator (AbstractValidator<T>)

#### 2. **Queries** (Read-Only Operations)
- `GetUserDashboardQuery` - Aggregated user financial dashboard
- `CalculateFinancialMetricsQuery` - Calculate financial metrics

#### 3. **DTOs** (Data Transfer Objects)
- `UserDto` - User representation
- `AssetDto` - Asset representation
- `LiabilityDto` - Liability representation
- `TransactionDto` - Transaction representation
- `FinancialMetricsDto` - Metrics representation
- `InvestmentRecommendationDto` - Recommendation representation

#### 4. **Service Interfaces**
- `IMarketDataService` - Market data integration
- `IAiRecommendationService` - AI recommendation generation
- `IFinancialAnalysisService` - Financial calculations
- `IDomainEventDispatcher` - Domain event publishing

#### 5. **Behaviors**
- `ValidationBehavior` - Automatic input validation via MediatR pipeline

#### 6. **Mappings**
- `MappingProfile` - AutoMapper configuration for entity-to-DTO mapping

## Key Features

### CQRS Pattern
```
Commands: CreateUserCommand, AddAssetCommand, etc.
        ↓
    Handlers (Modify State)
        ↓
    Repository/Domain Updates

Queries: GetUserDashboardQuery, CalculateFinancialMetricsQuery
       ↓
    Handlers (Read-Only)
       ↓
    Return Aggregated Data
```

### Input Validation
FluentValidation rules are automatically executed before command handlers via the ValidationBehavior:

```csharp
RuleFor(x => x.FullName)
    .NotEmpty().WithMessage("Full name is required")
    .MaximumLength(256);
```

### AI-Powered Recommendations
The `GenerateInvestmentRecommendationCommand`:
1. Retrieves user financial data
2. Calculates financial metrics
3. Fetches market context
4. Builds intelligent AI prompt
5. Calls AI service for recommendations
6. Saves recommendation to repository

### Dashboard Aggregation
`GetUserDashboardQuery` aggregates:
- User profile with net worth
- Financial metrics
- Recent transactions
- Latest AI recommendation

## Dependency Injection

Register Application services in your API startup:

```csharp
services.AddApplicationServices();
```

This registers:
- MediatR with all handlers
- AutoMapper with profiles
- FluentValidation with validators
- Validation behavior in MediatR pipeline

## Usage Examples

### Creating a User
```csharp
var command = new CreateUserCommand
{
    FullName = "John Doe",
    Email = "john@example.com"
};

var result = await mediator.Send(command);
```

### Adding an Asset
```csharp
var command = new AddAssetCommand
{
    UserId = userId,
    Name = "Tesla Stock",
    Value = 10000m,
    Type = "Stock"
};

var result = await mediator.Send(command);
```

### Getting Dashboard
```csharp
var query = new GetUserDashboardQuery(userId);
var dashboard = await mediator.Send(query);
```

### Generating Recommendations
```csharp
var command = new GenerateInvestmentRecommendationCommand(userId);
var recommendation = await mediator.Send(command);
```

## Architecture Rules

✅ **Depends ONLY on Domain**
- No Infrastructure references
- No HTTP calls
- No database access directly
- All dependencies via interfaces

✅ **Clean Separation**
- No business logic in API controllers
- All orchestration in Application layer
- Handlers are thin, focused, and testable

✅ **SOLID Principles**
- Single Responsibility: Each command/query has one purpose
- Open/Closed: Easy to extend with new commands/queries
- Liskov Substitution: Proper interface contracts
- Interface Segregation: Small, focused interfaces
- Dependency Inversion: All dependencies via interfaces

✅ **Async/Await Throughout**
- All repository calls are async
- All service calls are async
- Proper CancellationToken support

## File Structure

```
src/Application/
├── Common/
│   ├── Interfaces/
│   │   ├── IMarketDataService.cs
│   │   ├── IAiRecommendationService.cs
│   │   ├── IFinancialAnalysisService.cs
│   │   └── IDomainEventDispatcher.cs
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs
│   └── Mappings/
│       └── MappingProfile.cs
├── Features/
│   ├── Users/
│   │   ├── Commands/ (CreateUserCommand, Handler, Validator)
│   │   └── Queries/ (GetUserDashboardQuery, Handler)
│   ├── Assets/
│   │   └── Commands/ (AddAssetCommand, Handler, Validator)
│   ├── Transactions/
│   │   └── Commands/ (AddTransactionCommand, Handler, Validator)
│   ├── Recommendations/
│   │   └── Commands/ (GenerateInvestmentRecommendationCommand, Handler)
│   └── Metrics/
│       └── Queries/ (CalculateFinancialMetricsQuery, Handler)
├── DTOs/
│   ├── UserDto.cs
│   ├── AssetDto.cs
│   ├── LiabilityDto.cs
│   ├── TransactionDto.cs
│   ├── FinancialMetricsDto.cs
│   └── InvestmentRecommendationDto.cs
└── DependencyInjection.cs
```

## Testing

All components are designed for easy testing:

### Unit Testing Handlers
```csharp
[Test]
public async Task CreateUserCommand_ValidInput_CreatesUser()
{
    // Arrange
    var handler = new CreateUserCommandHandler(mockRepository, mockMapper);
    var command = new CreateUserCommand { FullName = "Test", Email = "test@test.com" };

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.IsNotNull(result);
}
```

### Testing Validation
```csharp
[Test]
public void CreateUserCommandValidator_EmptyEmail_FailsValidation()
{
    // Arrange
    var validator = new CreateUserCommandValidator();
    var command = new CreateUserCommand { FullName = "Test", Email = "" };

    // Act
    var result = validator.Validate(command);

    // Assert
    Assert.IsFalse(result.IsValid);
}
```

## Extension Points

To add new functionality:

1. **New Command**
   - Create command class implementing `IRequest<T>`
   - Create handler implementing `IRequestHandler<,>`
   - Create validator implementing `AbstractValidator<T>`
   - Place in `Features/[Feature]/Commands/`

2. **New Query**
   - Create query class implementing `IRequest<T>`
   - Create handler implementing `IRequestHandler<,>`
   - Place in `Features/[Feature]/Queries/`

3. **New DTO**
   - Create DTO class
   - Add mapping in `MappingProfile`

## Best Practices

✅ Always validate input with FluentValidation
✅ Use async/await for all I/O operations
✅ Return DTOs from handlers (not domain entities)
✅ Keep handlers focused on orchestration
✅ Use dependency injection for all services
✅ Throw appropriate exceptions with clear messages
✅ Log important operations via ILogger
✅ Use CancellationToken for long-running operations

## Performance Considerations

- MediatR is lightweight with minimal overhead
- Validation happens once per request
- DTOs are small and efficient for transfer
- Queries can include pagination for large datasets
- Consider caching for expensive calculations

## Security

- Input validation prevents injection attacks
- DTOs hide internal entity details
- All external service calls abstracted via interfaces
- Domain logic remains protected in Domain layer
- Authorization can be added as a behavior

---

**Status**: Production Ready ✅
**Version**: 1.0
**Quality**: Enterprise Grade
