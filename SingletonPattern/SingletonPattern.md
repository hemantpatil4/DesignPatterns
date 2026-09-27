# Singleton Design Pattern - FX Trading Example

## 📌 Interview Definition

> **"Singleton ensures a class has only ONE instance throughout the application and provides a global access point to that instance."**

---

## 🎯 Real-World FX Trading Scenario

In FX Trading, certain components **MUST** be single instance:

| Component                 | Why Singleton?                            |
| ------------------------- | ----------------------------------------- |
| **Rate Cache**            | All services must see SAME live rates     |
| **Configuration Manager** | Single source of truth for trading params |
| **Audit Logger**          | Single compliance log for all trades      |
| **Connection Pool**       | Efficient resource management             |

### What happens WITHOUT Singleton?

```
❌ Multiple Rate Caches:
   - Trading Service sees EUR/USD = 1.0850
   - Risk Service sees EUR/USD = 1.0855
   - Customer gets wrong price!

❌ Multiple Config Managers:
   - Different spread values across services
   - Inconsistent trading rules

❌ Multiple Loggers:
   - Split audit trail
   - Compliance nightmare!
```

---

## 🏗️ Pattern Structure

```
┌─────────────────────────────────────────────────────────┐
│                    SINGLETON CLASS                       │
├─────────────────────────────────────────────────────────┤
│ - static instance: Singleton                            │
│ - private Constructor()                                 │
├─────────────────────────────────────────────────────────┤
│ + static Instance { get; }  ← Global access point       │
│ + BusinessMethods()                                     │
└─────────────────────────────────────────────────────────┘
                         │
                         │ Same instance
         ┌───────────────┼───────────────┐
         │               │               │
         ▼               ▼               ▼
   ┌──────────┐   ┌──────────┐   ┌──────────┐
   │ Service A│   │ Service B│   │ Service C│
   │ Trading  │   │ Risk     │   │ Reporting│
   └──────────┘   └──────────┘   └──────────┘
```

---

## 📝 Code Implementations

### 1. Basic Singleton (NOT Thread-Safe) ❌

```csharp
// DON'T USE IN PRODUCTION - Just for understanding
public class BasicRateCache
{
    private static BasicRateCache _instance;

    // Private constructor - prevents external instantiation
    private BasicRateCache() { }

    public static BasicRateCache Instance
    {
        get
        {
            if (_instance == null)  // ❌ Race condition here!
            {
                _instance = new BasicRateCache();
            }
            return _instance;
        }
    }
}
```

**Problem:** Two threads can both see `_instance == null` and create two instances!

---

### 2. Thread-Safe Singleton (Lock) ✓

```csharp
public class ThreadSafeRateCache
{
    private static ThreadSafeRateCache _instance;
    private static readonly object _lock = new object();

    private ThreadSafeRateCache() { }

    public static ThreadSafeRateCache Instance
    {
        get
        {
            // Double-check locking pattern
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ThreadSafeRateCache();
                    }
                }
            }
            return _instance;
        }
    }
}
```

**Why double-check?** First check avoids lock overhead, second check ensures thread-safety.

---

### 3. Lazy<T> Singleton (RECOMMENDED) ✅

```csharp
public sealed class FXRateCache
{
    // Lazy<T> ensures thread-safe lazy initialization
    private static readonly Lazy<FXRateCache> _lazyInstance =
        new Lazy<FXRateCache>(() => new FXRateCache());

    private readonly Dictionary<string, RateInfo> _rates;

    // Private constructor
    private FXRateCache()
    {
        _rates = new Dictionary<string, RateInfo>();
        LoadInitialRates();
    }

    // Public accessor
    public static FXRateCache Instance => _lazyInstance.Value;

    public void UpdateRate(string currencyPair, decimal bid, decimal ask)
    {
        _rates[currencyPair] = new RateInfo(currencyPair, bid, ask, DateTime.Now);
    }

    public RateInfo GetRate(string currencyPair)
    {
        return _rates[currencyPair];
    }
}
```

**Why Lazy<T>?**

- Thread-safe by default
- Lazy initialization (created only when first accessed)
- Clean, simple code
- No manual locking needed for creation

---

## 🔐 Open/Closed Principle (OCP)

### What is Open/Closed Principle?

> **"Software entities should be OPEN for extension, but CLOSED for modification."**

| Term                        | Meaning                                |
| --------------------------- | -------------------------------------- |
| **OPEN for extension**      | You can add new behavior/features      |
| **CLOSED for modification** | Without changing existing working code |

---

### How Singleton Follows OCP

#### Scenario: Add new rate source (Bloomberg) without changing existing code

**✅ WITH Proper Singleton Design:**

```csharp
// Original Singleton - UNCHANGED ✅
public sealed class FXRateCache
{
    public static FXRateCache Instance => _lazyInstance.Value;

    public void UpdateRate(string currencyPair, decimal bid, decimal ask)
    {
        // ... existing logic
    }
}

// EXTENSION: New Bloomberg rate feeder - NEW FILE
public class BloombergRateFeeder
{
    public void FeedRates()
    {
        // Uses existing singleton, no modification needed
        FXRateCache.Instance.UpdateRate("EUR/USD", 1.0850m, 1.0852m);
    }
}

// EXTENSION: New Reuters rate feeder - NEW FILE
public class ReutersRateFeeder
{
    public void FeedRates()
    {
        // Uses existing singleton, no modification needed
        FXRateCache.Instance.UpdateRate("GBP/USD", 1.2650m, 1.2653m);
    }
}
```

