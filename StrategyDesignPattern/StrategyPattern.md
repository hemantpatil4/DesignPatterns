# Strategy Design Pattern in C#

## Overview

The Strategy Design Pattern is a behavioral design pattern that enables selecting an algorithm's behavior at runtime. It defines a family of algorithms, encapsulates each one, and makes them interchangeable. The pattern lets the algorithm vary independently from clients that use it.

## Implementation in This Project

- **Strategy Interface**: Defines a contract (`Drive` method) for all concrete strategies.
- **Concrete Strategies**: `NormalStrategy`, `SportsStrategy`, and `OffroadStrategy` implement the `Strategy` interface.
- **Context (Vehicle)**: Accepts a `Strategy` object and delegates the `Drive` method to the injected strategy.
- **Concrete Contexts**: `NormalVehicle` and `SportsVehicle` inject specific strategies.

## Example Code

```csharp
public interface Strategy {
    void Drive();
}

public class NormalStrategy : Strategy {
    public void Drive() {
        Console.WriteLine("Normal Strategy Injected");
    }
}

public class SportsStrategy : Strategy {
    public void Drive() {
        Console.WriteLine("Sports Strategy Injected");
    }
}

public class Vehicle {
    private Strategy injectedStrategy;
    public Vehicle(Strategy s) {
        this.injectedStrategy = s;
    }
    public void Drive() {
        injectedStrategy.Drive();
    }
}

public class NormalVehicle : Vehicle {
    public NormalVehicle() : base(new NormalStrategy()) {}
}

public class SportsVehicle : Vehicle {
    public SportsVehicle() : base(new SportsStrategy()) {}
}
```

## Usage

```csharp
SportsVehicle sportsCar = new SportsVehicle();
sportsCar.Drive(); // Output: Sports Strategy Injected

NormalVehicle normalCar = new NormalVehicle();
normalCar.Drive(); // Output: Normal Strategy Injected
```

## Benefits

- Promotes code reusability and flexibility
- Makes it easy to add new strategies without changing existing code
- Reduces conditional statements

## When to Use

- When you have multiple related algorithms for a specific task
- When you want to switch algorithms at runtime
- When you want to avoid using many conditional statements
