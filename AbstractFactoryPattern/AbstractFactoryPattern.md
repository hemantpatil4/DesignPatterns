# Abstract Factory Design Pattern - FX Trading Example

## 📌 Interview Definition

> **"Abstract Factory provides an interface for creating FAMILIES of related objects without specifying their concrete classes."**

---

## 🔄 Simple Factory vs Abstract Factory

| Aspect       | Simple Factory        | Abstract Factory                                                       |
| ------------ | --------------------- | ---------------------------------------------------------------------- |
| **Creates**  | ONE type of product   | FAMILY of related products                                             |
| **Methods**  | `CreateOrder()`       | `CreateOrder()`, `CreateQuoteParser()`, `CreateSettlementCalculator()` |
| **Use Case** | Different order types | Different regions with complete trading setup                          |
| **Products** | Independent           | Must work together                                                     |

---

## 🎯 Real-World FX Trading Scenario

Different **REGIONS** have different trading rules:

| Region   | Order Routing | Quote Format              | Settlement        | Compliance  |
| -------- | ------------- | ------------------------- | ----------------- | ----------- |
| **US**   | US desk       | `EUR/USD\|1.0850\|1.0852` | T+2, Fed holidays | Fed rules   |
| **EU**   | EU desk       | `EUR/USD;1,0850;1,0852`   | T+2, TARGET2      | MiFID II    |
| **APAC** | APAC desk     | `EUR/USD:1.0850:1.0852`   | T+1 (JPY), T+2    | Local rules |

**Problem:** Each region needs a FAMILY of components that work together:

- Order (region-specific routing)
- Quote Parser (region-specific format)
- Settlement Calculator (region-specific rules)

**Solution:** Abstract Factory ensures all components from same region are used together!

---

## 🏗️ Pattern Structure

```
                    ┌─────────────────────────────┐
                    │    IFXTradingFactory        │
                    │    (Abstract Factory)       │
                    ├─────────────────────────────┤
                    │ + CreateOrder()             │
                    │ + CreateQuoteParser()       │
                    │ + CreateSettlementCalc()    │
                    └──────────────┬──────────────┘
                                   │
          ┌────────────────────────┼────────────────────────┐
          │                        │                        │
          ▼                        ▼                        ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ USFXFactory     │    │ EUFXFactory     │    │ APACFXFactory   │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ CreateOrder()   │    │ CreateOrder()   │    │ CreateOrder()   │
│ → USFXOrder     │    │ → EUFXOrder     │    │ → APACFXOrder   │
│                 │    │                 │    │                 │
│ CreateParser()  │    │ CreateParser()  │    │ CreateParser()  │
│ → USQuoteParser │    │ → EUQuoteParser │    │ → APACParser    │
│                 │    │                 │    │                 │
│ CreateSettCalc()│    │ CreateSettCalc()│    │ CreateSettCalc()│
│ → USSettlement  │    │ → EUSettlement  │    │ → APACSettlement│
└─────────────────┘    └─────────────────┘    └─────────────────┘
          │                        │                        │
          │     PRODUCT FAMILIES   │                        │
          ▼                        ▼                        ▼
    ┌───────────┐            ┌───────────┐            ┌───────────┐
    │ US Family │            │ EU Family │            │APAC Family│
    ├───────────┤            ├───────────┤            ├───────────┤
    │ USOrder   │            │ EUOrder   │            │ APACOrder │
    │ USParser  │            │ EUParser  │            │ APACParser│
    │ USSettlem │            │ EUSettlem │            │APACSettlem│
    └───────────┘            └───────────┘            └───────────┘
```

---

## 📝 Code Components

### 1. Product Interfaces (The Family)

```csharp
// Product A: FX Order
public interface IFXOrder
{
    string Region { get; }
    void Execute();
}

// Product B: Quote Parser
public interface IQuoteParser
{
    void ParseQuote(string rawQuote);
    decimal GetBidRate();
    decimal GetAskRate();
}

// Product C: Settlement Calculator
public interface ISettlementCalculator
{
    DateTime CalculateSettlementDate(DateTime tradeDate, string currencyPair);
    string GetSettlementRules();
}
```

### 2. Abstract Factory Interface

```csharp
public interface IFXTradingFactory
{
    IFXOrder CreateOrder(string currencyPair, decimal amount);
    IQuoteParser CreateQuoteParser();
    ISettlementCalculator CreateSettlementCalculator();
}
```

### 3. Concrete Factory (US Region)

```csharp
public class USFXTradingFactory : IFXTradingFactory
{
    public IFXOrder CreateOrder(string currencyPair, decimal amount)
        => new USFXOrder(currencyPair, amount);

    public IQuoteParser CreateQuoteParser()
        => new USQuoteParser();

    public ISettlementCalculator CreateSettlementCalculator()
        => new USSettlementCalculator();
}
```

### 4. Client Code (Region-Agnostic)

