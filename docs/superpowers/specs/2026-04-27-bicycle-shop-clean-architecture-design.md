# BicycleShop Clean Architecture — Design Spec
**Date:** 2026-04-27  
**Audience:** Intermediate C# students learning Clean Architecture patterns  
**Stack:** ASP.NET Core Web API, EF Core Code First, SQL Server (LocalDB), ASP.NET Core Identity, JWT, AutoMapper, .NET Aspire

---

## 1. Goals

Teach intermediate C# students the following concepts through a real, working Bicycle Shop API:

- Clean Architecture layer separation (Domain → Application → Infrastructure → Presentation)
- Repository Pattern with interfaces and Dependency Injection
- Service layer with business logic
- DTOs and AutoMapper
- EF Core Code First with SQL Server
- ASP.NET Core Identity + JWT authentication
- Role-Based Access Control (RBAC)
- Decorator Pattern for cross-cutting concerns (logging)
- .NET Aspire for structured logging and observability
- Global exception middleware with domain exception mapping

---

## 2. Solution Structure

```
BicycleShop.sln
├── BicycleShop.Domain             (no project references)
├── BicycleShop.Application        (refs: Domain)
├── BicycleShop.Infrastructure     (refs: Application, Domain)
├── BicycleShop.Presentation       (refs: Application, Infrastructure)
├── BicycleShop.ServiceDefaults    (Aspire shared config)
└── BicycleShop.AppHost            (Aspire orchestrator)
```

### Dependency Rule
Arrows point inward only. Domain has zero knowledge of any other layer. The compiler enforces this — if a student accidentally references `AppDbContext` from Domain, it will not build.

---

## 3. Domain Layer (`BicycleShop.Domain`)

**No NuGet dependencies.** Pure C# only.

### Entities

#### `Bicycle`
| Property | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| Name | string | Required |
| Brand | string | Required |
| Model | string | Required |
| BicycleType | BicycleType | Enum |
| PricePerHour | decimal | Base rental price |
| PurchasePrice | decimal | Shop purchase price |
| IsAvailable | bool | Availability flag |
| YearManufactured | int | |
| CreatedAt | DateTime | Auto-set |
| UpdatedAt | DateTime | Auto-updated |

#### `Rental`
| Property | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| BicycleId | Guid | FK to Bicycle |
| UserId | string | FK to Identity user |
| StartTime | DateTime | |
| EndTime | DateTime? | Null while active |
| TotalCost | decimal? | Null while active |
| Status | RentalStatus | Enum |

### Enums
- `BicycleType`: Road, Mountain, Hybrid, Electric, BMX
- `RentalStatus`: Active, Completed, Cancelled

### Repository Interfaces
Defined here, implemented in Infrastructure. This is the key DI teaching point.

```csharp
IBicycleRepository
  GetAllAsync()
  GetByIdAsync(Guid id)
  GetByTypeAsync(BicycleType type)
  GetAvailableAsync()
  SearchAsync(string brand, string? model)
  AddAsync(Bicycle bicycle)
  UpdateAsync(Bicycle bicycle)
  DeleteAsync(Guid id)
  SaveChangesAsync()

IRentalRepository
  GetByIdAsync(Guid id)
  GetActiveAsync()
  GetByBicycleIdAsync(Guid bicycleId)
  AddAsync(Rental rental)
  UpdateAsync(Rental rental)
  SaveChangesAsync()
```

### Domain Exceptions
```csharp
BicycleNotFoundException        : Exception
BicycleNotAvailableException    : Exception
InvalidRentalOperationException : Exception
```

---

## 4. Application Layer (`BicycleShop.Application`)

**Dependencies:** Domain only. No EF Core, no HTTP, no Identity.

### DTOs

| DTO | Purpose |
|---|---|
| `BicycleDto` | API response for a bicycle |
| `CreateBicycleDto` | Create request (no Id/timestamps) |
| `UpdateBicycleDto` | Update request |
| `RentalDto` | API response for a rental |
| `StartRentalDto` | `{ BicycleId }` |
| `RentalQuoteDto` | `{ BicycleId, Hours, BasePrice, TypeMultiplier, DiscountApplied, FinalPrice }` |
| `RegisterDto` | `{ Username, Email, Password, Role? }` |
| `LoginDto` | `{ Email, Password }` |
| `AuthResponseDto` | `{ Token, Expiry, Username, Role }` |

### AutoMapper
`BicycleMappingProfile` maps:
- `Bicycle → BicycleDto`
- `CreateBicycleDto → Bicycle`
- `Rental → RentalDto`

### Service Interfaces & Implementations

