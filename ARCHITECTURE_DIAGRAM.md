# 📊 Architecture Diagram: Valuation-Driven Recommendations

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                      PROSPERA .NET 8 API                            │
└─────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │
                    ┌───────────────┴───────────────┐
                    │                               │
        ┌──────────────────────┐       ┌────────────────────────┐
        │  RecommendationsAPI  │       │  CoachingAPI           │
        │  POST /recommendations      │  POST /coaching/session │
        │  /generate           │       │                        │
        └──────────────────────┘       └────────────────────────┘
                    │                               │
                    └───────────────┬───────────────┘
                                    ▼
        ┌───────────────────────────────────────────────────────┐
        │  Application Layer (MediatR Commands)                │
        │  ├─ GenerateRecommendationCommand ◄─── ✨ ENHANCED    │
        │  │  └─ GenerateRecommendationCommandHandler           │
        │  │     ├─ Fetches market data                         │
        │  │     ├─ Fetches valuation data ◄──────── NEW!      │
        │  │     ├─ Builds valuation context ◄────── NEW!      │
        │  │     ├─ Calls LLM with enhanced prompt             │
        │  │     └─ Returns recommendation                      │
        │  │                                                    │
        │  └─ StartCoachingSessionCommand                       │
        │     └─ StartCoachingSessionCommandHandler             │
        └───────────────────────────────────────────────────────┘
                    ▲         ▲         ▲
                    │         │         │
        ┌───────────┴──┐   ┌──┴──────┐ ┌┴────────────┐
        │ Market Data  │   │ User    │ │ Valuation  │
        │ Service      │   │ Context │ │ Data ◄─ NEW!
        └──────────────┘   └─────────┘ └────────────┘
                    │         │         │
                    └────┬────┴────┬────┘
                         ▼
        ┌───────────────────────────────────────────────────────┐
        │  Infrastructure Layer                                │
        │  ├─ MarketDataService (Alpha Vantage)               │
        │  ├─ FinancialDataService ◄─────────── NEW!           │
        │  │  ├─ GetStockValuationAsync                        │
        │  │  ├─ GetMultipleStockValuationsAsync               │
        │  │  └─ GetValuationRecommendationAsync               │
        │  ├─ LLM Providers (Ollama, OpenRouter)               │
        │  └─ Database (MSSQL)                                 │
        └───────────────────────────────────────────────────────┘
                    ▲         ▲
                    │         │
        ┌───────────┴──┐   ┌──┴──────────────┐
        │ Market Data  │   │ FastAPI (NEW)  │◄── Valuation
        │ APIs         │   │ Side System     │    Endpoint
        └──────────────┘   └─────────────────┘
