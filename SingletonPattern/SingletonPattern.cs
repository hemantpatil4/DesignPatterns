using System;
using System.Collections.Generic;

namespace FXTradingSingletonPattern
{
    // =====================================================
    // SINGLETON DESIGN PATTERN - FX TRADING EXAMPLE
    // =====================================================
    // 
    // INTERVIEW DEFINITION:
    // "Singleton ensures a class has only ONE instance and provides 
    //  a global point of access to it."
    //
    // REAL-WORLD FX SCENARIO:
    // In FX Trading, certain components MUST be single instance:
    // - Rate Cache: One source of truth for live rates
    // - Configuration Manager: Single config across the app
    // - Connection Pool: Single pool to manage DB/API connections
    // - Audit Logger: Single logger for compliance/audit trail
    //
    // Why? Multiple instances would cause:
    // - Inconsistent rates (different caches showing different prices)
    // - Resource waste (multiple connection pools)
    // - Data inconsistency (multiple configs)

    // ==========================================
    // EXAMPLE 1: Basic Singleton (NOT Thread-Safe)
    // ==========================================
    // DON'T USE IN PRODUCTION - Just for understanding

    public class BasicRateCache
    {
        private static BasicRateCache _instance;
        private Dictionary<string, decimal> _rates;

        // Private constructor - prevents external instantiation
        private BasicRateCache()
        {
            _rates = new Dictionary<string, decimal>();
            Console.WriteLine("[BasicRateCache] Instance created");
        }