```csharp
public class FXTradingService
{
    private readonly IFXTradingFactory _factory;
    private readonly IQuoteParser _quoteParser;
    private readonly ISettlementCalculator _settlementCalculator;

    // Service works with ANY region - doesn't know concrete classes!
    public FXTradingService(IFXTradingFactory factory)
    {
        _factory = factory;
        _quoteParser = factory.CreateQuoteParser();
        _settlementCalculator = factory.CreateSettlementCalculator();
    }

    public void ProcessTrade(string currencyPair, decimal amount, string rawQuote)
    {
        // 1. Parse quote (region-specific format)
        _quoteParser.ParseQuote(rawQuote);

        // 2. Create and execute order (region-specific routing)
        var order = _factory.CreateOrder(currencyPair, amount);
        order.Execute();

        // 3. Calculate settlement (region-specific rules)
        _settlementCalculator.CalculateSettlementDate(DateTime.Today, currencyPair);
    }
}
```

### 5. Usage

```csharp
// US Region - gets US family of products
var usFactory = new USFXTradingFactory();
var usService = new FXTradingService(usFactory);
usService.ProcessTrade("EUR/USD", 1000000, "EUR/USD|1.0850|1.0852");

// EU Region - gets EU family of products
var euFactory = new EUFXTradingFactory();
var euService = new FXTradingService(euFactory);
euService.ProcessTrade("GBP/EUR", 500000, "GBP/EUR;0,8450;0,8455");
```

---

## 🔐 Open/Closed Principle (OCP)

### What is Open/Closed Principle?

> **"Software entities should be OPEN for extension, but CLOSED for modification."**

| Term                        | Meaning                                |
| --------------------------- | -------------------------------------- |
| **OPEN for extension**      | You can add new behavior/features      |
| **CLOSED for modification** | Without changing existing working code |

---

### How Abstract Factory Follows OCP

#### Scenario: Business says "Add LATAM Region (Latin America)"

**❌ WITHOUT Abstract Factory (Violates OCP):**

```csharp
// You'd have to modify EVERY service that handles regions

// OrderService.cs - MODIFIED ❌
if (region == "LATAM")
{
    order = new LATAMOrder(...);
    parser = new LATAMQuoteParser();
    settlement = new LATAMSettlementCalculator();
}

// TradeService.cs - MODIFIED ❌
// ReportService.cs - MODIFIED ❌
// ... 20 more files MODIFIED
```

**Problem:** Modifying existing tested code = Risk of bugs!

---

**✅ WITH Abstract Factory (Follows OCP):**

```csharp
// STEP 1: Add new product classes (EXTENSION - new files)
public class LATAMFXOrder : IFXOrder { /* ... */ }
public class LATAMQuoteParser : IQuoteParser { /* ... */ }
public class LATAMSettlementCalculator : ISettlementCalculator { /* ... */ }

// STEP 2: Add new factory (EXTENSION - new file)
public class LATAMFXTradingFactory : IFXTradingFactory
{
    public IFXOrder CreateOrder(...) => new LATAMFXOrder(...);
    public IQuoteParser CreateQuoteParser() => new LATAMQuoteParser();
    public ISettlementCalculator CreateSettlementCalculator() => new LATAMSettlementCalculator();
}

// STEP 3: Update factory provider (single point of change)
TradingRegion.LATAM => new LATAMFXTradingFactory(),

// STEP 4: Client code? ZERO CHANGES! ✅
// FXTradingService, OrderService, etc. - UNCHANGED
```

---