```

## Data Flow: Recommendation with Valuation

```
┌─────────────────────────────────────────────────────────────────────┐
│ USER: Generate investment recommendation                            │
└─────────────────────────────────────────────────────────────────────┘
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│ CONTROLLER: RecommendationsController                               │
│ POST /api/recommendations/generate                                  │
│ ├─ Validate request                                                 │
│ └─ Dispatch GenerateRecommendationCommand                           │
└─────────────────────────────────────────────────────────────────────┘
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│ HANDLER: GenerateRecommendationCommandHandler                       │
│ Handle(GenerateRecommendationCommand)                               │
│                                                                     │
│ STEP 1: Get User Financial Profile                                 │
│ ├─ Query Assets, Liabilities, Transactions                         │
│ └─ Calculate: NetWorth, Income, Expenses                           │
│                                                                     │
│ STEP 2: Calculate Risk Tolerance (0-1.0)                           │
│ ├─ DebtRatio factor                                                │
│ ├─ SavingsRate factor                                              │
│ └─ NetWorth factor                                                 │
│                                                                     │
│ STEP 3: Fetch Market Data (Existing)                               │
│ └─ MarketDataService.GetMarketTrendDataAsync()                     │
│    Returns: StockMarketPrice, Trend, CryptoPriceUSD                │
│                                                                     │
│ STEP 4: Fetch Valuation Data ◄───────────── NEW!                   │
│ ├─ tickers = ["AAPL", "MSFT", "GOOGL", ...]                        │
│ └─ FinancialDataService.GetMultipleStockValuationsAsync()           │
│    Returns: List<StockValuationData>                               │
│    Each contains:                                                   │
│    ├─ Ticker, CurrentPrice                                         │
│    ├─ PeRatio, PbRatio, DividendYield                              │
│    ├─ FairValue, PriceTarget                                       │
│    ├─ MarginOfSafety, ValuationHealth                              │
│    └─ LastUpdated                                                   │
│                                                                     │
│ STEP 5: Build Valuation Context ◄───────── NEW!                    │
│ ├─ Categorize stocks:                                              │
│ │  ├─ Undervalued: MarginOfSafety > 20%                            │
│ │  ├─ Fair: MarginOfSafety 10-20%                                  │
│ │  └─ Overvalued: MarginOfSafety < 10%                             │
│ ├─ For each category, summarize:                                   │
│ │  ├─ Stock names and count                                        │
│ │  ├─ Key metrics (P/E, Fair Value, Dividend)                      │
│ │  └─ Recommendation                                               │
│ └─ buildValuationContext() returns: String                         │
│    Example:                                                         │
│    "UNDERVALUED OPPORTUNITIES (3):                                 │
│     - MSFT: P/E 22, Fair Value $380, Margin 18%                   │
│     - JNJ: P/E 16, Fair Value $210, Margin 7%                     │
│    FAIRLY VALUED (5): ...                                          │
│    OVERVALUED (2): ..."                                            │
│                                                                     │
│ STEP 6: Build Enhanced AI Prompt ◄──────── ENHANCED!               │
│ ├─ Existing prompt includes:                                       │
│ │  ├─ Investor profile (networth, income, etc.)                    │
│ │  ├─ Risk profile classification                                  │
│ │  └─ Market conditions (S&P, BTC price)                           │
│ └─ NEW additions:                                                   │
│    ├─ Stock valuation analysis                                     │
│    ├─ Valuation-informed rules:                                    │
│    │  "UNDERVALUED (P/E<15, Safety>20%): Strong buy"              │
│    │  "FAIR (P/E 15-25, Safety 10-20%): Good"                     │
│    │  "OVERVALUED (P/E>25, Safety<10%): AVOID"                    │
│    └─ Coaching philosophy with data awareness                      │
│                                                                     │
│ STEP 7: Call LLM with Enhanced Prompt                              │
│ ├─ LLMProvider.GenerateResponseAsync(prompt)                       │
│ ├─ LLM sees: user profile + market data + stock valuations         │
│ └─ LLM generates: allocation percentages                           │
│    Example output:                                                  │
│    "45% Stocks, 35% Bonds, 15% Real Estate, 3% Crypto, 2% Cash"   │
│                                                                     │
│ STEP 8: Extract & Save Recommendation                              │
│ ├─ Parse LLM response                                              │
│ ├─ Create InvestmentRecommendation entity                          │
│ └─ Save to database via repository                                 │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│ RESPONSE: InvestmentRecommendationDto                               │
│ {                                                                   │
│   "id": "...",                                                      │
│   "allocation": "45% Stocks, 35% Bonds, 15% RE, 3% Crypto, 2% Cash",
│   "explanation": "Based on valuation analysis showing MSFT at P/E   │
│     22 (fair value), JNJ at P/E 16 (undervalued, 3.2% dividend)...│
│     Recommend quality dividend stocks + fair valued tech leaders    │
│     with mixed bonds for stability..."                              │
│ }                                                                   │
└─────────────────────────────────────────────────────────────────────┘
```

## Component Interaction Diagram

```
                    ┌─────────────────────────────┐
                    │  User Request               │
                    └──────────────┬──────────────┘
                                   │
                    ┌──────────────┴──────────────┐
                    │ RecommendationsController  │
                    └──────────────┬──────────────┘
                                   │
        ┌──────────────────────────┼──────────────────────────┐
        │                          │                          │
        ▼                          ▼                          ▼