#### `IBicycleService` / `BicycleService`
- `GetAllAsync() → IEnumerable<BicycleDto>`
- `GetByIdAsync(Guid id) → BicycleDto`
- `GetByTypeAsync(BicycleType type) → IEnumerable<BicycleDto>`
- `GetAvailableAsync() → IEnumerable<BicycleDto>`
- `SearchAsync(string brand, string? model) → IEnumerable<BicycleDto>`
- `CreateAsync(CreateBicycleDto dto) → BicycleDto`
- `UpdateAsync(Guid id, UpdateBicycleDto dto) → BicycleDto`
- `DeleteAsync(Guid id)`

#### `IRentalPricingService` / `RentalPricingService`
The primary business logic teaching example — contains multiple private methods students can trace through:

- `GetRentalQuoteAsync(Guid bicycleId, int hours) → RentalQuoteDto`
- `ApplyDiscountAsync(Guid bicycleId, int hours, string discountCode) → RentalQuoteDto`
- `StartRentalAsync(Guid bicycleId, string userId) → RentalDto`
- `CompleteRentalAsync(Guid rentalId) → RentalDto`
- `GetActiveRentalsAsync() → IEnumerable<RentalDto>`
- `GetRentalHistoryForBicycleAsync(Guid bicycleId) → IEnumerable<RentalDto>`

**Pricing business logic (multi-method chain):**
```
GetRentalQuoteAsync
  → CalculateBasePrice(pricePerHour, hours)
  → ApplyTypeMultiplier(basePrice, bicycleType)
  → ApplyBulkDiscount(price, hours)         ← 8+ hours = 20% off automatically
  → RoundToTwoDecimals(price)

ApplyDiscountAsync
  → GetRentalQuoteAsync(...)                ← reuses quote logic
  → ValidateDiscountCode(code)
  → CalculateDiscountAmount(price, code)
  → ApplyDiscount(price, discountAmount)
```

**Type multipliers:** Electric=1.5×, Mountain=1.3×, Road=1.1×, Hybrid=1.0×, BMX=0.9×  
**Discount codes:** `STUDENT10`=10%, `WEEKEND15`=15%, `BULK20`=20% (8+ hours only)

---

## 5. Infrastructure Layer (`BicycleShop.Infrastructure`)

**Dependencies:** Application + Domain. This is the only layer that touches EF Core, Identity, and JWT.

### `AppDbContext`
- Inherits `IdentityDbContext<ApplicationUser>`
- `DbSet<Bicycle>`, `DbSet<Rental>`
- Fluent API: decimal precision (18,2) on prices, required strings, FK relationships
- `SaveChangesAsync` override: auto-sets `CreatedAt` on insert, `UpdatedAt` on every save
- Seed data: 8 bicycles across all 5 types with varied pricing

### `ApplicationUser`
Extends `IdentityUser`:
- `FirstName (string)`
- `LastName (string)`

### Repository Implementations

**`BicycleRepository`** implements `IBicycleRepository` — injects `AppDbContext` directly.  
**`RentalRepository`** implements `IRentalRepository` — injects `AppDbContext` directly.

Both use async LINQ queries. Students can see that only these classes know about EF Core.

### Decorator Pattern — Logging Repositories

**`LoggingBicycleRepository`** wraps `IBicycleRepository`:
- Constructor takes `IBicycleRepository inner` + `ILogger<LoggingBicycleRepository>`
- Every method: logs start → calls inner → logs duration/result or logs error and rethrows
- Teaches Open/Closed Principle: logging added without touching `BicycleRepository`

**`LoggingRentalRepository`** — same pattern for `IRentalRepository`.

DI registration (explicit, shown as a teaching example in `Program.cs`):
```csharp
services.AddScoped<BicycleRepository>();
services.AddScoped<IBicycleRepository>(provider =>
    new LoggingBicycleRepository(
        provider.GetRequiredService<BicycleRepository>(),
        provider.GetRequiredService<ILogger<LoggingBicycleRepository>>()));
```

### JWT Token Service
**`IJwtTokenService`** / **`JwtTokenService`**:
- `GenerateToken(ApplicationUser user, IList<string> roles) → string`
- Reads config from `JwtSettings` (Key, Issuer, Audience, ExpiryMinutes)
- Adds claims: `sub`, `email`, `jti`, `role`

### `DbSeeder`
Runs at startup from `Program.cs`:
1. Applies pending EF Core migrations
2. Seeds `Admin` and `Customer` roles via `RoleManager`
3. Seeds a default admin user: `admin@bicycleshop.com` / `Admin123!`
4. Seeds 8 sample bicycles if table is empty

