# CrossApp_2026
This project models a delivery service where a Customer can create an Order consisting of various Products 
By utilizing OrderLine entries, the system processes the order operations and calculates the total checkout amount

Domain: Orders. Entities: Customer, Product, Order, OrderLine
Purpose: Processing delivery orders and calculating total amounts

## Environment
.NET SDK 10.0, Arch Linux x64

## Lab 4: Domain Model - Invariants and Encapsulation

**Domain:** Order Management (`Order` and `OrderLine`)

### Domain Invariants (Business Rules)

1. **Order ID and Customer name cannot be empty or whitespace.** 
   * Method: `Order.Create`
   * Throws: `ArgumentException`
2. **Product price cannot be negative.** 
   * Method: `OrderLine.Create`
   * Throws: `ArgumentOutOfRangeException`
3. **Product quantity in an order line must be greater than zero.** 
   * Method: `OrderLine.Create`
   * Throws: `ArgumentOutOfRangeException`
4. **Cannot confirm an empty order (an order containing no items).** 
   * Method: `Order.Confirm`
   * Throws: `InvalidOperationException`
5. **Cannot add new order lines to an already confirmed order.** 
   * Method: `Order.AddLine`
   * Throws: `InvalidOperationException`

## Run
```bash
dotnet build
dotnet run --project src/Cli