┌──────────────┐         ┌──────────────────┐      ┌────────────────┐
│ User Data    │         │ Market Data      │      │ Valuation Data │
│ Repository  │         │ Service          │      │ Service (NEW!) │
│             │         │                  │      │                │
│ -Assets     │         │ -StockPrice      │      │ -P/E Ratio     │
│ -Liabilities│         │ -CryptoPriceUSD  │      │ -Fair Value    │
│ -Transact.  │         │ -StockTrend      │      │ -PB Ratio      │
│             │         │                  │      │ -Dividend      │
└──────────────┘         └──────────────────┘      │ -Margin Safety │
        │                          │              └────────────────┘
        │                          │                      │
        └──────────────┬───────────┴──────────────────────┘
                       │
        ┌──────────────▼─────────────────────────┐
        │ GenerateRecommendationHandler          │
        │                                        │
        │ CalculateRiskTolerance()               │
        │ BuildValuationContext() ◄───── NEW!    │
        │ BuildPersonalizedAIPrompt() ◄─ ENHANCED
        │                                        │
        └──────────────┬─────────────────────────┘
                       │
        ┌──────────────▼────────────────────────┐
        │ LLM Provider                          │
        │ (Ollama / OpenRouter)                 │
        │                                       │
        │ Processes enhanced prompt with:       │
        │ • User financial profile              │
        │ • Market conditions                   │
        │ • Stock valuations ◄────── NEW!       │
        │                                       │
        └──────────────┬────────────────────────┘
                       │
        ┌──────────────▼────────────────────────┐
        │ InvestmentRecommendation              │
        │ (with valuation reasoning)            │
        │                                       │
        │ Allocation + Explanation              │
        │ with specific stock names ◄─ ENHANCED │
        └────────────────────────────────────────┘
```

## Valuation Categories & Stock Distribution

```
Margin of Safety Distribution
┌──────────────────────────────────────────────────┐
│                                                  │
│  25%+ Safety  │   15-20% Safety   │  5-10% Safe │  0-5% Safe  │ <0%
│  (Steal!)    │   (Fair)          │  (Risky)    │  (Overval)  │(Bad)
│              │                   │             │             │
│  ██████████  │   ███████         │  ███        │  ██         │ █
│  AAPL JNJ    │   MSFT GOOGL      │  AMZN TSL   │  TSLA       │ NFLX
│  V WMT       │   BRK.B DIS       │  META       │             │
│              │                   │             │             │
└──────────────────────────────────────────────────┘

From 10 Popular Stocks:
 Undervalued (>20%):    3 stocks
 Fair (10-20%):         5 stocks
 Overvalued (<10%):     2 stocks

Recommendation Action:
- Undervalued: BUY (strong buying signal)
- Fair: BUY/HOLD (good entry point)
- Overvalued: SELL/WAIT (avoid or wait for pullback)
```

## Prompt Enhancement Visualization

```
┌─────────────────────────────────────────────────────────┐
│ BEFORE (Old Prompt)                                     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ You are a financial coach.                              │
│ INVESTOR PROFILE:                                       │
│ - Net Worth: $100,000                                   │
│ - Risk Profile: Conservative                           │
│                                                         │
│ CURRENT MARKET:                                         │
│ - Stock Market: bullish                                 │
│ - Bitcoin: $45,000                                      │
│                                                         │
│ ALLOCATION TASK:                                        │
│ Suggest portfolio allocation:                           │
│ 1. Stocks  2. Bonds  3. Real Estate  4. Crypto  5. Cash │
│                                                         │
│ (LLM generates generic allocation)                     │
│                                                         │
└─────────────────────────────────────────────────────────┘
                          ▼
                    (ENHANCEMENT)
                          ▼