        public static BasicRateCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BasicRateCache();
                }
                return _instance;
            }
        }

        public void UpdateRate(string currencyPair, decimal rate)
        {
            _rates[currencyPair] = rate;
        }

        public decimal GetRate(string currencyPair)
        {
            return _rates.TryGetValue(currencyPair, out var rate) ? rate : 0;
        }
    }

    // ==========================================
    // EXAMPLE 2: Thread-Safe Singleton (Lock)
    // ==========================================
    // Good for most scenarios

    public class ThreadSafeRateCache
    {
        private static ThreadSafeRateCache _instance;
        private static readonly object _lock = new object();
        private Dictionary<string, decimal> _rates;

        private ThreadSafeRateCache()
        {
            _rates = new Dictionary<string, decimal>();
            Console.WriteLine("[ThreadSafeRateCache] Instance created");
        }

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

        public void UpdateRate(string currencyPair, decimal rate)
        {
            lock (_lock)
            {
                _rates[currencyPair] = rate;
            }
        }

        public decimal GetRate(string currencyPair)
        {
            lock (_lock)
            {
                return _rates.TryGetValue(currencyPair, out var rate) ? rate : 0;
            }
        }
    }

    // ==========================================
    // EXAMPLE 3: Lazy<T> Singleton (RECOMMENDED)
    // ==========================================
    // Best practice - Thread-safe and lazy initialization

    public sealed class FXRateCache
    {
        // Lazy<T> ensures thread-safe lazy initialization
        private static readonly Lazy<FXRateCache> _lazyInstance =
            new Lazy<FXRateCache>(() => new FXRateCache());

        private readonly Dictionary<string, RateInfo> _rates;
        private readonly object _lock = new object();

        // Private constructor
        private FXRateCache()
        {
            _rates = new Dictionary<string, RateInfo>();
            Console.WriteLine("[FXRateCache] Singleton instance created");
            LoadInitialRates();
        }

        // Public accessor
        public static FXRateCache Instance => _lazyInstance.Value;

        private void LoadInitialRates()
        {
            // Simulate loading rates from liquidity provider
            _rates["EUR/USD"] = new RateInfo("EUR/USD", 1.0850m, 1.0852m, DateTime.Now);
            _rates["GBP/USD"] = new RateInfo("GBP/USD", 1.2650m, 1.2653m, DateTime.Now);
            _rates["USD/JPY"] = new RateInfo("USD/JPY", 148.50m, 148.53m, DateTime.Now);
            Console.WriteLine("[FXRateCache] Initial rates loaded");
        }

        public void UpdateRate(string currencyPair, decimal bid, decimal ask)
        {
            lock (_lock)
            {
                _rates[currencyPair] = new RateInfo(currencyPair, bid, ask, DateTime.Now);
                Console.WriteLine($"[FXRateCache] Updated {currencyPair}: Bid={bid}, Ask={ask}");
            }
        }

        public RateInfo GetRate(string currencyPair)
        {
            lock (_lock)
            {
                if (_rates.TryGetValue(currencyPair, out var rate))
                {
                    return rate;
                }
                throw new KeyNotFoundException($"Rate not found for {currencyPair}");
            }
        }

        public IEnumerable<RateInfo> GetAllRates()
        {
            lock (_lock)
            {
                return new List<RateInfo>(_rates.Values);
            }
        }

        public int RateCount => _rates.Count;
    }

    public class RateInfo
    {
        public string CurrencyPair { get; }
        public decimal Bid { get; }
        public decimal Ask { get; }
        public decimal Spread => Ask - Bid;
        public DateTime LastUpdated { get; }

        public RateInfo(string currencyPair, decimal bid, decimal ask, DateTime lastUpdated)
        {
            CurrencyPair = currencyPair;
            Bid = bid;
            Ask = ask;
            LastUpdated = lastUpdated;
        }

        public override string ToString()
        {
            return $"{CurrencyPair}: Bid={Bid}, Ask={Ask}, Spread={Spread:F4}";
        }
    }

    // ==========================================
    // EXAMPLE 4: FX Configuration Manager Singleton
    // ==========================================

    public sealed class FXConfigurationManager
    {
        private static readonly Lazy<FXConfigurationManager> _lazyInstance =
            new Lazy<FXConfigurationManager>(() => new FXConfigurationManager());

        private readonly Dictionary<string, object> _settings;

        private FXConfigurationManager()
        {
            _settings = new Dictionary<string, object>();
            Console.WriteLine("[FXConfigurationManager] Singleton instance created");
            LoadConfiguration();
        }

        public static FXConfigurationManager Instance => _lazyInstance.Value;

        private void LoadConfiguration()
        {
            // Simulate loading from config file/database
            _settings["DefaultSpread"] = 0.0002m;
            _settings["MaxOrderAmount"] = 10000000m;
            _settings["SettlementDays"] = 2;
            _settings["TradingHoursStart"] = "08:00";
            _settings["TradingHoursEnd"] = "17:00";
            _settings["BaseCurrency"] = "USD";
            Console.WriteLine("[FXConfigurationManager] Configuration loaded");
        }

        public T GetSetting<T>(string key)
        {
            if (_settings.TryGetValue(key, out var value))
            {
                return (T)value;
            }
            throw new KeyNotFoundException($"Setting not found: {key}");
        }

        public void UpdateSetting(string key, object value)
        {
            _settings[key] = value;
            Console.WriteLine($"[FXConfigurationManager] Updated {key} = {value}");
        }
    }

    // ==========================================
    // EXAMPLE 5: Audit Logger Singleton
    // ==========================================

    public sealed class FXAuditLogger
    {
        private static readonly Lazy<FXAuditLogger> _lazyInstance =
            new Lazy<FXAuditLogger>(() => new FXAuditLogger());

        private readonly List<AuditEntry> _auditLog;
        private readonly object _lock = new object();

        private FXAuditLogger()
        {
            _auditLog = new List<AuditEntry>();
            Console.WriteLine("[FXAuditLogger] Singleton instance created");
        }

        public static FXAuditLogger Instance => _lazyInstance.Value;

        public void LogTrade(string tradeId, string currencyPair, decimal amount, string action)
        {
            lock (_lock)
            {
                var entry = new AuditEntry(tradeId, currencyPair, amount, action);
                _auditLog.Add(entry);
                Console.WriteLine($"[AUDIT] {entry}");
            }
        }

        public void LogRateUpdate(string currencyPair, decimal oldRate, decimal newRate)
        {
            lock (_lock)
            {
                var entry = new AuditEntry("RATE", currencyPair, newRate,
                    $"Rate updated from {oldRate} to {newRate}");
                _auditLog.Add(entry);
            }
        }

        public IEnumerable<AuditEntry> GetAuditTrail()
        {
            lock (_lock)
            {
                return new List<AuditEntry>(_auditLog);
            }
        }
    }

    public class AuditEntry
    {
        public string TradeId { get; }
        public string CurrencyPair { get; }
        public decimal Amount { get; }
        public string Action { get; }
        public DateTime Timestamp { get; }

        public AuditEntry(string tradeId, string currencyPair, decimal amount, string action)
        {
            TradeId = tradeId;
            CurrencyPair = currencyPair;
            Amount = amount;
            Action = action;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {TradeId} | {CurrencyPair} | {Amount:N2} | {Action}";
        }
    }

    // ==========================================
    // CLIENT CODE: FX Trading Service using Singletons
    // ==========================================

    public class FXTradingService
    {
        public void ExecuteTrade(string tradeId, string currencyPair, decimal amount, string side)
        {
            Console.WriteLine($"\n{'=',-60}");
            Console.WriteLine($"Executing Trade: {tradeId}");
            Console.WriteLine($"{'=',-60}");

            // Get rate from singleton cache
            var rateCache = FXRateCache.Instance;
            var rate = rateCache.GetRate(currencyPair);

            // Get spread from singleton config
            var config = FXConfigurationManager.Instance;
            var defaultSpread = config.GetSetting<decimal>("DefaultSpread");

            // Calculate execution rate
            decimal executionRate = side == "BUY" ? rate.Ask : rate.Bid;
            decimal totalAmount = amount * executionRate;

            Console.WriteLine($"Currency Pair: {currencyPair}");
            Console.WriteLine($"Side: {side}");
            Console.WriteLine($"Amount: {amount:N2}");
            Console.WriteLine($"Rate: {executionRate}");
            Console.WriteLine($"Total: {totalAmount:N2}");

            // Log to singleton audit logger
            var logger = FXAuditLogger.Instance;
            logger.LogTrade(tradeId, currencyPair, amount, $"{side} executed at {executionRate}");

            Console.WriteLine(">> Trade executed successfully!");
        }
    }

    // ==========================================
    // Rate Streaming Service (Updates Singleton Cache)
    // ==========================================

    public class RateStreamingService
    {
        public void SimulateRateUpdate(string currencyPair, decimal bid, decimal ask)
        {
            // Both services use the SAME singleton instance
            var rateCache = FXRateCache.Instance;
            var oldRate = rateCache.GetRate(currencyPair);

            rateCache.UpdateRate(currencyPair, bid, ask);

            // Log rate change
            FXAuditLogger.Instance.LogRateUpdate(currencyPair, oldRate.Bid, bid);
        }
    }

    // ==========================================
    // MAIN PROGRAM
    // ==========================================

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=====================================================");
            Console.WriteLine("  SINGLETON PATTERN - FX TRADING DEMO");
            Console.WriteLine("=====================================================\n");

            // ---- DEMO 1: Singleton Instance Verification ----
            Console.WriteLine("--- Demo 1: Verifying Singleton Instance ---\n");

            var cache1 = FXRateCache.Instance;
            var cache2 = FXRateCache.Instance;

            Console.WriteLine($"cache1 == cache2: {ReferenceEquals(cache1, cache2)}");
            Console.WriteLine($"Same instance? {(ReferenceEquals(cache1, cache2) ? "YES ✓" : "NO ✗")}\n");

            // ---- DEMO 2: Rate Streaming Updates Singleton ----
            Console.WriteLine("--- Demo 2: Rate Streaming Service ---\n");

            var streamingService = new RateStreamingService();
            streamingService.SimulateRateUpdate("EUR/USD", 1.0855m, 1.0858m);

            // ---- DEMO 3: Trading Service Uses Same Singleton ----
            Console.WriteLine("\n--- Demo 3: Trading Service ---");

            var tradingService = new FXTradingService();
            tradingService.ExecuteTrade("TRD001", "EUR/USD", 1000000, "BUY");
            tradingService.ExecuteTrade("TRD002", "GBP/USD", 500000, "SELL");

            // ---- DEMO 4: All Services Share Same Data ----
            Console.WriteLine("\n--- Demo 4: Shared State Verification ---\n");

            Console.WriteLine("All rates in cache:");
            foreach (var rate in FXRateCache.Instance.GetAllRates())
            {
                Console.WriteLine($"  {rate}");
            }

            Console.WriteLine("\nAudit Trail:");
            foreach (var entry in FXAuditLogger.Instance.GetAuditTrail())
            {
                Console.WriteLine($"  {entry}");
            }

            Console.WriteLine("\n=====================================================");
            Console.WriteLine("  DEMO COMPLETE!");
            Console.WriteLine("=====================================================");
        }
    }
}

// ==========================================
// INTERVIEW TALKING POINTS
// ==========================================
/*
Q: What is Singleton Pattern?

A: "Singleton ensures a class has only ONE instance throughout the application
    and provides a global access point to that instance."

Q: Why use Singleton in FX Trading?

A: "In our FX system:
    1. Rate Cache - All services must see the SAME live rates
    2. Configuration - Single source of truth for trading parameters
    3. Audit Logger - Single compliance log for all trades
    4. Connection Pool - Efficient resource management
    
    Multiple instances would cause rate inconsistency, resource waste, 
    and compliance issues."

Q: How do you make Singleton thread-safe?

A: Three approaches:
   1. Lock with double-check (good)
   2. Lazy<T> (best - recommended in C#)
   3. Static initialization (simple but less flexible)
   
   "In our system, we use Lazy<T> because it's thread-safe by default
    and only creates the instance when first accessed."

Q: What are the drawbacks of Singleton?

A: 1. Hard to unit test (global state)
   2. Hidden dependencies
   3. Can violate Single Responsibility
   4. Not suitable when you need multiple instances
   
   "We mitigate testing issues by using interfaces and dependency injection
    where the singleton is injected rather than directly accessed."
*/
