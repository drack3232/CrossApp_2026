# CrossApp_2026
This project models a delivery service where a Customer can create an Order consisting of various Products 
By utilizing OrderLine entries, the system processes the order operations and calculates the total checkout amount

Domain: Orders. Entities: Customer, Product, Order, OrderLine
Purpose: Processing delivery orders and calculating total amounts

## Environment
.NET SDK 10.0, Arch Linux x64

## Run
```bash
dotnet build
dotnet run --project src/Cli
