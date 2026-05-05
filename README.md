
# CS3009 Backend Performance Optimization
## Course Instructor & Project Supervisor: Dr. Sobia Iftikhar
## Semester & Section: BS(CS)-6A (SP26)

## Project Description:
**ASP.NET Core 8 Web API** demonstrating four key backend optimization techniques using **Entity Framework Core** and **Oracle Database 21c XE**.

- **Database Indexing Impact Analysis**: compare indexed vs non‑indexed queries.
- **Query Optimization Benchmarking**: projection, tracking, raw SQL, and `LIKE` vs `Contains`.
- **Concurrency Handling**: optimistic concurrency with row versioning.
- **Multithreading vs Asynchronous Processing**: sync/async I/O, parallel tasks, CPU‑bound offloading.

## Tech Stack

| Layer                | Technology                                                       |
|----------------------|------------------------------------------------------------------|
| Backend              | C# / ASP.NET Core 8                                              |
| ORM                  | Entity Framework Core 8 + Oracle EF Core provider                |
| Database             | Oracle Database 21c XE (Service name: `XEPDB1`)                  |
| Frontend             | Next.js 14 (static export, TypeScript, Tailwind CSS)             |
| Testing              | xUnit + Moq                                                      |
| API Documentation    | Swagger / OpenAPI                                                |
| IDE                  | Visual Studio Code (with C# Dev Kit, Oracle Developer Tools)     |

## Project Structure

```
OracleDemo/
├── Controllers/               # API endpoints (5 controllers)
├── Data/                      # AppDbContext (EF Core)
├── Models/                    # Product entity (RowVersion for concurrency)
├── Repositories/              # Repository pattern (IProductRepository)
├── Strategies/                # Strategy pattern (IQueryStrategy + 3 impl)
├── Services/                  # Singleton BenchmarkService
├── Migrations/                # EF Core migrations (index, row version)
├── frontend/                  # Next.js static export (built to /out)
├── UML/                       # PlantUML diagrams
├── Program.cs                 # App configuration, DI, static file serving
└── appsettings.json           # Connection string, logging
```

## Prerequisites

- **.NET SDK 8.0**: installed on `D:\DevTools\dotnet` (or your preferred location)
- **Oracle Database 21c XE**: running with service name `XEPDB1`, user `zubair` / password `abc123`
- **Visual Studio Code** with extensions:
  - C# Dev Kit
  - Oracle Developer Tools for VS Code (for DB browsing)
  - Thunder Client (optional, for manual testing)
- **Node.js** (v20+): only for building the frontend

## Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/zahmed02/CS3009-Backend-Performance-Optimization.git
cd CS3009-Backend-Performance-Optimization/OracleDemo
```

### 2. Configure the Database Connection

Edit `appsettings.json`: update the connection string if your Oracle credentials differ:

```json
"ConnectionStrings": {
  "DefaultConnection": "User Id=zubair;Password=abc123;Data Source=localhost:1521/XEPDB1;"
}
```

### 3. Apply Database Migrations

```bash
dotnet ef database update
```

This creates the `PRODUCTS` table, the index on `PRODUCT_NAME`, and the `ROWVERSION` column.

### 4. (Optional) Generate Test Data

Once the API is running, you can generate 10,000 products via Thunder Client or Swagger:

```
POST http://localhost:8080/api/indexingdemo/generate-test-data?count=10000
```

### 5. Build and Run the Frontend

```bash
cd frontend
npm install
npm run build          # creates static export in frontend/out
cd ..
```

The backend serves these static files automatically (no separate web server required).

### 6. Run the Backend

```bash
dotnet run
```

The API will be available at `http://localhost:8080`.  
Swagger UI: `http://localhost:8080/swagger`

## Testing

### Unit Tests (xUnit + Moq)

```bash
cd ../OracleDemo.Tests   # from the solution root
dotnet test
```

The test suite covers the `AsyncVsSyncController` (6 passing tests). For controllers that depend on the database (indexing, concurrency, products), manual integration tests are performed via Thunder Client.

### Manual API Testing

Use **Thunder Client** (VS Code extension) or **Swagger** to call the endpoints described below.

## API Endpoints Overview

All endpoints are under `http://localhost:8080/api/`

### Products (CRUD): Repository Pattern

| Method | Endpoint                     | Description                    |
|--------|------------------------------|--------------------------------|
| GET    | `/api/products`              | Get all products               |
| GET    | `/api/products/{id}`         | Get a single product           |
| POST   | `/api/products`              | Create a new product           |
| PUT    | `/api/products/{id}`         | Update a product (needs rowVersion) |
| DELETE | `/api/products/{id}`         | Delete a product               |

### Indexing Impact Analysis

| Method | Endpoint                                           | Description                                       |
|--------|----------------------------------------------------|---------------------------------------------------|
| GET    | `/api/indexingdemo/slow`                           | Table scan (`LIKE '%a%'`) – no index              |
| GET    | `/api/indexingdemo/fast`                           | Index scan (`LIKE 'A%'`) – uses index             |
| GET    | `/api/indexingdemo/benchmark`                     | Runs both queries multiple times (averages)       |
| GET    | `/api/indexingdemo/strategy/{indexed\|nonindexed\|rawsql}` | Strategy pattern – switch algorithms at runtime |
| POST   | `/api/indexingdemo/generate-test-data`            | Fill table with sample data                       |

### Query Optimization Benchmarking

| Method | Endpoint                                         | Comparison                       |
|--------|--------------------------------------------------|----------------------------------|
| GET    | `/api/queryoptimization/projection`              | SELECT * vs projection           |
| GET    | `/api/queryoptimization/tracking`                | Tracking vs AsNoTracking         |
| GET    | `/api/queryoptimization/linq-vs-sql`             | LINQ vs parameterised raw SQL    |
| GET    | `/api/queryoptimization/contains-vs-like`        | `Contains` vs `LIKE 'A%'`        |
| GET    | `/api/queryoptimization/benchmark-all`           | Runs all comparisons N times     |

### Concurrency Handling (Optimistic)

| Method | Endpoint                              | Description                                         |
|--------|---------------------------------------|-----------------------------------------------------|
| GET    | `/api/concurrencydemo/{id}`           | Fetch product (includes current rowVersion)        |
| PUT    | `/api/concurrencydemo/{id}`           | Update – checks rowVersion, increments on success  |
| POST   | `/api/concurrencydemo/simulate-race`  | Simulates two concurrent updates (one wins)        |

### Async vs Multithreading

| Method | Endpoint                                          | Behaviour                          |
|--------|---------------------------------------------------|------------------------------------|
| GET    | `/api/asyncvssync/sync`                          | Synchronous I/O (blocks thread)    |
| GET    | `/api/asyncvssync/async`                         | Asynchronous I/O (non‑blocking)    |
| GET    | `/api/asyncvssync/parallel-async?count=10`       | 10 async delays – concurrent       |
| GET    | `/api/asyncvssync/parallel-sync?count=10`        | 10 sync sleeps – sequential        |
| GET    | `/api/asyncvssync/cpu-bound-sync`                | CPU‑heavy loop on request thread   |
| GET    | `/api/asyncvssync/cpu-bound-async`               | CPU‑heavy loop offloaded to thread pool |
| GET    | `/api/asyncvssync/load-test?requests=10&useAsync=true/false` | Compare async vs sync load |

## Design Patterns Used

| Pattern             | Location                                                                 | Benefit                                      |
|---------------------|--------------------------------------------------------------------------|----------------------------------------------|
| **Repository**      | `IProductRepository` / `ProductRepository`                               | Decouples controllers from data access      |
| **Strategy**        | `IQueryStrategy` + three concrete strategies (`Indexed`, `NonIndexed`, `RawSql`) | Runtime selection of query algorithms |
| **Singleton**       | `BenchmarkService.Instance`                                              | Single source of performance metrics        |
| **Unit of Work**    | EF Core `DbContext`                                                      | Atomic transactions (SaveChanges)            |
| **Dependency Injection** | Built‑in ASP.NET Core DI container                               | Loose coupling, testability                  |

## UML Diagrams

PlantUML diagrams are available in the `UML/` folder (generated PNGs in `out/UML/`):

- `class_diagram.puml` – overall architecture (controllers, repositories, strategies)
- `sequence_concurrency.puml` – race condition flow
- `sequence_async_vs_sync.puml` – load test comparison
- `activity_indexing.puml` – indexing benchmark
- `component_diagram.puml` – layered architecture
- `deployment_diagram.puml` – physical deployment view

## Frontend

The frontend is a **static Next.js** application (TypeScript, Tailwind CSS). After building (`npm run build`), the `frontend/out` folder is served directly by ASP.NET Core via `UseStaticFiles()` and `UseDefaultFiles()`.

To modify the frontend, edit the files in `frontend/app` and rebuild.

*Performance results vary; run the benchmarks on your own machine to see the improvements.*

## Dependencies

### Environment Variables (User Variables)

| Variable            | Value                                           | Purpose                                   |
|---------------------|-------------------------------------------------|-------------------------------------------|
| `DOTNET_ROOT`       | `D:\DevTools\dotnet`                            | Points to .NET SDK installation           |
| `DOTNET_TOOLS_DIR`  | `D:\DevTools\dotnet-tools`                      | Stores global .NET tools                  |
| `NUGET_PACKAGES`    | `D:\DevTools\dotnet\.nuget\packages`            | Cache for NuGet packages                  |
| `Path (user)`       | Added `D:\DevTools\dotnet-tools` ; removed `C:\Users\zubai\.dotnet\tools` | Ensures tools on D: drive are found first |

### .NET SDK

- **Version:** 8.0.412
- **Location:** `D:\DevTools\dotnet`
- **Installation:** Official installer with custom path
- **Download:** [dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Global .NET Tools

| Tool                  | Version   | Location                       |
|-----------------------|-----------|--------------------------------|
| `dotnet-ef` (EF Core) | 8.0.13    | `D:\DevTools\dotnet-tools`     |
| `dotnet-script`       | latest    | `D:\DevTools\dotnet-tools`     |

**Installed with:**
```bash
dotnet tool install --global dotnet-ef --version 8.0.13
```

### NuGet Packages (for `OracleDemo` project)

All packages added inside `D:\DevTools\dotnet-projects\OracleDemo`:

```bash
dotnet add package Oracle.EntityFrameworkCore --version 8.23.60
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.13
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.13
```

Packages are stored in `D:\DevTools\dotnet\.nuget\packages`.

### VS Code Extensions

| Extension                         | Publisher | Installed |
|-----------------------------------|-----------|-----------|
| C# Dev Kit                        | Microsoft | ✅        |
| Oracle Developer Tools for VSCode | Oracle    | ✅        |
| Thunder Client                    | Ranga Vadhineni | ✅  |

Installed via Extensions panel (`Ctrl+Shift+X`).

### Oracle Database

- **Edition:** Oracle Database 21c XE
- **Location:** `D:\Oracle\app\zubai\product\21c`
- **Service Name:** `XEPDB1`
- **User / Password:** `zubair` / `abc123`
- **Sample schema:** `hr` (loaded for practice)

### Commands Run (with Locations)

| Command                                                                 | Location                                           |
|-------------------------------------------------------------------------|----------------------------------------------------|
| `cd D:\DevTools\dotnet-projects`                                        | D:\DevTools\dotnet-projects                        |
| `dotnet new webapi -n OracleDemo`                                      | D:\DevTools\dotnet-projects                        |
| `cd OracleDemo`                                                         | D:\DevTools\dotnet-projects\OracleDemo             |
| `code .`                                                                | D:\DevTools\dotnet-projects\OracleDemo             |
| `dotnet add package Oracle.EntityFrameworkCore --version 8.23.60`      | same                                               |
| `dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.13` | same                                           |
| `dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.13` | same                                         |
| `mkdir Models`, `mkdir Data`                                            | same                                               |
| `code Models\Product.cs` etc.                                           | same                                               |
| `dotnet ef migrations add InitialCreate`                                | same                                               |
| `dotnet ef database update`                                             | same                                               |
| `dotnet ef migrations add AddIndexOnProductName` (Step 1)              | same                                               |
| `dotnet ef database update`                                             | same                                               |
| `dotnet ef migrations add UseLongRowVersion` (Step 3)                  | same                                               |
| `dotnet ef database update`                                             | same                                               |
| `dotnet run`                                                            | same                                               |

**Additional controllers (no terminal commands):**
- `IndexingDemoController.cs`
- `QueryOptimizationController.cs` (Step 2 – code only)
- `ConcurrencyDemoController.cs`
