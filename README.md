# ShopApi — a .NET learning project

> A small REST API for a toy shop (products, users, orders), built to **learn ASP.NET Core
> and Entity Framework Core**. It's intentionally simple — the goal was to practice the core
> building blocks of a real .NET web API, not to be production-grade.

## What this project taught me
Building this covered the fundamentals of an ASP.NET Core Web API end to end:

- **ASP.NET Core Web API** — `[ApiController]`, attribute routing, `ControllerBase`, REST conventions
- **Entity Framework Core (code-first)** — `DbContext`, `DbSet<T>`, LINQ queries, `Include`/`ThenInclude` for related data
- **Migrations** — `dotnet ef migrations add` / `database update`; schema generated from C# models
- **Relationships & constraints** — 1-to-many (Order → OrderItems), foreign keys, a unique index on email, cascade vs restrict delete rules
- **Dependency injection** — `DbContext` and `ILogger` injected into controllers via the built-in container
- **DTOs** — `CreateOrderDto` / `OrderItemDto` to shape request bodies instead of exposing entities directly
- **Custom middleware** — a `RequestLoggingMiddleware` in the pipeline
- **async/await** — async EF Core calls throughout
- **Swagger / OpenAPI** — interactive API docs via Swashbuckle
- **Configuration** — connection strings via `appsettings.json` / environment variables

## Stack
- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core 8 + **PostgreSQL** (Npgsql)
- Swashbuckle (Swagger UI)

## Domain model
```
User 1───* Order 1───* OrderItem *───1 Product
```
- **Product** — Id, Name, Description, Price
- **User** — Id, FullName, Email (unique)
- **Order** — Id, CreatedAt, UserId → User, Items
- **OrderItem** — Id, OrderId, ProductId → Product, Quantity, UnitPrice

## Endpoints
| Method | Route | Description |
|---|---|---|
| GET | `/ping` | Health check → `pong` |
| GET | `/api/products` · `/api/products/{id}` | List / get products |
| POST · PUT · DELETE | `/api/products` · `/api/products/{id}` | Create / update / delete a product |
| GET | `/api/users` · `/api/users/{id}` | List / get users |
| POST · PUT · DELETE | `/api/users` · `/api/users/{id}` | Create / update / delete a user |
| GET | `/api/orders` · `/api/orders/{id}` | List / get orders (with user + items) |
| POST | `/api/orders` | Create an order (validates user & products) |
| DELETE | `/api/orders/{id}` | Delete an order |

## Running it
**Prerequisites:** .NET 8 SDK and a PostgreSQL server.

1. Set your database connection string. Don't commit real passwords — override the placeholder in
   `appsettings.json` with an environment variable or user-secrets:
   ```bash
   # option A: environment variable
   set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=shopdb;Username=postgres;Password=YOURPASS
   # option B: user-secrets (keeps it out of git)
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=shopdb;Username=postgres;Password=YOURPASS"
   ```
2. Run it — migrations are applied automatically on startup (the schema and seed data are created on first run):
   ```bash
   dotnet run
   ```
3. Open the root URL — it redirects to **Swagger UI** (`/swagger`) where you can try every endpoint.
   Sample request bodies are also in `ShopApi.http` and `body.json`.

Seed data (2 products + a demo user) is inserted via EF Core `HasData`, so the API has something to
show immediately.

## Notes & next steps (learning backlog)
This is a learning project, so a few things are deliberately left simple. If I took it further:
- Input validation (data annotations / FluentValidation) and consistent error responses
- AuthN/AuthZ (JWT) and password hashing for users
- A service/repository layer instead of querying `DbContext` directly in controllers
- Unit/integration tests (xUnit + `WebApplicationFactory`)
- Pagination on list endpoints

---
*Built while learning ASP.NET Core & EF Core.*
