
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


### Lab 7 — Optional Tasks

1. GroupJoin combines customers and orders into CustomerOrdersSummary.
   Customers without orders are included with zero totals.

2. Custom TopBy<T> LINQ extension sorts data by a decimal
   value and a secondary string key.
   It is used in TopCustomers and TopOrders.

3. Query syntax uses from, group, into and select.
   ByStatusQuerySyntax produces the same results as ByStatus.
   Method syntax is convenient for combining many operators,
   while query syntax makes grouping easier to read.

4. Stopwatch compares LINQ processing of 100,000 generated
   orders with and without an intermediate ToList().
   Both queries calculate the same sum.
   Intermediate materialization creates an additional list
   and may increase memory usage and execution time.

Commands:
- report-join
- report-top-orders
- report-query
- report-bench

### Lab 7 — LINQ Query Syntax

Implemented two versions of the order status grouping report in `ReportService.cs`:

- **Method Syntax (`ByStatus`)** — uses `GroupBy()`, `Select()`, `OrderBy()` and `ToList()`.
- **Query Syntax (`ByStatusQuerySyntax`)** — uses `from`, `group`, `into` and `select`.

Both methods group orders by confirmation status and calculate the number of orders and their total amount for each group.

**Readability comparison:**

Method Syntax is more convenient for building chains of LINQ operations and is generally more compact.

Query Syntax resembles SQL and makes grouping operations easier to understand, especially when using `group ... into`.

Both approaches produce the same results.

**Verification command:**

`dotnet run --project src/Cli -- report-query`

**Expected result:**

`Результати збігаються: True`

The comparison is performed using `SequenceEqual()`, which checks whether both reports contain equal elements in the same order.

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