### Visual Summary of OCP

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENT CODE                          │
│        (FXTradingService, OrderService, etc.)           │
│                                                         │
│  var service = new FXTradingService(factory);           │
│                    │                                    │
│                    │  ← CLOSED (no changes needed)      │
└────────────────────┼────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│               ABSTRACT FACTORY                          │
│                                                         │
│   US    → USFXTradingFactory                            │
│   EU    → EUFXTradingFactory                            │
│   APAC  → APACFXTradingFactory                          │
│   LATAM → LATAMFXTradingFactory  ← OPEN (extend here)   │
└─────────────────────────────────────────────────────────┘
```

---

## 🎤 Interview Q&A

### Q1: What is Abstract Factory Pattern?

> **Answer:** "Abstract Factory provides an interface for creating FAMILIES of related objects. Unlike Simple Factory which creates one product type, Abstract Factory creates multiple related products that are designed to work together."

---

### Q2: How is it different from Simple Factory?

> **Answer:**
>
> | Simple Factory              | Abstract Factory                                                       |
> | --------------------------- | ---------------------------------------------------------------------- |
> | Creates ONE type of product | Creates FAMILY of related products                                     |
> | `CreateOrder()`             | `CreateOrder()`, `CreateQuoteParser()`, `CreateSettlementCalculator()` |
> | Products are independent    | Products must work together                                            |

---

### Q3: How does Abstract Factory follow Open/Closed Principle?

> **Answer:** "When business asked for LATAM region support, we created new LATAM product classes (LATAMOrder, LATAMParser, LATAMSettlement) and a new LATAMFXTradingFactory. The FXTradingService, OrderService, ReportService - none of that code changed. We extended the system by adding new classes without modifying existing tested code - that's OCP."

---

### Q4: Real scenario from your FX Trading project?

> **Answer:** "In our multi-region FX system, each region (US, EU, APAC) has different:
>
> - Order routing rules and compliance
> - Quote formats from liquidity providers
> - Settlement date calculations (T+1 vs T+2, different holidays)
>
> We use Abstract Factory so when a trade comes from EU region, we get the EU factory which gives us EU-compliant order, EU quote parser (handles comma decimal separator), and EU settlement calculator (TARGET2 holidays) - all guaranteed to work together correctly."

---

### Q5: When to use Abstract Factory vs Simple Factory?

> **Answer:**
>
> **Use Abstract Factory when:**
>
> 1. You need to create FAMILIES of related objects
> 2. Objects in the family must work together
> 3. You want to enforce consistency (all products from same family)
>
> **Use Simple Factory when:**
>
> 1. You need to create ONE type of product with variations
> 2. Products don't have dependencies on each other

---

## 🏦 FX Trading Use Cases for Abstract Factory

| Region    | Order        | Quote Parser     | Settlement               | Compliance  |
| --------- | ------------ | ---------------- | ------------------------ | ----------- |
| **US**    | USFXOrder    | USQuoteParser    | USSettlement (T+2, Fed)  | Fed rules   |
| **EU**    | EUFXOrder    | EUQuoteParser    | EUSettlement (TARGET2)   | MiFID II    |
| **APAC**  | APACFXOrder  | APACQuoteParser  | APACSettlement (T+1/T+2) | Local rules |
| **LATAM** | LATAMFXOrder | LATAMQuoteParser | LATAMSettlement          | Local rules |

### Other FX Use Cases:

| Scenario                | Abstract Factory Use                                                                       |
| ----------------------- | ------------------------------------------------------------------------------------------ |
| **Customer Tiers**      | Premium/Standard/Basic → Different spread calculators, rate sources, service levels        |
| **Liquidity Providers** | Reuters/Bloomberg/Internal → Different quote parsers, connection handlers, rate formatters |
| **Product Types**       | Spot/Forward/Swap → Different calculators, validators, booking handlers                    |

---

## ✅ Key Benefits

1. **Product Consistency** - All products from same family work together
2. **Loose Coupling** - Client doesn't know concrete classes
3. **Open/Closed Principle** - Add new families without changing client code
4. **Single Responsibility** - Each factory handles its region
5. **Easy Testing** - Can mock entire factory for unit tests

---

## 🚀 Run the Example

```bash
cd AbstractFactoryPattern
dotnet run
```

**Expected Output:**

```
=====================================================
  ABSTRACT FACTORY PATTERN - FX TRADING DEMO
=====================================================


>>> US REGION TRADING <<<

==================================================
Processing Trade: EUR/USD | Amount: 1,000,000.00
==================================================
[US PARSER] Parsed US format quote: Bid=1.0850, Ask=1.0852
Bid: 1.0850, Ask: 1.0852
[US ORDER] Executing EUR/USD for $1,000,000.00
>> Routed through US trading desk, Fed compliance applied.
[US SETTLEMENT] EUR/USD: T+2 = 2026-02-16
Settlement Rules: US T+2, Federal Reserve holidays, NY cut-off 5PM ET


>>> EU REGION TRADING <<<

==================================================
Processing Trade: GBP/EUR | Amount: 500,000.00
==================================================
[EU PARSER] Parsed EU format quote: Bid=0.8450, Ask=0.8455
Bid: 0.8450, Ask: 0.8455
[EU ORDER] Executing GBP/EUR for €500,000.00
>> Routed through EU trading desk, MiFID II compliance applied.
[EU SETTLEMENT] GBP/EUR: T+2 TARGET2 = 2026-02-16
Settlement Rules: EU T+2, TARGET2 holidays, London cut-off 4PM GMT


>>> APAC REGION TRADING <<<

==================================================
Processing Trade: USD/JPY | Amount: 2,000,000.00
==================================================
[APAC PARSER] Parsed APAC format quote: Bid=148.50, Ask=148.55
Bid: 148.50, Ask: 148.55
[APAC ORDER] Executing USD/JPY for ¥2,000,000.00
>> Routed through APAC trading desk, local compliance applied.
[APAC SETTLEMENT] USD/JPY: T+1 = 2026-02-13
Settlement Rules: APAC T+1 (JPY pairs), T+2 (others), Tokyo cut-off 3PM JST


=====================================================
  DEMO COMPLETE - All regions processed successfully!
=====================================================
```
