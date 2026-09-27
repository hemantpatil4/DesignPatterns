using System;

namespace FXTradingAbstractFactoryPattern
{
    // =====================================================
    // ABSTRACT FACTORY DESIGN PATTERN - FX TRADING EXAMPLE
    // =====================================================
    // 
    // INTERVIEW DEFINITION:
    // "Abstract Factory provides an interface for creating FAMILIES of related objects
    //  without specifying their concrete classes."
    //
    // KEY DIFFERENCE FROM SIMPLE FACTORY:
    // - Simple Factory: Creates ONE type of product (e.g., Orders)
    // - Abstract Factory: Creates FAMILY of related products (e.g., Order + Quote + Settlement)
    //
    // REAL-WORLD FX SCENARIO:
    // Different REGIONS have different trading rules:
    // - US Region: US orders, US quotes format, US settlement (T+2)
    // - EU Region: EU orders, EU quotes format, EU settlement (T+2 with TARGET2)
    // - APAC Region: APAC orders, APAC quotes, APAC settlement (varies by country)
    //
    // Each region needs a FAMILY of related components that work together.

    // ==========================================
    // STEP 1: Define Product Interfaces (Family)
    // ==========================================

    // Product A: FX Order
    public interface IFXOrder
    {
        string Region { get; }
        string CurrencyPair { get; }
        decimal Amount { get; }
        void Execute();
    }

    // Product B: Quote Parser (for rate streaming)
    public interface IQuoteParser
    {
        string Region { get; }
        void ParseQuote(string rawQuote);
        decimal GetBidRate();
        decimal GetAskRate();
    }

    // Product C: Settlement Calculator (for date calculation)
    public interface ISettlementCalculator
    {
        string Region { get; }
        DateTime CalculateSettlementDate(DateTime tradeDate, string currencyPair);
        string GetSettlementRules();
    }

    // ==========================================
    // STEP 2: Create Concrete Products - US Region
    // ==========================================

    public class USFXOrder : IFXOrder
    {
        public string Region => "US";
        public string CurrencyPair { get; private set; }
        public decimal Amount { get; private set; }

        public USFXOrder(string currencyPair, decimal amount)
        {
            CurrencyPair = currencyPair;
            Amount = amount;
        }

        public void Execute()
        {
            Console.WriteLine($"[US ORDER] Executing {CurrencyPair} for ${Amount:N2}");
            Console.WriteLine(">> Routed through US trading desk, Fed compliance applied.");
        }
    }

    public class USQuoteParser : IQuoteParser
    {
        public string Region => "US";
        private decimal _bid;
        private decimal _ask;

        public void ParseQuote(string rawQuote)
        {
            // US format: "EUR/USD|1.0850|1.0852"
            var parts = rawQuote.Split('|');
            _bid = decimal.Parse(parts[1]);
            _ask = decimal.Parse(parts[2]);
            Console.WriteLine($"[US PARSER] Parsed US format quote: Bid={_bid}, Ask={_ask}");
        }

        public decimal GetBidRate() => _bid;
        public decimal GetAskRate() => _ask;
    }

    public class USSettlementCalculator : ISettlementCalculator
    {
        public string Region => "US";

        public DateTime CalculateSettlementDate(DateTime tradeDate, string currencyPair)
        {
            // US: T+2, skip US holidays
            var settlement = tradeDate.AddDays(2);
            // Skip weekends (simplified)
            while (settlement.DayOfWeek == DayOfWeek.Saturday || 
                   settlement.DayOfWeek == DayOfWeek.Sunday)
            {
                settlement = settlement.AddDays(1);
            }
            Console.WriteLine($"[US SETTLEMENT] {currencyPair}: T+2 = {settlement:yyyy-MM-dd}");
            return settlement;
        }

        public string GetSettlementRules() => "US T+2, Federal Reserve holidays, NY cut-off 5PM ET";
    }

    // ==========================================
    // STEP 3: Create Concrete Products - EU Region
    // ==========================================

    public class EUFXOrder : IFXOrder
    {
        public string Region => "EU";
        public string CurrencyPair { get; private set; }
        public decimal Amount { get; private set; }

        public EUFXOrder(string currencyPair, decimal amount)
        {
            CurrencyPair = currencyPair;
            Amount = amount;
        }

        public void Execute()
        {
            Console.WriteLine($"[EU ORDER] Executing {CurrencyPair} for €{Amount:N2}");
            Console.WriteLine(">> Routed through EU trading desk, MiFID II compliance applied.");
        }
    }

    public class EUQuoteParser : IQuoteParser
    {
        public string Region => "EU";
        private decimal _bid;
        private decimal _ask;

        public void ParseQuote(string rawQuote)
        {
            // EU format: "EUR/USD;1,0850;1,0852" (comma as decimal separator)
            var parts = rawQuote.Split(';');
            _bid = decimal.Parse(parts[1].Replace(',', '.'));
            _ask = decimal.Parse(parts[2].Replace(',', '.'));
            Console.WriteLine($"[EU PARSER] Parsed EU format quote: Bid={_bid}, Ask={_ask}");
        }

