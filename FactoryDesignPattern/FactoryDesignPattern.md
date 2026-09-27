# Factory Design Pattern - FX Trading Example

## 📌 Interview Definition

> **"Factory Pattern creates objects without exposing the creation logic. The client uses the factory to get objects instead of using 'new' directly."**

---

## 🎯 Real-World FX Trading Scenario

In FX Trading, we deal with different **order types**:

- **Market Order** - Execute immediately at current rate
- **Limit Order** - Execute when rate hits target
- **Stop Order** - Execute to prevent losses (risk management)

| Without Factory                                    | With Factory                                                     |
| -------------------------------------------------- | ---------------------------------------------------------------- |
| Client code is tightly coupled to concrete classes | Client asks factory for order, doesn't care about creation logic |
| `new MarketOrder()` scattered everywhere           | `factory.CreateOrder(OrderType.Market, ...)`                     |
| Hard to maintain and extend                        | Easy to add new order types                                      |

---

## 🏗️ Pattern Structure

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENT CODE                          │
│  (OrderBookingService, TradeService, ReportService)     │
│                                                         │
│     var order = factory.CreateOrder(orderType, ...);    │
│                    │                                    │
└────────────────────┼────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   FACTORY                               │
│        FXOrderFactory.CreateOrder()                     │
│                     │                                   │
│    ┌────────────────┼────────────────┐                  │
│    ▼                ▼                ▼                  │
│ MarketOrder    LimitOrder       StopOrder               │
│    │                │                │                  │
│    └────────────────┴────────────────┘                  │
│                     │                                   │
│                     ▼                                   │
│               IFXOrder (Interface)                      │
└─────────────────────────────────────────────────────────┘
```

---

## 📝 Code Components

### 1. Interface (IFXOrder)

```csharp
public interface IFXOrder
{
    string OrderType { get; }
    string CurrencyPair { get; }
    decimal Amount { get; }
    decimal Rate { get; }
    void Execute();
    string GetOrderDetails();
}
```

### 2. Concrete Products

```csharp
public class MarketOrder : IFXOrder { /* ... */ }
public class LimitOrder : IFXOrder { /* ... */ }
public class StopOrder : IFXOrder { /* ... */ }
```

### 3. Factory

```csharp
public class FXOrderFactory
{
    public IFXOrder CreateOrder(OrderType orderType, string currencyPair, decimal amount, decimal rate)
    {
        return orderType switch
        {
            OrderType.Market => new MarketOrder(currencyPair, amount, rate),
            OrderType.Limit => new LimitOrder(currencyPair, amount, rate),
            OrderType.Stop => new StopOrder(currencyPair, amount, rate),
            _ => throw new ArgumentException($"Unknown order type: {orderType}")
        };
    }
}
```

### 4. Client Usage

```csharp
// Client doesn't do this (tight coupling):
IFXOrder order = new MarketOrder("EUR/USD", 1000000, 1.0850m);  ❌

// Client uses Factory (loose coupling):
IFXOrder order = factory.CreateOrder(OrderType.Market, "EUR/USD", 1000000, 1.0850m);  ✅
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

### How Factory Pattern Follows OCP

#### Scenario: Business says "Add TWI (Time-Weighted Order) for large trades"

**❌ WITHOUT Factory (Violates OCP):**

```csharp
// You'd have to modify EVERY place that creates orders

// OrderBookingService.cs - MODIFIED ❌
if (orderType == "TWI")
    order = new TWIOrder(...);

// TradeExecutionService.cs - MODIFIED ❌
if (orderType == "TWI")
    order = new TWIOrder(...);

// ReportingService.cs - MODIFIED ❌
// ... and 10 more files MODIFIED
```

**Problem:** You're modifying existing tested code = Risk of bugs!

---

**✅ WITH Factory (Follows OCP):**

```csharp
// STEP 1: Add new class (EXTENSION - new file, no existing code touched)
public class TWIOrder : IFXOrder
{
    public string OrderType => "TWI";
    // ... implementation
}

// STEP 2: Update Factory ONLY (single point of change)
public enum OrderType
{
    Market,
    Limit,
    Stop,
    TWI      // <-- Just add enum
}

// In Factory method - add one line:
OrderType.TWI => new TWIOrder(currencyPair, amount, rate),

// STEP 3: Client code? ZERO CHANGES! ✅
// OrderBookingService, TradeExecutionService, etc. - UNCHANGED
```