┌─────────────────────────────────────────────────────────┐
│ AFTER (Enhanced Prompt)                                 │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ You are a financial coach.                              │
│ INVESTOR PROFILE:                                       │
│ - Net Worth: $100,000                                   │
│ - Risk Profile: Conservative                           │
│                                                         │
│ CURRENT MARKET:                                         │
│ - Stock Market: bullish                                 │
│ - Bitcoin: $45,000                                      │
│                                                         │
│ ✨ STOCK VALUATIONS (Data-Driven):                      │
│   UNDERVALUED OPPORTUNITIES (3):                        │
│   - JNJ: P/E 16.5, Fair $210, Margin 7%, Div 3.2%    │
│   - V: P/E 28, Fair Value $295, Margin 5%, Div 1.8%   │
│                                                         │
│   FAIRLY VALUED (5):                                    │
│   - MSFT: P/E 22.1, Target $380, Div 0.8%             │
│   - AAPL: P/E 25.4, Target $195, Div 0.4%             │
│                                                         │
│   OVERVALUED (2):                                       │
│   - TSLA: P/E 65, Fair $150, overvalued 15%            │
│                                                         │
│ ✨ VALUATION-INFORMED RULES:                            │
│   - Undervalued (Safety >20%): STRONG BUY              │
│   - Fair (Safety 10-20%): GOOD for balanced            │
│   - Overvalued (Safety <10%): AVOID                    │
│                                                         │
│ ALLOCATION TASK:                                        │
│ Suggest portfolio allocation considering:              │
│ - User risk profile                                     │
│ - Available undervalued opportunities                   │
│ - Stock valuations                                      │
│ 1. Stocks  2. Bonds  3. Real Estate  4. Crypto  5. Cash │
│                                                         │
│ (LLM generates valuation-aware allocation)             │
│ → "45% Stocks (JNJ + MSFT for value),                  │
│    35% Bonds, 15% RE, 3% Crypto, 2% Cash"             │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## Technology Stack Integration

```
┌────────────────────────────────────────────────────────┐
│                   .NET 8 Application                   │
├────────────────────────────────────────────────────────┤
│                                                        │
│  MediatR          EF Core            Mapster          │
│  (Commands)       (Database)         (Mapping)        │
│      │                │                  │            │
│      └────────────────┼──────────────────┘            │
│                       │                               │
│  ┌──────────────────────────────────────────────┐    │
│  │    HTTP Client (Valuation Data Fetching)     │    │
│  ├──────────────────────────────────────────────┤    │
│  │ IFinancialDataService                        │    │
│  │ ├─ GetStockValuationAsync                   │    │
│  │ ├─ GetMultipleStockValuationsAsync          │    │
│  │ └─ GetValuationRecommendationAsync          │    │
│  └──────────────────────────────────────────────┘    │
│                       │                               │
│          ┌────────────┴────────────┐                 │
│          ▼                         ▼                 │
│  ┌──────────────┐         ┌──────────────┐         │
│  │ FastAPI      │         │ LLM Provider │         │
│  │ /valuation/* │         │ (Ollama/OR)  │         │
│  └──────────────┘         └──────────────┘         │
│          │                         │                │
└──────────┼─────────────────────────┼────────────────┘
           │                         │
    Stock Valuations           Generated Text
    (P/E, Fair Value)          (Recommendations)
```

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   Production Environment                │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌──────────────┐          ┌──────────────┐            │
│  │ Prospera API │ ◄────────► FastAPI Server │           │
│  │ (.NET 8)     │          (Financial Data) │          │
│  └──────────────┘          └──────────────┘            │
│        │                            │                  │
│        └────────────────┬───────────┘                  │
│                         ▼                              │
│              ┌────────────────────┐                    │
│              │ MSSQL Database     │                    │
│              │ Recommendations    │                    │
│              │ Coaching Sessions  │                    │
│              │ User Data          │                    │
│              └────────────────────┘                    │
│                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────┐  │
│  │ Ollama       │  │ OpenRouter   │  │ Market API │  │
│  │ (Local LLM)  │  │ (Cloud LLM)  │  │ (AlphaVant)│  │
│  └──────────────┘  └──────────────┘  └────────────┘  │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

This architecture ensures:
✅ Separation of concerns
✅ Clean code organization
✅ Scalability
✅ Maintainability
✅ Data-driven recommendations