        public decimal GetBidRate() => _bid;
        public decimal GetAskRate() => _ask;
    }

    public class EUSettlementCalculator : ISettlementCalculator
    {
        public string Region => "EU";

        public DateTime CalculateSettlementDate(DateTime tradeDate, string currencyPair)
        {
            // EU: T+2, skip TARGET2 holidays
            var settlement = tradeDate.AddDays(2);
            while (settlement.DayOfWeek == DayOfWeek.Saturday || 
                   settlement.DayOfWeek == DayOfWeek.Sunday)
            {
                settlement = settlement.AddDays(1);
            }
            Console.WriteLine($"[EU SETTLEMENT] {currencyPair}: T+2 TARGET2 = {settlement:yyyy-MM-dd}");
            return settlement;
        }

        public string GetSettlementRules() => "EU T+2, TARGET2 holidays, London cut-off 4PM GMT";
    }

    // ==========================================
    // STEP 4: Create Concrete Products - APAC Region
    // ==========================================

    public class APACFXOrder : IFXOrder
    {
        public string Region => "APAC";
        public string CurrencyPair { get; private set; }
        public decimal Amount { get; private set; }

        public APACFXOrder(string currencyPair, decimal amount)
        {
            CurrencyPair = currencyPair;
            Amount = amount;
        }

        public void Execute()
        {
            Console.WriteLine($"[APAC ORDER] Executing {CurrencyPair} for ¥{Amount:N2}");
            Console.WriteLine(">> Routed through APAC trading desk, local compliance applied.");
        }
    }

    public class APACQuoteParser : IQuoteParser
    {
        public string Region => "APAC";
        private decimal _bid;
        private decimal _ask;

        public void ParseQuote(string rawQuote)
        {
            // APAC format: "EUR/USD:1.0850:1.0852"
            var parts = rawQuote.Split(':');
            _bid = decimal.Parse(parts[1]);
            _ask = decimal.Parse(parts[2]);
            Console.WriteLine($"[APAC PARSER] Parsed APAC format quote: Bid={_bid}, Ask={_ask}");
        }

        public decimal GetBidRate() => _bid;
        public decimal GetAskRate() => _ask;
    }

    public class APACSettlementCalculator : ISettlementCalculator
    {
        public string Region => "APAC";

        public DateTime CalculateSettlementDate(DateTime tradeDate, string currencyPair)
        {
            // APAC: T+1 for some pairs (JPY), T+2 for others
            int days = currencyPair.Contains("JPY") ? 1 : 2;
            var settlement = tradeDate.AddDays(days);
            while (settlement.DayOfWeek == DayOfWeek.Saturday || 
                   settlement.DayOfWeek == DayOfWeek.Sunday)
            {
                settlement = settlement.AddDays(1);
            }
            Console.WriteLine($"[APAC SETTLEMENT] {currencyPair}: T+{days} = {settlement:yyyy-MM-dd}");
            return settlement;
        }

        public string GetSettlementRules() => "APAC T+1 (JPY pairs), T+2 (others), Tokyo cut-off 3PM JST";
    }

    // ==========================================
    // STEP 5: Abstract Factory Interface
    // ==========================================

    public interface IFXTradingFactory
    {
        IFXOrder CreateOrder(string currencyPair, decimal amount);
        IQuoteParser CreateQuoteParser();
        ISettlementCalculator CreateSettlementCalculator();
    }

    // ==========================================
    // STEP 6: Concrete Factories
    // ==========================================

    public class USFXTradingFactory : IFXTradingFactory
    {
        public IFXOrder CreateOrder(string currencyPair, decimal amount)
            => new USFXOrder(currencyPair, amount);

        public IQuoteParser CreateQuoteParser()
            => new USQuoteParser();

        public ISettlementCalculator CreateSettlementCalculator()
            => new USSettlementCalculator();
    }

    public class EUFXTradingFactory : IFXTradingFactory
    {
        public IFXOrder CreateOrder(string currencyPair, decimal amount)
            => new EUFXOrder(currencyPair, amount);

        public IQuoteParser CreateQuoteParser()
            => new EUQuoteParser();

        public ISettlementCalculator CreateSettlementCalculator()
            => new EUSettlementCalculator();
    }

    public class APACFXTradingFactory : IFXTradingFactory
    {
        public IFXOrder CreateOrder(string currencyPair, decimal amount)
            => new APACFXOrder(currencyPair, amount);

        public IQuoteParser CreateQuoteParser()
            => new APACQuoteParser();

        public ISettlementCalculator CreateSettlementCalculator()
            => new APACSettlementCalculator();
    }

    // ==========================================
    // STEP 7: Factory Provider (Optional Helper)
    // ==========================================

    public enum TradingRegion
    {
        US,
        EU,
        APAC
    }