---

### Visual Summary of OCP

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENT CODE                          │
│  (OrderBookingService, TradeService, ReportService)     │
│                                                         │
│     var order = factory.CreateOrder(orderType, ...);    │
│                    │                                    │
│                    │  ← CLOSED (no changes needed)      │
└────────────────────┼────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   FACTORY                               │
│                                                         │
│   Market → MarketOrder                                  │
│   Limit  → LimitOrder                                   │
│   Stop   → StopOrder                                    │
│   TWI    → TWIOrder    ← OPEN (easy to extend here)     │
└─────────────────────────────────────────────────────────┘
```

---

## 🎤 Interview Q&A

### Q1: Why use Factory Pattern in FX Trading?

> **Answer:** "In our FX trading system:
>
> 1. We have multiple order types (Market, Limit, Stop, etc.)
> 2. Each order type has different creation logic
> 3. Factory centralizes this logic - easy to maintain
> 4. If we add new order type (e.g., OCO Order), we just add a new class and update factory - client code doesn't change"

---

### Q2: What problem does Factory Pattern solve?

> **Answer:**
>
> **Without Factory:**
>
> - Client code has `new MarketOrder()`, `new LimitOrder()` scattered everywhere
> - If constructor changes, we update 100 places
> - Tight coupling between client and concrete classes
>
> **With Factory:**
>
> - Client only knows `IFXOrder` interface and `OrderType` enum
> - Factory handles all creation logic
> - Easy to extend, easy to test, follows Open/Closed Principle

---

### Q3: How does Factory Pattern follow Open/Closed Principle?

> **Answer:** "Open/Closed means code should be open for extension but closed for modification. In our FX system, when business asked for new order types like TWI or OCO orders, we just created new classes implementing `IFXOrder` and added one line in the factory. The order booking service, trade execution, reporting - none of that code changed. We extended the system without modifying existing tested code - that's OCP."

---

### Q4: Real scenario from your FX Trading project?

> **Answer:** "In our rate streaming module, we receive quotes from multiple liquidity providers. Each provider has different quote format. We use Factory to create the appropriate `QuoteParser` based on provider type. The streaming service just asks factory for parser - doesn't care which concrete class."

---

## 🏦 FX Trading Use Cases for Factory Pattern

| Module                 | Factory Use Case                                                |
| ---------------------- | --------------------------------------------------------------- |
| **Order Booking**      | Create different order types (Market, Limit, Stop, TWI)         |
| **Rate Streaming**     | Create parsers for different liquidity providers                |
| **Customer Data**      | Create data fetchers for different external services            |
| **Spread Calculation** | Create spread calculators for different customer tiers          |
| **Date Calculation**   | Create date calculators for different currency pairs (T+1, T+2) |

---

## ✅ Key Benefits

1. **Loose Coupling** - Client doesn't know concrete classes
2. **Single Responsibility** - Factory handles object creation
3. **Open/Closed Principle** - Extend without modifying existing code
4. **Easy Testing** - Can mock the factory in unit tests
5. **Centralized Logic** - All creation logic in one place

---

## 🚀 Run the Example

```bash
cd FactoryDesignPattern
dotnet run
```

**Expected Output:**

```
========================================
  FX TRADING - FACTORY PATTERN DEMO
========================================

--- Scenario 1: Client wants immediate execution ---
[MARKET ORDER] Executing EUR/USD trade for 1,000,000.00 at rate 1.0850
>> Trade executed immediately at current market rate.

--- Scenario 2: Client wants to buy at better rate ---
[LIMIT ORDER] Placed GBP/USD order for 500,000.00 at target rate 1.2700
>> Order will execute when market rate reaches target.

--- Scenario 3: Client wants stop-loss protection ---
[STOP ORDER] Placed USD/JPY stop order for 2,000,000.00 at stop rate 148.50
>> Order will trigger if rate falls to stop level (risk protection).

========================================
  ORDER SUMMARY
========================================
Market Order: EUR/USD | Amount: 1,000,000.00 | Rate: 1.0850
Limit Order: GBP/USD | Amount: 500,000.00 | Target Rate: 1.2700
Stop Order: USD/JPY | Amount: 2,000,000.00 | Stop Rate: 148.50
```
