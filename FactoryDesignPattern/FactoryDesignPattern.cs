using System;

namespace FXTradingFactoryPattern
{
    // ============================================
    // FACTORY DESIGN PATTERN - FX TRADING EXAMPLE
    // ============================================
    // 
    // INTERVIEW DEFINITION:
    // "Factory Pattern creates objects without exposing the creation logic.
    //  The client uses the factory to get objects instead of using 'new' directly."
    //
    // REAL-WORLD FX SCENARIO:
    // In FX Trading, we deal with different order types:
    // - Market Order (execute immediately at current rate)
    // - Limit Order (execute when rate hits target)
    // - Stop Order (execute to prevent losses)
    //
    // Without Factory: Client code is tightly coupled to concrete classes
    // With Factory: Client asks factory for order, doesn't care about creation logic

    // ==========================================
    // STEP 1: Define the Product Interface
    // ==========================================
    public interface IFXOrder
    {
        string OrderType { get; }
        string CurrencyPair { get; }
        decimal Amount { get; }
        decimal Rate { get; }
        void Execute();
        string GetOrderDetails();
    }

    // ==========================================
    // STEP 2: Create Concrete Products
    // ==========================================

    // Market Order - Executes immediately at current market rate
    public class MarketOrder : IFXOrder
    {
        public string OrderType => "MARKET";
        public string CurrencyPair { get; private set; }
        public decimal Amount { get; private set; }
        public decimal Rate { get; private set; }

        public MarketOrder(string currencyPair, decimal amount, decimal currentRate)
        {
            CurrencyPair = currencyPair;
            Amount = amount;
            Rate = currentRate;
        }

        public void Execute()
        {
            Console.WriteLine($"[MARKET ORDER] Executing {CurrencyPair} trade for {Amount:N2} at rate {Rate}");
            Console.WriteLine(">> Trade executed immediately at current market rate.");
        }

        public string GetOrderDetails()
        {
            return $"Market Order: {CurrencyPair} | Amount: {Amount:N2} | Rate: {Rate}";
        }
    }

    // Limit Order - Executes when target rate is reached
    public class LimitOrder : IFXOrder
    {
        public string OrderType => "LIMIT";
        public string CurrencyPair { get; private set; }
        public decimal Amount { get; private set; }
        public decimal Rate { get; private set; }
        public decimal TargetRate { get; private set; }

        public LimitOrder(string currencyPair, decimal amount, decimal targetRate)
        {
            CurrencyPair = currencyPair;
            Amount = amount;
            Rate = targetRate;
            TargetRate = targetRate;
        }

        public void Execute()
        {
            Console.WriteLine($"[LIMIT ORDER] Placed {CurrencyPair} order for {Amount:N2} at target rate {TargetRate}");
            Console.WriteLine(">> Order will execute when market rate reaches target.");
        }

        public string GetOrderDetails()
        {
            return $"Limit Order: {CurrencyPair} | Amount: {Amount:N2} | Target Rate: {TargetRate}";
        }
    }

    // Stop Order - For risk management
    public class StopOrder : IFXOrder
    {
        public string OrderType => "STOP";
        public string CurrencyPair { get; private set; }
        public decimal Amount { get; private set; }
        public decimal Rate { get; private set; }
        public decimal StopRate { get; private set; }

        public StopOrder(string currencyPair, decimal amount, decimal stopRate)
        {
            CurrencyPair = currencyPair;
            Amount = amount;
            Rate = stopRate;
            StopRate = stopRate;
        }

        public void Execute()
        {
            Console.WriteLine($"[STOP ORDER] Placed {CurrencyPair} stop order for {Amount:N2} at stop rate {StopRate}");
            Console.WriteLine(">> Order will trigger if rate falls to stop level (risk protection).");
        }

        public string GetOrderDetails()
        {
            return $"Stop Order: {CurrencyPair} | Amount: {Amount:N2} | Stop Rate: {StopRate}";
        }
    }