---

## 6. Presentation Layer (`BicycleShop.Presentation`)

**Dependencies:** Application + Infrastructure (for DI registration only).  
Controllers depend only on Application interfaces — never on Infrastructure directly.

### Controllers

#### `AuthController` — `/api/auth` — `[AllowAnonymous]`
| Method | Route | Description |
|---|---|---|
| POST | `/register` | Register, assign role, return token |
| POST | `/login` | Authenticate, return JWT token |

#### `BicyclesController` — `/api/bicycles`
| Method | Route | Policy | Description |
|---|---|---|---|
| GET | `/` | Authenticated | Get all bicycles |
| GET | `/{id}` | Authenticated | Get bicycle by ID |
| GET | `/available` | Authenticated | Get available bicycles |
| GET | `/type/{type}` | Authenticated | Get by type |
| GET | `/search` | Authenticated | Search by brand/model |
| POST | `/` | AdminOnly | Create bicycle |
| PUT | `/{id}` | AdminOnly | Update bicycle |
| DELETE | `/{id}` | AdminOnly | Delete bicycle |

#### `RentalsController` — `/api/rentals`
| Method | Route | Policy | Description |
|---|---|---|---|
| POST | `/quote` | Authenticated | Get rental price quote |
| POST | `/quote/discount` | Authenticated | Get quote with discount code |
| POST | `/start` | Authenticated | Start a rental |
| POST | `/{id}/complete` | Authenticated | Complete a rental |
| GET | `/active` | AdminOnly | Get all active rentals |
| GET | `/bicycle/{bicycleId}` | AdminOnly | Get rental history for bicycle |

### Authorization Policies
Registered in `Program.cs`:
- `"AdminOnly"` → Role: Admin
- `"CustomerOnly"` → Role: Customer
- `"Authenticated"` → any authenticated user

### Global Exception Middleware
`ExceptionHandlingMiddleware`:
- Catches `BicycleNotFoundException` → 404 ProblemDetails
- Catches `BicycleNotAvailableException` → 409 ProblemDetails
- Catches `InvalidRentalOperationException` → 400 ProblemDetails
- Catches all other exceptions → 500 ProblemDetails
- Logs every exception with full stack trace via `ILogger`

### `Program.cs` Responsibilities
- `builder.AddServiceDefaults()` — Aspire integration
- Register Identity + JWT
- Register AutoMapper
- Register repositories (with decorator wiring)
- Register services
- Register authorization policies
- Add Swagger with JWT bearer support
- Map controllers
- Apply `ExceptionHandlingMiddleware`
- Call `DbSeeder.SeedAsync()`

---

## 7. Aspire Setup

### Projects
- **`BicycleShop.AppHost`** — references Presentation project, defines resources
- **`BicycleShop.ServiceDefaults`** — OpenTelemetry, health checks, service discovery

### What Students See in the Aspire Dashboard
- Structured logs per request (including decorator logs showing timing)
- Distributed traces showing the full call stack (Controller → Service → Repository)
- Metrics: request count, duration histograms
- Health check status

### `AppHost` resource definition:
```csharp
var api = builder.AddProject<Projects.BicycleShop_Presentation>("bicycle-shop-api");
```

---

## 8. RBAC Summary

| Role | Can Do |
|---|---|
| Admin | Everything: full CRUD on bicycles, view all rentals, manage system |
| Customer | Read bicycles, get quotes, start/complete own rentals |
| Anonymous | Register, login only |

Role seeded at startup. Registration endpoint accepts optional `role` param for easy student testing.

---

## 9. Key Teaching Points (Summary)

| Concept | Where It Lives | Example |
|---|---|---|
| Entities & Domain Logic | Domain | `Bicycle`, `Rental`, domain exceptions |
| Interface definitions | Domain | `IBicycleRepository`, `IRentalRepository` |
| Business Logic | Application | `RentalPricingService` multi-method chain |
| DTOs & Mapping | Application | `BicycleDto`, `BicycleMappingProfile` |
| Dependency Injection | All layers | Interfaces registered in `Program.cs`, injected via constructors |
| EF Core & DB access | Infrastructure only | `AppDbContext`, `BicycleRepository` |
| Decorator Pattern | Infrastructure | `LoggingBicycleRepository` wrapping `BicycleRepository` |
| Auth & Identity | Infrastructure + Presentation | `JwtTokenService`, `AuthController`, policies |
| HTTP concerns | Presentation only | Status codes, `ProblemDetails`, middleware |
| Observability | Aspire + Presentation | Dashboard, structured logs, traces |
