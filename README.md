# CrossApp_2026

A C#/.NET application for managing customer orders.

The system allows users to create orders, add products, calculate order totals, confirm orders, and store data in memory or files. It also supports LINQ reports, asynchronous imports, automated testing, and configurable data storage.

**Domain:** Order Management  
**Main entities:** Order, OrderLine  
**Platform:** .NET 8 / .NET 10  
**Development environment:** Arch Linux x64, VS Code

## 1. Project Structure

```text
CrossApp_2026/
├── src/
│   ├── Core/
│   │   ├── Domain/
│   │   ├── Abstractions/
│   │   ├── Services/
│   │   ├── Storage/
│   │   ├── Reports/
│   │   ├── Import/
│   │   └── Configuration/
│   └── Cli/
│       ├── Configuration/
│       ├── Notifications/
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── appsettings.Production.json
├── tests/
│   └── Core.Tests/
├── CrossApp.slnx
└── README.md
```

`Core` contains business logic, domain models, services, storage implementations, and reports.

`Cli` handles user interaction, application configuration, and command execution.

`Core.Tests` contains automated tests using xUnit.

## 2. Domain Invariants

The application enforces the following business rules:

1. Order ID and customer name cannot be empty.
2. Product price cannot be negative.
3. Product quantity must be greater than zero.
4. An empty order cannot be confirmed.
5. New lines cannot be added to a confirmed order.

Invalid method arguments cause `ArgumentException` or `ArgumentOutOfRangeException`.

Business rule violations, such as modifying a confirmed order, cause `DomainException`.

## 3. Storage and Dependency Injection

The application uses the `IOrderStore` interface with the following operations:

- `List()`
- `GetById()`
- `Add()`
- `Update()`
- `Remove()`

Two storage implementations are available:

| Implementation | Description |
|---|---|
| `InMemoryOrderStore` | Stores orders in memory |
| `FileOrderStore` | Stores orders in a JSON file |

Manual Dependency Injection is used to pass the selected storage implementation to `OrderService`.

The `--file` argument enables file-based storage.

### Running the application

Build:

```bash
dotnet build
```

Run with in-memory storage:

```bash
dotnet run --project src/Cli
```

Run with file storage:

```bash
dotnet run --project src/Cli -- --file
```

File storage preserves orders, their lines, and confirmation status between application runs.

## 4. Lab 6 — Events

`OrderService` provides the `Changed` event.

The event is raised after a successful modification of an order.

The CLI supports event subscriptions and notifications through console or file output.

The implementation also includes an optional CSV importer with error reporting.

## 5. Lab 7 — LINQ Reports

LINQ to Objects is used to filter, group, sort, and aggregate order data.

Implemented reports:

| Command | Description |
|---|---|
| `report-top` | Top 5 customers by total amount |
| `report-high` | Orders above the specified amount |
| `report-groups` | Grouping by confirmation status |
| `report-sum` | Total, average, and maximum order amounts |
| `report-deferred` | Deferred execution demonstration |

### Optional Tasks

**1. GroupJoin**

`GroupJoin` combines customers with their orders. Customers without orders are included with zero totals.

Command: `report-join`

**2. Custom TopBy Extension**

The generic `TopBy<T>()` method selects the top N elements using primary and secondary sorting keys.

It is used for customer and order reports.

Command: `report-top-orders`

**3. LINQ Query Syntax**

Two versions of the status report are implemented:

- Method Syntax: `GroupBy()`, `Select()`, `OrderBy()`.
- Query Syntax: `from`, `group`, `into`, `select`.

Method Syntax is convenient for chaining operations. Query Syntax resembles SQL and can make grouping easier to read.

The results are compared using `SequenceEqual()`.

Command:

```bash
dotnet run --project src/Cli -- report-query
```

Expected result:

```text
Результати збігаються: True
```

**4. Performance Comparison**

`Stopwatch` compares LINQ processing of 100,000 generated orders with and without an intermediate `ToList()`.

Command: `report-bench`

## 6. Lab 8 — Async/Await and Unit Tests

Asynchronous JSON import of order lines is implemented using `FileStream`, `JsonSerializer.DeserializeAsync()`, and `async/await`.

`CancellationToken` is passed through the following components:

`Cli → OrderService.ImportAsync() → JsonOrderLineImporter.LoadAsync()`

The application supports cancellation through Ctrl+C and handles `OperationCanceledException` separately from domain and file errors.

### Optional Tasks

1. Streaming import using `IAsyncEnumerable`, `await foreach`, and `[EnumeratorCancellation]`.
2. Linked cancellation tokens using `CreateLinkedTokenSource()` for Ctrl+C and timeout.
3. Import progress reporting through `IProgress<int>`.
4. Code coverage collection using `XPlat Code Coverage`.

### Commands

Standard import:

```bash
dotnet run --project src/Cli -- --import
```

Streaming import:

```bash
dotnet run --project src/Cli -- --import-stream --expected 2
```

Timeout demonstration:

```bash
dotnet run --project src/Cli -- --import-stream --expected 2 --timeout-ms 50 --delay-ms 200
```

Ctrl+C demonstration:

```bash
dotnet run --project src/Cli -- --import-stream --expected 2 --delay-ms 3000
```

Code coverage:

```bash
dotnet test tests/Core.Tests/Core.Tests.csproj --collect:"XPlat Code Coverage"
```

## 7. Lab 9 — Paths, JSON, Configuration, Secrets

Application settings are loaded from JSON configuration files with support for environment variables, command-line arguments, and User Secrets.

File paths are constructed using `Path.Combine()`, `Path.GetFullPath()`, and `AppContext.BaseDirectory`.

### Configuration Profiles

| Profile | Logging Level | Data Directory |
|---|---|---|
| Development | Debug | data |
| Production | Warning | data/production |

Configuration files are located in `src/Cli/`.

### Configuration Keys

| Key | Description | Default |
|---|---|---|
| `EnvironmentName` | Application environment | Development |
| `Storage:DataPath` | Data directory | data |
| `Storage:FileName` | Storage filename | orders.json |
| `Storage:Format` | Export/import format | json |
| `Logging:MinimumLevel` | Logging level | Information |
| `Orders:Currency` | Order currency | UAH |
| `Integration:ApiKey` | Secret API key | Not set |

Configuration sources are applied in this order:

1. `appsettings.json`
2. Environment-specific JSON
3. User Secrets
4. Environment variables
5. Command-line arguments

Later sources override earlier values for the same key.

### JSON Export and Import

Export orders:

```bash
dotnet run --project src/Cli -- export
```

Import orders:

```bash
dotnet run --project src/Cli -- import-all
```

`JsonOrderDataFormat` preserves order details, confirmation status, and product lines.

Atomic writing is implemented using a temporary `.tmp` file followed by `File.Move()`.

JSON serialization uses `WriteIndented` and `UnsafeRelaxedJsonEscaping` for readable output.

### Environment Variable Overrides

**Linux (bash):**

```bash
CROSSAPP_DATA_PATH=/tmp/crossapp-env dotnet run --project src/Cli -- export
```

The nested `Storage:DataPath` setting can also be overridden using `CROSSAPP_Storage__DataPath`.

**Windows (PowerShell):**

```powershell
$env:CROSSAPP_DATA_PATH = "C:\Temp\CrossApp"
dotnet run --project src/Cli -- export
Remove-Item Env:CROSSAPP_DATA_PATH
```

### User Secrets

Initialize User Secrets:

```bash
dotnet user-secrets init --project src/Cli
```

Set a local development key:

```bash
dotnet user-secrets set "Integration:ApiKey" "local-dev-key" --project src/Cli
```

List stored secrets:

```bash
dotnet user-secrets list --project src/Cli
```

On Linux, User Secrets are stored under:

`~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

Secret values are not stored in the repository.

The CLI displays only whether the API key exists and its length.

### Lab 9 Optional Tasks

**1. Configuration Validation**

`AppOptionsValidator` checks configuration values before application startup.

Invalid settings produce an error message and exit code `2`.

Example:

```bash
dotnet run --project src/Cli -- export --Storage:FileName=wrong.txt
echo $?
```

**2. Command-Line Path Override**

The `--data-path` argument overrides the configured directory and the `CROSSAPP_DATA_PATH` environment variable.

Example:

```bash
dotnet run --project src/Cli -- export --data-path=/tmp/crossapp-arg
```

**3. JSON and CSV Formats**

Both formats implement the `IOrderDataFormat` interface.

CSV export:

```bash
CROSSAPP_Storage__Format=csv CROSSAPP_Storage__FileName=orders.csv dotnet run --project src/Cli -- export
```

CSV import:

```bash
CROSSAPP_Storage__Format=csv CROSSAPP_Storage__FileName=orders.csv dotnet run --project src/Cli -- import-all
```

Verified result:

| Metric | Value |
|---|---:|
| Imported orders | 19 |
| Order lines | 20 |
| Total amount | 43,560.00 UAH |

**4. File Storage Unit Tests**

`Lab09StorageTests` verifies:

- Saving an order to a temporary directory.
- Reading the same order from a new `FileOrderStore` instance.
- Preserving order lines and confirmation status.
- CSV export/import.
- Rejecting invalid configuration values.

Temporary directories are created using `Path.GetTempPath()` and deleted after testing.

### Test Commands

Run all tests:

```bash
dotnet test
```

Run Lab 9 tests:

```bash
dotnet test tests/Core.Tests/Core.Tests.csproj --filter "FullyQualifiedName~Lab09StorageTests"
```

Verified test results:

| Scope | Passed | Failed |
|---|---:|---:|
| Lab 9 tests | 5 | 0 |
| All project tests | 18 | 0 |

## 8. Error Handling

The application uses specific exception types:

- `ArgumentException` — invalid method arguments.
- `DomainException` — violated business rules.
- `OperationCanceledException` — cancelled operations.
- `IOException` — file system errors.
- `JsonException` — invalid JSON data.

Invalid configuration is handled with a clear error message and exit code `2`.

## 9. Data and Security

Runtime data files and temporary files should be excluded from Git through `.gitignore`.

Development secrets must be stored using User Secrets or environment variables.

The application never prints the API key value to the console.