**Result:** Added new rate sources without modifying the Singleton!

---

## 🎤 Interview Q&A

### Q1: What is Singleton Pattern?

> **Answer:** "Singleton ensures a class has only ONE instance throughout the application and provides a global access point. In FX trading, we use it for Rate Cache so all services see the same live rates."

---

### Q2: Why use Singleton in FX Trading?

> **Answer:** "In our FX system:
>
> 1. **Rate Cache** - All services must see SAME live rates from streaming
> 2. **Configuration** - Single source of truth for spread, limits, trading hours
> 3. **Audit Logger** - Single compliance log for regulatory requirements
> 4. **Connection Pool** - Efficient DB/API connection management
>
> Multiple instances would cause rate inconsistency - customer could get wrong price!"

---

### Q3: How do you make Singleton thread-safe?

> **Answer:** "Three approaches:
>
> 1. **Lock with double-check** - Good, but verbose
> 2. **Lazy<T>** - Best practice in C#, thread-safe by default
> 3. **Static initialization** - Simple but less flexible
>
> We use `Lazy<T>` because it's thread-safe, lazy (created on first access), and clean code."

---

### Q4: What are the drawbacks of Singleton?

> **Answer:**
>
> 1. **Hard to unit test** - Global state makes mocking difficult
> 2. **Hidden dependencies** - Classes secretly depend on singleton
> 3. **Can violate SRP** - Often accumulates too many responsibilities
> 4. **Not suitable for multiple instances** - By design, only one
>
> "We mitigate testing issues by using interfaces and DI - inject singleton rather than direct access."

---

### Q5: Real scenario from your FX Trading project?

> **Answer:** "Our Rate Streaming service receives thousands of rate updates per second from multiple liquidity providers. All updates go to a Singleton Rate Cache. When Trading Service, Risk Service, or Reporting Service need rates, they all access the same cache instance - guaranteed consistency. Without singleton, each service might have stale or different rates."

---

### Q6: Singleton vs Static Class?

> **Answer:**
>
> | Singleton                           | Static Class                |
> | ----------------------------------- | --------------------------- |
> | Can implement interfaces            | Cannot implement interfaces |
> | Can be passed as parameter          | Cannot be passed            |
> | Lazy initialization possible        | Initialized at startup      |
> | Can have state with instance fields | Only static fields          |
> | Can be subclassed (if not sealed)   | Cannot be inherited         |
>
> "We use Singleton for Rate Cache because we inject it via interface for testing."

---

## 🏦 FX Trading Use Cases for Singleton

| Singleton                  | Purpose            | Why Single Instance?         |
| -------------------------- | ------------------ | ---------------------------- |
| **FXRateCache**            | Store live rates   | All services need same rates |
| **FXConfigurationManager** | Trading parameters | Consistent rules across app  |
| **FXAuditLogger**          | Compliance logging | Single audit trail           |
| **ConnectionPool**         | DB connections     | Resource efficiency          |
| **SessionManager**         | User sessions      | Centralized session state    |

---

## ✅ Key Benefits

1. **Single Instance** - Guaranteed one instance across application
2. **Global Access** - Easy access from anywhere
3. **Lazy Initialization** - Created only when needed
4. **Thread-Safe** - With proper implementation (Lazy<T>)
5. **Resource Efficiency** - No duplicate resources

---

## ⚠️ When NOT to Use Singleton

1. When you need multiple instances
2. When global state makes testing difficult
3. When it violates Single Responsibility Principle
4. When dependencies should be explicit (use DI instead)

---

## 🚀 Run the Example

```bash
cd SingletonPattern
dotnet run
```

**Expected Output:**

```
=====================================================
  SINGLETON PATTERN - FX TRADING DEMO
=====================================================

--- Demo 1: Verifying Singleton Instance ---

[FXRateCache] Singleton instance created
[FXRateCache] Initial rates loaded
cache1 == cache2: True
Same instance? YES ✓

--- Demo 2: Rate Streaming Service ---

[FXRateCache] Updated EUR/USD: Bid=1.0855, Ask=1.0858

--- Demo 3: Trading Service ---

============================================================
Executing Trade: TRD001
============================================================
Currency Pair: EUR/USD
Side: BUY
Amount: 1,000,000.00
Rate: 1.0858
Total: 1,085,800.00
[AUDIT] [10:30:45] TRD001 | EUR/USD | 1,000,000.00 | BUY executed at 1.0858
>> Trade executed successfully!

============================================================
Executing Trade: TRD002
============================================================
Currency Pair: GBP/USD
Side: SELL
Amount: 500,000.00
Rate: 1.2650
Total: 632,500.00
[AUDIT] [10:30:45] TRD002 | GBP/USD | 500,000.00 | SELL executed at 1.2650
>> Trade executed successfully!

--- Demo 4: Shared State Verification ---

All rates in cache:
  EUR/USD: Bid=1.0855, Ask=1.0858, Spread=0.0003
  GBP/USD: Bid=1.2650, Ask=1.2653, Spread=0.0003
  USD/JPY: Bid=148.50, Ask=148.53, Spread=0.0300

Audit Trail:
  [10:30:45] RATE | EUR/USD | 1.0855 | Rate updated from 1.0850 to 1.0855
  [10:30:45] TRD001 | EUR/USD | 1,000,000.00 | BUY executed at 1.0858
  [10:30:45] TRD002 | GBP/USD | 500,000.00 | SELL executed at 1.2650

=====================================================
  DEMO COMPLETE!
=====================================================
```
