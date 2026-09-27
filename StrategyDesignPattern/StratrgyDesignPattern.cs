using System;
using StrategyNameSpace;

namespace StrategyDesignPatternNameSpace
{
    public class Vehicle
    {
        private Strategy injectedStrategy;
        public Vehicle(Strategy s)
        {
            this.injectedStrategy = s;
        }

        public void Drive()
        {
            injectedStrategy.Drive();
        }

    }

    public class NormalVehicle : Vehicle
    {
        public NormalVehicle():base(new NormalStrategy())
        {
            
        }
    }

    public class SportsVehicle : Vehicle
    {
        public SportsVehicle():base(new SportsStrategy())
        {
            
        }
    }

    public class Program
    {
        public static void Main()
        {
            SportsVehicle SportsCar = new SportsVehicle();
            SportsCar.Drive();

            NormalVehicle NormalCar = new NormalVehicle();
            NormalCar.Drive();
        }
    }
}


namespace StrategyNameSpace
{
    public interface Strategy
    {
        public void Drive();
    }

    public class NormalStrategy : Strategy
    {
        public void Drive()
        {
            Console.WriteLine("Normal Strategy Injected");
        }

    }
    public class SportsStrategy : Strategy
    {
        public void Drive()
        {
            Console.WriteLine("Sports Strategy Injected");
        }

    }
    public class OffroadStrategy : Strategy
    {
        public void Drive()
        {
            Console.WriteLine("Offroad Strategy Injected");
        }
        
    }
}