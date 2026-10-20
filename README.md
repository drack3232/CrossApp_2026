
# CrossApp_2026

This project models a delivery service where a Customer can create an Order consisting of various Products.

The system uses OrderLine entries to manage order items, process orders, and calculate the total amount.

**Domain:** Order Management  
**Entities:** Customer, Product, Order, OrderLine  
**Purpose:** Processing delivery orders and calculating total amounts.

## Environment

- .NET SDK 10.0
- Arch Linux x64
- C#

### Domain Invariants (Business Rules)

1. **Order ID and Customer name cannot be empty or whitespace.**
   - Method: `Order.Create`
   - Throws: `ArgumentException`

2. **Product price cannot be negative.**
   - Method: `OrderLine.Create`
   - Throws: `ArgumentOutOfRangeException`

3. **Product quantity must be greater than zero.**
   - Method: `OrderLine.Create`
   - Throws: `ArgumentOutOfRangeException`

4. **Cannot confirm an empty order.**
   - Method: `Order.Confirm`
   - Throws: `InvalidOperationException`

5. **Cannot add new lines to a confirmed order.**
   - Method: `Order.AddLine`
   - Throws: `InvalidOperationException`


### Implemented Components

- **IOrderStore** - storage interface with List, GetById, Add, Update, and Remove methods.
- **InMemoryOrderStore** - stores orders in memory using Dictionary.
- **FileOrderStore** - stores orders in a JSON file.
- **OrderService** - provides CreateOrder, AddLine, and Confirm operations.

### Dependency Injection

Manual Dependency Injection is implemented in `Program.cs`.

The storage implementation is selected using the `--file` argument and passed to `OrderService` through its constructor.

Architecture:

```text
Cli
 └── OrderService
      └── IOrderStore
           ├── InMemoryOrderStore
           └── FileOrderStore
```

No DI container is used.

### Storage Behavior

- **InMemory:** Changes are lost after the application exits.
- **File:** Orders are saved to `data/orders.json` in the CLI output directory and restored on subsequent runs.
- Each order is stored together with its lines and confirmation status.

## Run

### Build

```bash
dotnet build
```

### In-Memory Storage

```bash
dotnet run --project src/Cli
```

### File Storage (JSON)

```bash
dotnet run --project src/Cli -- --file
```

The `orders.json` file is created automatically when running in file mode.

## Error Handling

Invalid operations are handled using exceptions.

The CLI demonstrates error handling for domain invariant violations and operations involving non-existent orders.