    // ==========================================
    // STEP 3: Create the Factory
    // ==========================================
    public enum OrderType
    {
        Market,
        Limit,
        Stop
    }

    public class FXOrderFactory
    {
        // Simple Factory Method
        public IFXOrder CreateOrder(OrderType orderType, string currencyPair, decimal amount, decimal rate)
        {
            // Factory decides WHICH concrete class to instantiate
            // Client doesn't need to know about MarketOrder, LimitOrder, StopOrder classes
            
            return orderType switch
            {
                OrderType.Market => new MarketOrder(currencyPair, amount, rate),
                OrderType.Limit => new LimitOrder(currencyPair, amount, rate),
                OrderType.Stop => new StopOrder(currencyPair, amount, rate),
                _ => throw new ArgumentException($"Unknown order type: {orderType}")
            };
        }
    }

    // ==========================================
    // STEP 4: Client Code (Usage Example)
    // ==========================================
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  FX TRADING - FACTORY PATTERN DEMO");
            Console.WriteLine("========================================\n");

            // Create the factory
            var orderFactory = new FXOrderFactory();

            // ---- SCENARIO 1: Sales desk books a Market Order ----
            Console.WriteLine("--- Scenario 1: Client wants immediate execution ---");
            IFXOrder marketOrder = orderFactory.CreateOrder(
                OrderType.Market,
                "EUR/USD",
                1000000,   // 1 Million EUR
                1.0850m    // Current rate
            );
            marketOrder.Execute();
            Console.WriteLine();

            // ---- SCENARIO 2: Client places a Limit Order ----
            Console.WriteLine("--- Scenario 2: Client wants to buy at better rate ---");
            IFXOrder limitOrder = orderFactory.CreateOrder(
                OrderType.Limit,
                "GBP/USD",
                500000,    // 500K GBP
                1.2700m    // Target rate (better than current)
            );
            limitOrder.Execute();
            Console.WriteLine();

            // ---- SCENARIO 3: Risk management with Stop Order ----
            Console.WriteLine("--- Scenario 3: Client wants stop-loss protection ---");
            IFXOrder stopOrder = orderFactory.CreateOrder(
                OrderType.Stop,
                "USD/JPY",
                2000000,   // 2 Million USD
                148.50m    // Stop rate
            );
            stopOrder.Execute();
            Console.WriteLine();

            // Display all order details
            Console.WriteLine("========================================");
            Console.WriteLine("  ORDER SUMMARY");
            Console.WriteLine("========================================");
            Console.WriteLine(marketOrder.GetOrderDetails());
            Console.WriteLine(limitOrder.GetOrderDetails());
            Console.WriteLine(stopOrder.GetOrderDetails());
        }
    }
}

// ==========================================
// INTERVIEW TALKING POINTS
// ==========================================
/*
Q: Why use Factory Pattern in FX Trading?

A: In our FX trading system:
   1. We have multiple order types (Market, Limit, Stop, etc.)
   2. Each order type has different creation logic
   3. Factory centralizes this logic - easy to maintain
   4. If we add new order type (e.g., OCO Order), we just add a new class 
      and update factory - client code doesn't change

Q: What problem does it solve?

A: Without Factory:
   - Client code has "new MarketOrder()", "new LimitOrder()" scattered everywhere
   - If constructor changes, we update 100 places
   - Tight coupling between client and concrete classes

   With Factory:
   - Client only knows IFXOrder interface and OrderType enum
   - Factory handles all creation logic
   - Easy to extend, easy to test, follows Open/Closed Principle

Q: Real scenario from your project?

A: "In our rate streaming module, we receive quotes from multiple liquidity 
    providers. Each provider has different quote format. We use Factory to 
    create the appropriate QuoteParser based on provider type. The streaming 
    service just asks factory for parser - doesn't care which concrete class."
*/