    public static class FXTradingFactoryProvider
    {
        public static IFXTradingFactory GetFactory(TradingRegion region)
        {
            return region switch
            {
                TradingRegion.US => new USFXTradingFactory(),
                TradingRegion.EU => new EUFXTradingFactory(),
                TradingRegion.APAC => new APACFXTradingFactory(),
                _ => throw new ArgumentException($"Unknown region: {region}")
            };
        }
    }

    // ==========================================
    // STEP 8: Client Code (Trading Service)
    // ==========================================

    // This service works with ANY region - doesn't know concrete classes
    public class FXTradingService
    {
        private readonly IFXTradingFactory _factory;
        private readonly IQuoteParser _quoteParser;
        private readonly ISettlementCalculator _settlementCalculator;

        public FXTradingService(IFXTradingFactory factory)
        {
            _factory = factory;
            _quoteParser = factory.CreateQuoteParser();
            _settlementCalculator = factory.CreateSettlementCalculator();
        }

        public void ProcessTrade(string currencyPair, decimal amount, string rawQuote)
        {
            Console.WriteLine($"\n{'=',-50}");
            Console.WriteLine($"Processing Trade: {currencyPair} | Amount: {amount:N2}");
            Console.WriteLine($"{'=',-50}");

            // 1. Parse the quote (region-specific format)
            _quoteParser.ParseQuote(rawQuote);
            Console.WriteLine($"Bid: {_quoteParser.GetBidRate()}, Ask: {_quoteParser.GetAskRate()}");

            // 2. Create and execute order (region-specific routing)
            var order = _factory.CreateOrder(currencyPair, amount);
            order.Execute();

            // 3. Calculate settlement date (region-specific rules)
            var settlementDate = _settlementCalculator.CalculateSettlementDate(DateTime.Today, currencyPair);
            Console.WriteLine($"Settlement Rules: {_settlementCalculator.GetSettlementRules()}");
        }
    }

    // ==========================================
    // STEP 9: Main Program
    // ==========================================

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=====================================================");
            Console.WriteLine("  ABSTRACT FACTORY PATTERN - FX TRADING DEMO");
            Console.WriteLine("=====================================================");

            // ---- SCENARIO 1: US Client Trading ----
            Console.WriteLine("\n\n>>> US REGION TRADING <<<");
            var usFactory = FXTradingFactoryProvider.GetFactory(TradingRegion.US);
            var usService = new FXTradingService(usFactory);
            usService.ProcessTrade("EUR/USD", 1000000, "EUR/USD|1.0850|1.0852");

            // ---- SCENARIO 2: EU Client Trading ----
            Console.WriteLine("\n\n>>> EU REGION TRADING <<<");
            var euFactory = FXTradingFactoryProvider.GetFactory(TradingRegion.EU);
            var euService = new FXTradingService(euFactory);
            euService.ProcessTrade("GBP/EUR", 500000, "GBP/EUR;0,8450;0,8455");

            // ---- SCENARIO 3: APAC Client Trading ----
            Console.WriteLine("\n\n>>> APAC REGION TRADING <<<");
            var apacFactory = FXTradingFactoryProvider.GetFactory(TradingRegion.APAC);
            var apacService = new FXTradingService(apacFactory);
            apacService.ProcessTrade("USD/JPY", 2000000, "USD/JPY:148.50:148.55");

            Console.WriteLine("\n\n=====================================================");
            Console.WriteLine("  DEMO COMPLETE - All regions processed successfully!");
            Console.WriteLine("=====================================================");
        }
    }
}

// ==========================================
// INTERVIEW TALKING POINTS
// ==========================================
/*
Q: What is Abstract Factory Pattern?

A: "Abstract Factory provides an interface for creating FAMILIES of related objects.
    Unlike Simple Factory which creates one product type, Abstract Factory creates
    multiple related products that work together."

Q: How is it different from Simple Factory?

A: Simple Factory:
   - Creates ONE type of product (e.g., different Orders)
   - One factory method: CreateOrder()

   Abstract Factory:
   - Creates FAMILY of related products (Order + QuoteParser + SettlementCalculator)
   - Multiple factory methods: CreateOrder(), CreateQuoteParser(), CreateSettlementCalculator()
   - Products in a family are designed to work together

Q: Real scenario from your FX Trading project?

A: "In our multi-region FX system, each region (US, EU, APAC) has different:
    - Order routing rules
    - Quote formats from liquidity providers
    - Settlement date calculations
    - Compliance requirements
    
    We use Abstract Factory so when a trade comes from EU region, we get the
    EU factory which gives us EU-compliant order, EU quote parser, and EU
    settlement calculator - all guaranteed to work together correctly."

Q: When to use Abstract Factory vs Simple Factory?

A: Use Abstract Factory when:
   1. You need to create FAMILIES of related objects
   2. Objects in the family must work together
   3. You want to enforce that products from same family are used together
   
   Use Simple Factory when:
   1. You need to create ONE type of product with variations
   2. Products don't have dependencies on each other
*/
