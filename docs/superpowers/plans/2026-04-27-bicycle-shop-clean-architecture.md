# BicycleShop Clean Architecture Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a fully working ASP.NET Core Web API teaching project demonstrating Clean Architecture with Bicycles CRUD, rental/pricing business logic, EF Core Code First, ASP.NET Core Identity + JWT, RBAC, Repository Pattern, Decorator Pattern for logging, and .NET Aspire observability.

**Architecture:** Four separate C# projects (Domain → Application → Infrastructure → Presentation) enforce the dependency rule at compile time. Two Aspire projects (AppHost, ServiceDefaults) wire up observability. A test project covers the Application layer business logic with xUnit + Moq.

**Tech Stack:** .NET 9, ASP.NET Core Web API, EF Core 9 + SQL Server (LocalDB), ASP.NET Core Identity, JWT Bearer, AutoMapper, .NET Aspire 9, xUnit, Moq, Swashbuckle (Swagger)

---

## File Map

```
BicycleShop.sln
│
├── BicycleShop.Domain/
│   ├── Entities/Bicycle.cs
│   ├── Entities/Rental.cs
│   ├── Enums/BicycleType.cs
│   ├── Enums/RentalStatus.cs
│   ├── Interfaces/IBicycleRepository.cs
│   ├── Interfaces/IRentalRepository.cs
│   ├── Exceptions/BicycleNotFoundException.cs
│   ├── Exceptions/BicycleNotAvailableException.cs
│   └── Exceptions/InvalidRentalOperationException.cs
│
├── BicycleShop.Application/
│   ├── DTOs/BicycleDto.cs
│   ├── DTOs/CreateBicycleDto.cs
│   ├── DTOs/UpdateBicycleDto.cs
│   ├── DTOs/RentalDto.cs
│   ├── DTOs/StartRentalDto.cs
│   ├── DTOs/RentalQuoteDto.cs
│   ├── DTOs/Auth/RegisterDto.cs
│   ├── DTOs/Auth/LoginDto.cs
│   ├── DTOs/Auth/AuthResponseDto.cs
│   ├── Interfaces/IBicycleService.cs
│   ├── Interfaces/IRentalPricingService.cs
│   ├── Mappings/BicycleMappingProfile.cs
│   ├── Services/BicycleService.cs
│   └── Services/RentalPricingService.cs
│
├── BicycleShop.Infrastructure/
│   ├── Data/AppDbContext.cs
│   ├── Data/DbSeeder.cs
│   ├── Identity/ApplicationUser.cs
│   ├── Auth/IJwtTokenService.cs
│   ├── Auth/JwtTokenService.cs
│   ├── Repositories/BicycleRepository.cs
│   ├── Repositories/RentalRepository.cs
│   ├── Repositories/Decorators/LoggingBicycleRepository.cs
│   ├── Repositories/Decorators/LoggingRentalRepository.cs
│   └── Extensions/InfrastructureExtensions.cs
│
├── BicycleShop.Presentation/
│   ├── Controllers/AuthController.cs
│   ├── Controllers/BicyclesController.cs
│   ├── Controllers/RentalsController.cs
│   ├── Middleware/ExceptionHandlingMiddleware.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── BicycleShop.ServiceDefaults/
│   └── Extensions.cs
│
├── BicycleShop.AppHost/
│   └── Program.cs
│
└── BicycleShop.Tests/
    ├── Services/RentalPricingServiceTests.cs
    └── Services/BicycleServiceTests.cs
```

---

## Task 1: Scaffold the Solution and Projects

**Files:**
- Create: `BicycleShop.sln` and all 7 project files

- [ ] **Step 1: Install .NET Aspire workload**

```bash
dotnet workload install aspire
```

Expected: `Successfully installed workload(s) aspire`

- [ ] **Step 2: Create solution and projects**

Run from `C:\Capstone Project Lessons\CLEAN Architecture`:

```bash
dotnet new sln -n BicycleShop
dotnet new classlib -n BicycleShop.Domain -f net9.0
dotnet new classlib -n BicycleShop.Application -f net9.0
dotnet new classlib -n BicycleShop.Infrastructure -f net9.0
dotnet new webapi -n BicycleShop.Presentation -f net9.0 --no-openapi
dotnet new aspire-servicedefaults -n BicycleShop.ServiceDefaults
dotnet new aspire-apphost -n BicycleShop.AppHost
dotnet new xunit -n BicycleShop.Tests -f net9.0
```

- [ ] **Step 3: Add all projects to the solution**

```bash
dotnet sln BicycleShop.sln add BicycleShop.Domain/BicycleShop.Domain.csproj
dotnet sln BicycleShop.sln add BicycleShop.Application/BicycleShop.Application.csproj
dotnet sln BicycleShop.sln add BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj
dotnet sln BicycleShop.sln add BicycleShop.Presentation/BicycleShop.Presentation.csproj
dotnet sln BicycleShop.sln add BicycleShop.ServiceDefaults/BicycleShop.ServiceDefaults.csproj
dotnet sln BicycleShop.sln add BicycleShop.AppHost/BicycleShop.AppHost.csproj
dotnet sln BicycleShop.sln add BicycleShop.Tests/BicycleShop.Tests.csproj
```

- [ ] **Step 4: Wire project references (enforces dependency rule)**

```bash
dotnet add BicycleShop.Application/BicycleShop.Application.csproj reference BicycleShop.Domain/BicycleShop.Domain.csproj

dotnet add BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj reference BicycleShop.Domain/BicycleShop.Domain.csproj
dotnet add BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj reference BicycleShop.Application/BicycleShop.Application.csproj

dotnet add BicycleShop.Presentation/BicycleShop.Presentation.csproj reference BicycleShop.Application/BicycleShop.Application.csproj
dotnet add BicycleShop.Presentation/BicycleShop.Presentation.csproj reference BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj
dotnet add BicycleShop.Presentation/BicycleShop.Presentation.csproj reference BicycleShop.ServiceDefaults/BicycleShop.ServiceDefaults.csproj

dotnet add BicycleShop.AppHost/BicycleShop.AppHost.csproj reference BicycleShop.Presentation/BicycleShop.Presentation.csproj

dotnet add BicycleShop.Tests/BicycleShop.Tests.csproj reference BicycleShop.Application/BicycleShop.Application.csproj
dotnet add BicycleShop.Tests/BicycleShop.Tests.csproj reference BicycleShop.Domain/BicycleShop.Domain.csproj
```

- [ ] **Step 5: Install NuGet packages**

```bash
# Application
dotnet add BicycleShop.Application/BicycleShop.Application.csproj package AutoMapper --version 13.0.1

# Infrastructure
dotnet add BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.4
dotnet add BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools --version 9.0.4
dotnet add BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0.4
dotnet add BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.4
dotnet add BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj package AutoMapper --version 13.0.1

# Presentation
dotnet add BicycleShop.Presentation/BicycleShop.Presentation.csproj package Swashbuckle.AspNetCore --version 7.2.0

# Tests
dotnet add BicycleShop.Tests/BicycleShop.Tests.csproj package Moq --version 4.20.72
dotnet add BicycleShop.Tests/BicycleShop.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 9.0.4
```

- [ ] **Step 6: Delete template boilerplate files**

```bash
rm BicycleShop.Domain/Class1.cs
rm BicycleShop.Application/Class1.cs
rm BicycleShop.Infrastructure/Class1.cs
rm BicycleShop.Presentation/WeatherForecast.cs 2>/dev/null || true
```

- [ ] **Step 7: Verify solution builds**

```bash
dotnet build BicycleShop.sln
```

Expected: `Build succeeded.`

- [ ] **Step 8: Commit**

```bash
git init
git add .
git commit -m "feat: scaffold solution with 7 projects and dependency rules"
```

---

## Task 2: Domain Layer — Enums and Entities

**Files:**
- Create: `BicycleShop.Domain/Enums/BicycleType.cs`
- Create: `BicycleShop.Domain/Enums/RentalStatus.cs`
- Create: `BicycleShop.Domain/Entities/Bicycle.cs`
- Create: `BicycleShop.Domain/Entities/Rental.cs`

- [ ] **Step 1: Create BicycleType enum**

`BicycleShop.Domain/Enums/BicycleType.cs`:
```csharp
namespace BicycleShop.Domain.Enums;

public enum BicycleType
{
    Road,
    Mountain,
    Hybrid,
    Electric,
    BMX
}
```

- [ ] **Step 2: Create RentalStatus enum**

`BicycleShop.Domain/Enums/RentalStatus.cs`:
```csharp
namespace BicycleShop.Domain.Enums;

public enum RentalStatus
{
    Active,
    Completed,
    Cancelled
}
```

- [ ] **Step 3: Create Bicycle entity**

`BicycleShop.Domain/Entities/Bicycle.cs`:
```csharp
using BicycleShop.Domain.Enums;

namespace BicycleShop.Domain.Entities;

public class Bicycle
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public BicycleType BicycleType { get; set; }
    public decimal PricePerHour { get; set; }
    public decimal PurchasePrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int YearManufactured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
```

- [ ] **Step 4: Create Rental entity**

`BicycleShop.Domain/Entities/Rental.cs`:
```csharp
using BicycleShop.Domain.Enums;

namespace BicycleShop.Domain.Entities;

public class Rental
{
    public Guid Id { get; set; }
    public Guid BicycleId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? TotalCost { get; set; }
    public RentalStatus Status { get; set; } = RentalStatus.Active;

    public Bicycle Bicycle { get; set; } = null!;
}
```

- [ ] **Step 5: Build to verify**

```bash
dotnet build BicycleShop.Domain/BicycleShop.Domain.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add BicycleShop.Domain/
git commit -m "feat: add domain entities and enums"
```

---

## Task 3: Domain Layer — Interfaces and Exceptions

**Files:**
- Create: `BicycleShop.Domain/Interfaces/IBicycleRepository.cs`
- Create: `BicycleShop.Domain/Interfaces/IRentalRepository.cs`
- Create: `BicycleShop.Domain/Exceptions/BicycleNotFoundException.cs`
- Create: `BicycleShop.Domain/Exceptions/BicycleNotAvailableException.cs`
- Create: `BicycleShop.Domain/Exceptions/InvalidRentalOperationException.cs`

- [ ] **Step 1: Create IBicycleRepository**

`BicycleShop.Domain/Interfaces/IBicycleRepository.cs`:
```csharp
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;

namespace BicycleShop.Domain.Interfaces;

public interface IBicycleRepository
{
    Task<IEnumerable<Bicycle>> GetAllAsync();
    Task<Bicycle?> GetByIdAsync(Guid id);
    Task<IEnumerable<Bicycle>> GetByTypeAsync(BicycleType type);
    Task<IEnumerable<Bicycle>> GetAvailableAsync();
    Task<IEnumerable<Bicycle>> SearchAsync(string brand, string? model);
    Task AddAsync(Bicycle bicycle);
    Task UpdateAsync(Bicycle bicycle);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
```

- [ ] **Step 2: Create IRentalRepository**

`BicycleShop.Domain/Interfaces/IRentalRepository.cs`:
```csharp
using BicycleShop.Domain.Entities;

namespace BicycleShop.Domain.Interfaces;

public interface IRentalRepository
{
    Task<Rental?> GetByIdAsync(Guid id);
    Task<IEnumerable<Rental>> GetActiveAsync();
    Task<IEnumerable<Rental>> GetByBicycleIdAsync(Guid bicycleId);
    Task AddAsync(Rental rental);
    Task UpdateAsync(Rental rental);
    Task SaveChangesAsync();
}
```

- [ ] **Step 3: Create domain exceptions**

`BicycleShop.Domain/Exceptions/BicycleNotFoundException.cs`:
```csharp
namespace BicycleShop.Domain.Exceptions;

public class BicycleNotFoundException : Exception
{
    public BicycleNotFoundException(Guid id)
        : base($"Bicycle with ID '{id}' was not found.") { }
}
```

`BicycleShop.Domain/Exceptions/BicycleNotAvailableException.cs`:
```csharp
namespace BicycleShop.Domain.Exceptions;

public class BicycleNotAvailableException : Exception
{
    public BicycleNotAvailableException(Guid id)
        : base($"Bicycle with ID '{id}' is not currently available for rental.") { }
}
```

`BicycleShop.Domain/Exceptions/InvalidRentalOperationException.cs`:
```csharp
namespace BicycleShop.Domain.Exceptions;

public class InvalidRentalOperationException : Exception
{
    public InvalidRentalOperationException(string message) : base(message) { }
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build BicycleShop.Domain/BicycleShop.Domain.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add BicycleShop.Domain/
git commit -m "feat: add domain repository interfaces and domain exceptions"
```

---

## Task 4: Application Layer — DTOs

**Files:**
- Create: `BicycleShop.Application/DTOs/BicycleDto.cs`
- Create: `BicycleShop.Application/DTOs/CreateBicycleDto.cs`
- Create: `BicycleShop.Application/DTOs/UpdateBicycleDto.cs`
- Create: `BicycleShop.Application/DTOs/RentalDto.cs`
- Create: `BicycleShop.Application/DTOs/StartRentalDto.cs`
- Create: `BicycleShop.Application/DTOs/RentalQuoteDto.cs`
- Create: `BicycleShop.Application/DTOs/Auth/RegisterDto.cs`
- Create: `BicycleShop.Application/DTOs/Auth/LoginDto.cs`
- Create: `BicycleShop.Application/DTOs/Auth/AuthResponseDto.cs`

- [ ] **Step 1: Create BicycleDto**

`BicycleShop.Application/DTOs/BicycleDto.cs`:
```csharp
using BicycleShop.Domain.Enums;

namespace BicycleShop.Application.DTOs;

public class BicycleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public BicycleType BicycleType { get; set; }
    public string BicycleTypeName => BicycleType.ToString();
    public decimal PricePerHour { get; set; }
    public decimal PurchasePrice { get; set; }
    public bool IsAvailable { get; set; }
    public int YearManufactured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

- [ ] **Step 2: Create CreateBicycleDto**

`BicycleShop.Application/DTOs/CreateBicycleDto.cs`:
```csharp
using BicycleShop.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BicycleShop.Application.DTOs;

public class CreateBicycleDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    public BicycleType BicycleType { get; set; }

    [Range(0.01, 10000)]
    public decimal PricePerHour { get; set; }

    [Range(0.01, 100000)]
    public decimal PurchasePrice { get; set; }

    [Range(1900, 2100)]
    public int YearManufactured { get; set; }
}
```

- [ ] **Step 3: Create UpdateBicycleDto**

`BicycleShop.Application/DTOs/UpdateBicycleDto.cs`:
```csharp
using BicycleShop.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BicycleShop.Application.DTOs;

public class UpdateBicycleDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    public BicycleType BicycleType { get; set; }

    [Range(0.01, 10000)]
    public decimal PricePerHour { get; set; }

    [Range(0.01, 100000)]
    public decimal PurchasePrice { get; set; }

    public bool IsAvailable { get; set; }

    [Range(1900, 2100)]
    public int YearManufactured { get; set; }
}
```

- [ ] **Step 4: Create RentalDto**

`BicycleShop.Application/DTOs/RentalDto.cs`:
```csharp
using BicycleShop.Domain.Enums;

namespace BicycleShop.Application.DTOs;

public class RentalDto
{
    public Guid Id { get; set; }
    public Guid BicycleId { get; set; }
    public string BicycleName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? TotalCost { get; set; }
    public RentalStatus Status { get; set; }
    public string StatusName => Status.ToString();
}
```

- [ ] **Step 5: Create StartRentalDto and RentalQuoteDto**

`BicycleShop.Application/DTOs/StartRentalDto.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace BicycleShop.Application.DTOs;

public class StartRentalDto
{
    [Required]
    public Guid BicycleId { get; set; }
}
```

`BicycleShop.Application/DTOs/RentalQuoteDto.cs`:
```csharp
namespace BicycleShop.Application.DTOs;

public class RentalQuoteDto
{
    public Guid BicycleId { get; set; }
    public string BicycleName { get; set; } = string.Empty;
    public int Hours { get; set; }
    public decimal BasePrice { get; set; }
    public decimal TypeMultiplier { get; set; }
    public decimal PriceAfterTypeMultiplier { get; set; }
    public string? DiscountCode { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal FinalPrice { get; set; }
}
```

- [ ] **Step 6: Create Auth DTOs**

`BicycleShop.Application/DTOs/Auth/RegisterDto.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace BicycleShop.Application.DTOs.Auth;

public class RegisterDto
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";
}
```

`BicycleShop.Application/DTOs/Auth/LoginDto.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace BicycleShop.Application.DTOs.Auth;

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
```

`BicycleShop.Application/DTOs/Auth/AuthResponseDto.cs`:
```csharp
namespace BicycleShop.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
```

- [ ] **Step 7: Build to verify**

```bash
dotnet build BicycleShop.Application/BicycleShop.Application.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 8: Commit**

```bash
git add BicycleShop.Application/
git commit -m "feat: add application DTOs"
```

---

## Task 5: Application Layer — AutoMapper Profile and Service Interfaces

**Files:**
- Create: `BicycleShop.Application/Mappings/BicycleMappingProfile.cs`
- Create: `BicycleShop.Application/Interfaces/IBicycleService.cs`
- Create: `BicycleShop.Application/Interfaces/IRentalPricingService.cs`

- [ ] **Step 1: Create AutoMapper mapping profile**

`BicycleShop.Application/Mappings/BicycleMappingProfile.cs`:
```csharp
using AutoMapper;
using BicycleShop.Application.DTOs;
using BicycleShop.Domain.Entities;

namespace BicycleShop.Application.Mappings;

public class BicycleMappingProfile : Profile
{
    public BicycleMappingProfile()
    {
        CreateMap<Bicycle, BicycleDto>();
        CreateMap<CreateBicycleDto, Bicycle>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        CreateMap<UpdateBicycleDto, Bicycle>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<Rental, RentalDto>()
            .ForMember(dest => dest.BicycleName,
                opt => opt.MapFrom(src => src.Bicycle != null ? src.Bicycle.Name : string.Empty));
    }
}
```

- [ ] **Step 2: Create IBicycleService**

`BicycleShop.Application/Interfaces/IBicycleService.cs`:
```csharp
using BicycleShop.Application.DTOs;
using BicycleShop.Domain.Enums;

namespace BicycleShop.Application.Interfaces;

public interface IBicycleService
{
    Task<IEnumerable<BicycleDto>> GetAllAsync();
    Task<BicycleDto> GetByIdAsync(Guid id);
    Task<IEnumerable<BicycleDto>> GetByTypeAsync(BicycleType type);
    Task<IEnumerable<BicycleDto>> GetAvailableAsync();
    Task<IEnumerable<BicycleDto>> SearchAsync(string brand, string? model);
    Task<BicycleDto> CreateAsync(CreateBicycleDto dto);
    Task<BicycleDto> UpdateAsync(Guid id, UpdateBicycleDto dto);
    Task DeleteAsync(Guid id);
}
```

- [ ] **Step 3: Create IRentalPricingService**

`BicycleShop.Application/Interfaces/IRentalPricingService.cs`:
```csharp
using BicycleShop.Application.DTOs;

namespace BicycleShop.Application.Interfaces;

public interface IRentalPricingService
{
    Task<RentalQuoteDto> GetRentalQuoteAsync(Guid bicycleId, int hours);
    Task<RentalQuoteDto> ApplyDiscountAsync(Guid bicycleId, int hours, string discountCode);
    Task<RentalDto> StartRentalAsync(Guid bicycleId, string userId);
    Task<RentalDto> CompleteRentalAsync(Guid rentalId);
    Task<IEnumerable<RentalDto>> GetActiveRentalsAsync();
    Task<IEnumerable<RentalDto>> GetRentalHistoryForBicycleAsync(Guid bicycleId);
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build BicycleShop.Application/BicycleShop.Application.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add BicycleShop.Application/
git commit -m "feat: add AutoMapper profile and application service interfaces"
```

---

## Task 6: Application Layer — BicycleService

**Files:**
- Create: `BicycleShop.Application/Services/BicycleService.cs`

- [ ] **Step 1: Write the failing test first**

`BicycleShop.Tests/Services/BicycleServiceTests.cs`:
```csharp
using AutoMapper;
using BicycleShop.Application.DTOs;
using BicycleShop.Application.Mappings;
using BicycleShop.Application.Services;
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using BicycleShop.Domain.Exceptions;
using BicycleShop.Domain.Interfaces;
using Moq;

namespace BicycleShop.Tests.Services;

public class BicycleServiceTests
{
    private readonly Mock<IBicycleRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly BicycleService _service;

    public BicycleServiceTests()
    {
        _repositoryMock = new Mock<IBicycleRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BicycleMappingProfile>());
        _mapper = config.CreateMapper();
        _service = new BicycleService(_repositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        var bicycles = new List<Bicycle>
        {
            new() { Id = Guid.NewGuid(), Name = "Speedster", Brand = "Trek", Model = "X1",
                    BicycleType = BicycleType.Road, PricePerHour = 10m, IsAvailable = true }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(bicycles);

        var result = await _service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Speedster", result.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsBicycleNotFoundException_WhenNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Bicycle?)null);

        await Assert.ThrowsAsync<BicycleNotFoundException>(() => _service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_AddsBicycleAndReturnsDto()
    {
        var dto = new CreateBicycleDto
        {
            Name = "Trail Blazer", Brand = "Giant", Model = "ATX",
            BicycleType = BicycleType.Mountain, PricePerHour = 15m,
            PurchasePrice = 800m, YearManufactured = 2023
        };

        var result = await _service.CreateAsync(dto);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Bicycle>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.Equal("Trail Blazer", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsBicycleNotFoundException_WhenNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Bicycle?)null);

        await Assert.ThrowsAsync<BicycleNotFoundException>(() => _service.DeleteAsync(Guid.NewGuid()));
    }
}
```

- [ ] **Step 2: Run failing tests**

```bash
dotnet test BicycleShop.Tests/BicycleShop.Tests.csproj --filter "BicycleServiceTests"
```

Expected: Build error — `BicycleService` does not exist yet.

- [ ] **Step 3: Implement BicycleService**

`BicycleShop.Application/Services/BicycleService.cs`:
```csharp
using AutoMapper;
using BicycleShop.Application.DTOs;
using BicycleShop.Application.Interfaces;
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using BicycleShop.Domain.Exceptions;
using BicycleShop.Domain.Interfaces;

namespace BicycleShop.Application.Services;

public class BicycleService : IBicycleService
{
    private readonly IBicycleRepository _repository;
    private readonly IMapper _mapper;

    public BicycleService(IBicycleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BicycleDto>> GetAllAsync()
    {
        var bicycles = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<BicycleDto>>(bicycles);
    }

    public async Task<BicycleDto> GetByIdAsync(Guid id)
    {
        var bicycle = await _repository.GetByIdAsync(id)
            ?? throw new BicycleNotFoundException(id);
        return _mapper.Map<BicycleDto>(bicycle);
    }

    public async Task<IEnumerable<BicycleDto>> GetByTypeAsync(BicycleType type)
    {
        var bicycles = await _repository.GetByTypeAsync(type);
        return _mapper.Map<IEnumerable<BicycleDto>>(bicycles);
    }

    public async Task<IEnumerable<BicycleDto>> GetAvailableAsync()
    {
        var bicycles = await _repository.GetAvailableAsync();
        return _mapper.Map<IEnumerable<BicycleDto>>(bicycles);
    }

    public async Task<IEnumerable<BicycleDto>> SearchAsync(string brand, string? model)
    {
        var bicycles = await _repository.SearchAsync(brand, model);
        return _mapper.Map<IEnumerable<BicycleDto>>(bicycles);
    }

    public async Task<BicycleDto> CreateAsync(CreateBicycleDto dto)
    {
        var bicycle = _mapper.Map<Bicycle>(dto);
        await _repository.AddAsync(bicycle);
        await _repository.SaveChangesAsync();
        return _mapper.Map<BicycleDto>(bicycle);
    }

    public async Task<BicycleDto> UpdateAsync(Guid id, UpdateBicycleDto dto)
    {
        var bicycle = await _repository.GetByIdAsync(id)
            ?? throw new BicycleNotFoundException(id);

        _mapper.Map(dto, bicycle);
        await _repository.UpdateAsync(bicycle);
        await _repository.SaveChangesAsync();
        return _mapper.Map<BicycleDto>(bicycle);
    }

    public async Task DeleteAsync(Guid id)
    {
        var bicycle = await _repository.GetByIdAsync(id)
            ?? throw new BicycleNotFoundException(id);

        await _repository.DeleteAsync(bicycle.Id);
        await _repository.SaveChangesAsync();
    }
}
```

- [ ] **Step 4: Run tests — expect pass**

```bash
dotnet test BicycleShop.Tests/BicycleShop.Tests.csproj --filter "BicycleServiceTests" -v normal
```

Expected: `4 passed`

- [ ] **Step 5: Commit**

```bash
git add BicycleShop.Application/ BicycleShop.Tests/
git commit -m "feat: implement BicycleService with tests"
```

---

## Task 7: Application Layer — RentalPricingService (Business Logic)

**Files:**
- Create: `BicycleShop.Application/Services/RentalPricingService.cs`
- Modify: `BicycleShop.Tests/Services/RentalPricingServiceTests.cs`

- [ ] **Step 1: Write the failing tests**

`BicycleShop.Tests/Services/RentalPricingServiceTests.cs`:
```csharp
using AutoMapper;
using BicycleShop.Application.Mappings;
using BicycleShop.Application.Services;
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using BicycleShop.Domain.Exceptions;
using BicycleShop.Domain.Interfaces;
using Moq;

namespace BicycleShop.Tests.Services;

public class RentalPricingServiceTests
{
    private readonly Mock<IBicycleRepository> _bicycleRepoMock;
    private readonly Mock<IRentalRepository> _rentalRepoMock;
    private readonly IMapper _mapper;
    private readonly RentalPricingService _service;

    private readonly Bicycle _electricBicycle = new()
    {
        Id = Guid.NewGuid(), Name = "E-Cruiser", Brand = "Bosch", Model = "E1",
        BicycleType = BicycleType.Electric, PricePerHour = 20m, IsAvailable = true
    };

    private readonly Bicycle _roadBicycle = new()
    {
        Id = Guid.NewGuid(), Name = "Speedster", Brand = "Trek", Model = "X1",
        BicycleType = BicycleType.Road, PricePerHour = 10m, IsAvailable = true
    };

    public RentalPricingServiceTests()
    {
        _bicycleRepoMock = new Mock<IBicycleRepository>();
        _rentalRepoMock = new Mock<IRentalRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BicycleMappingProfile>());
        _mapper = config.CreateMapper();
        _service = new RentalPricingService(_bicycleRepoMock.Object, _rentalRepoMock.Object, _mapper);
    }

    [Fact]
    public async Task GetRentalQuoteAsync_Electric_AppliesOnePointFiveMultiplier()
    {
        _bicycleRepoMock.Setup(r => r.GetByIdAsync(_electricBicycle.Id)).ReturnsAsync(_electricBicycle);

        var quote = await _service.GetRentalQuoteAsync(_electricBicycle.Id, 2);

        // Base: 20 * 2 = 40, multiplier 1.5 = 60, no bulk discount
        Assert.Equal(40m, quote.BasePrice);
        Assert.Equal(1.5m, quote.TypeMultiplier);
        Assert.Equal(60m, quote.FinalPrice);
    }

    [Fact]
    public async Task GetRentalQuoteAsync_Road_AppliesOnePointOneMultiplier()
    {
        _bicycleRepoMock.Setup(r => r.GetByIdAsync(_roadBicycle.Id)).ReturnsAsync(_roadBicycle);

        var quote = await _service.GetRentalQuoteAsync(_roadBicycle.Id, 1);

        // Base: 10 * 1 = 10, multiplier 1.1 = 11
        Assert.Equal(10m, quote.BasePrice);
        Assert.Equal(1.1m, quote.TypeMultiplier);
        Assert.Equal(11m, quote.FinalPrice);
    }

    [Fact]
    public async Task GetRentalQuoteAsync_EightOrMoreHours_AppliesTwentyPercentBulkDiscount()
    {
        _bicycleRepoMock.Setup(r => r.GetByIdAsync(_roadBicycle.Id)).ReturnsAsync(_roadBicycle);

        var quote = await _service.GetRentalQuoteAsync(_roadBicycle.Id, 8);

        // Base: 10 * 8 = 80, multiplier 1.1 = 88, bulk 20% off = 88 * 0.8 = 70.4
        Assert.Equal(80m, quote.BasePrice);
        Assert.Equal(17.60m, quote.DiscountApplied);
        Assert.Equal(70.40m, quote.FinalPrice);
    }

    [Fact]
    public async Task ApplyDiscountAsync_STUDENT10_AppliesTenPercent()
    {
        _bicycleRepoMock.Setup(r => r.GetByIdAsync(_roadBicycle.Id)).ReturnsAsync(_roadBicycle);

        var quote = await _service.ApplyDiscountAsync(_roadBicycle.Id, 2, "STUDENT10");

        // Base: 10 * 2 = 20, multiplier 1.1 = 22, STUDENT10 = 10% off = 2.2 off = 19.8
        Assert.Equal(2.20m, quote.DiscountApplied);
        Assert.Equal(19.80m, quote.FinalPrice);
        Assert.Equal("STUDENT10", quote.DiscountCode);
    }

    [Fact]
    public async Task ApplyDiscountAsync_InvalidCode_ThrowsInvalidRentalOperationException()
    {
        _bicycleRepoMock.Setup(r => r.GetByIdAsync(_roadBicycle.Id)).ReturnsAsync(_roadBicycle);

        await Assert.ThrowsAsync<InvalidRentalOperationException>(
            () => _service.ApplyDiscountAsync(_roadBicycle.Id, 2, "FAKECODE"));
    }

    [Fact]
    public async Task StartRentalAsync_ThrowsBicycleNotAvailableException_WhenNotAvailable()
    {
        var unavailable = new Bicycle { Id = Guid.NewGuid(), IsAvailable = false, BicycleType = BicycleType.Road };
        _bicycleRepoMock.Setup(r => r.GetByIdAsync(unavailable.Id)).ReturnsAsync(unavailable);

        await Assert.ThrowsAsync<BicycleNotAvailableException>(
            () => _service.StartRentalAsync(unavailable.Id, "user-123"));
    }

    [Fact]
    public async Task StartRentalAsync_SetsBicycleUnavailable_AndCreatesActiveRental()
    {
        _bicycleRepoMock.Setup(r => r.GetByIdAsync(_electricBicycle.Id)).ReturnsAsync(_electricBicycle);

        var rental = await _service.StartRentalAsync(_electricBicycle.Id, "user-123");

        _bicycleRepoMock.Verify(r => r.UpdateAsync(It.Is<Bicycle>(b => !b.IsAvailable)), Times.Once);
        _rentalRepoMock.Verify(r => r.AddAsync(It.Is<Rental>(rl => rl.Status == RentalStatus.Active)), Times.Once);
        Assert.Equal(RentalStatus.Active, rental.Status);
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test BicycleShop.Tests/BicycleShop.Tests.csproj --filter "RentalPricingServiceTests"
```

Expected: Build error — `RentalPricingService` does not exist yet.

- [ ] **Step 3: Implement RentalPricingService**

`BicycleShop.Application/Services/RentalPricingService.cs`:
```csharp
using AutoMapper;
using BicycleShop.Application.DTOs;
using BicycleShop.Application.Interfaces;
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using BicycleShop.Domain.Exceptions;
using BicycleShop.Domain.Interfaces;

namespace BicycleShop.Application.Services;

public class RentalPricingService : IRentalPricingService
{
    private readonly IBicycleRepository _bicycleRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IMapper _mapper;

    // Type-based price multipliers — teaches students about domain constants
    private static readonly Dictionary<BicycleType, decimal> TypeMultipliers = new()
    {
        { BicycleType.Electric, 1.5m },
        { BicycleType.Mountain, 1.3m },
        { BicycleType.Road,     1.1m },
        { BicycleType.Hybrid,   1.0m },
        { BicycleType.BMX,      0.9m }
    };

    // Valid discount codes and their percentage values
    private static readonly Dictionary<string, decimal> DiscountCodes = new()
    {
        { "STUDENT10",  0.10m },
        { "WEEKEND15",  0.15m },
        { "BULK20",     0.20m }
    };

    private const int BulkHoursThreshold = 8;
    private const decimal BulkDiscountRate = 0.20m;

    public RentalPricingService(
        IBicycleRepository bicycleRepository,
        IRentalRepository rentalRepository,
        IMapper mapper)
    {
        _bicycleRepository = bicycleRepository;
        _rentalRepository = rentalRepository;
        _mapper = mapper;
    }

    public async Task<RentalQuoteDto> GetRentalQuoteAsync(Guid bicycleId, int hours)
    {
        var bicycle = await _bicycleRepository.GetByIdAsync(bicycleId)
            ?? throw new BicycleNotFoundException(bicycleId);

        var basePrice = CalculateBasePrice(bicycle.PricePerHour, hours);
        var multiplier = GetTypeMultiplier(bicycle.BicycleType);
        var priceAfterMultiplier = ApplyTypeMultiplier(basePrice, multiplier);
        var bulkDiscount = CalculateBulkDiscount(priceAfterMultiplier, hours);
        var finalPrice = RoundToTwoDecimals(priceAfterMultiplier - bulkDiscount);

        return new RentalQuoteDto
        {
            BicycleId = bicycle.Id,
            BicycleName = bicycle.Name,
            Hours = hours,
            BasePrice = basePrice,
            TypeMultiplier = multiplier,
            PriceAfterTypeMultiplier = priceAfterMultiplier,
            DiscountApplied = bulkDiscount,
            FinalPrice = finalPrice
        };
    }

    public async Task<RentalQuoteDto> ApplyDiscountAsync(Guid bicycleId, int hours, string discountCode)
    {
        var quote = await GetRentalQuoteAsync(bicycleId, hours);

        if (!DiscountCodes.TryGetValue(discountCode.ToUpperInvariant(), out var rate))
            throw new InvalidRentalOperationException($"Discount code '{discountCode}' is not valid.");

        if (discountCode.ToUpperInvariant() == "BULK20" && hours < BulkHoursThreshold)
            throw new InvalidRentalOperationException("BULK20 discount requires at least 8 hours.");

        var discountAmount = CalculateDiscountAmount(quote.FinalPrice, rate);
        var finalPrice = RoundToTwoDecimals(quote.FinalPrice - discountAmount);

        quote.DiscountCode = discountCode.ToUpperInvariant();
        quote.DiscountApplied = RoundToTwoDecimals(quote.DiscountApplied + discountAmount);
        quote.FinalPrice = finalPrice;

        return quote;
    }

    public async Task<RentalDto> StartRentalAsync(Guid bicycleId, string userId)
    {
        var bicycle = await _bicycleRepository.GetByIdAsync(bicycleId)
            ?? throw new BicycleNotFoundException(bicycleId);

        if (!bicycle.IsAvailable)
            throw new BicycleNotAvailableException(bicycleId);

        bicycle.IsAvailable = false;
        await _bicycleRepository.UpdateAsync(bicycle);

        var rental = new Rental
        {
            Id = Guid.NewGuid(),
            BicycleId = bicycleId,
            UserId = userId,
            StartTime = DateTime.UtcNow,
            Status = RentalStatus.Active,
            Bicycle = bicycle
        };

        await _rentalRepository.AddAsync(rental);
        await _rentalRepository.SaveChangesAsync();
        await _bicycleRepository.SaveChangesAsync();

        return _mapper.Map<RentalDto>(rental);
    }

    public async Task<RentalDto> CompleteRentalAsync(Guid rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidRentalOperationException($"Rental '{rentalId}' not found.");

        if (rental.Status != RentalStatus.Active)
            throw new InvalidRentalOperationException("Only active rentals can be completed.");

        rental.EndTime = DateTime.UtcNow;
        rental.Status = RentalStatus.Completed;

        var hours = (int)Math.Ceiling((rental.EndTime.Value - rental.StartTime).TotalHours);
        var quote = await GetRentalQuoteAsync(rental.BicycleId, Math.Max(hours, 1));
        rental.TotalCost = quote.FinalPrice;

        var bicycle = await _bicycleRepository.GetByIdAsync(rental.BicycleId)
            ?? throw new BicycleNotFoundException(rental.BicycleId);
        bicycle.IsAvailable = true;

        await _rentalRepository.UpdateAsync(rental);
        await _bicycleRepository.UpdateAsync(bicycle);
        await _rentalRepository.SaveChangesAsync();
        await _bicycleRepository.SaveChangesAsync();

        return _mapper.Map<RentalDto>(rental);
    }

    public async Task<IEnumerable<RentalDto>> GetActiveRentalsAsync()
    {
        var rentals = await _rentalRepository.GetActiveAsync();
        return _mapper.Map<IEnumerable<RentalDto>>(rentals);
    }

    public async Task<IEnumerable<RentalDto>> GetRentalHistoryForBicycleAsync(Guid bicycleId)
    {
        var rentals = await _rentalRepository.GetByBicycleIdAsync(bicycleId);
        return _mapper.Map<IEnumerable<RentalDto>>(rentals);
    }

    // ── Private helper methods — this chain demonstrates multi-method business logic ──

    private static decimal CalculateBasePrice(decimal pricePerHour, int hours)
        => pricePerHour * hours;

    private static decimal GetTypeMultiplier(BicycleType type)
        => TypeMultipliers.TryGetValue(type, out var multiplier) ? multiplier : 1.0m;

    private static decimal ApplyTypeMultiplier(decimal basePrice, decimal multiplier)
        => basePrice * multiplier;

    private static decimal CalculateBulkDiscount(decimal price, int hours)
        => hours >= BulkHoursThreshold ? RoundToTwoDecimals(price * BulkDiscountRate) : 0m;

    private static decimal CalculateDiscountAmount(decimal price, decimal rate)
        => RoundToTwoDecimals(price * rate);

    private static decimal RoundToTwoDecimals(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
```

- [ ] **Step 4: Run tests — expect pass**

```bash
dotnet test BicycleShop.Tests/BicycleShop.Tests.csproj -v normal
```

Expected: `11 passed`

- [ ] **Step 5: Commit**

```bash
git add BicycleShop.Application/ BicycleShop.Tests/
git commit -m "feat: implement RentalPricingService with business logic and tests"
```

---

## Task 8: Infrastructure Layer — Identity User and AppDbContext

**Files:**
- Create: `BicycleShop.Infrastructure/Identity/ApplicationUser.cs`
- Create: `BicycleShop.Infrastructure/Data/AppDbContext.cs`

- [ ] **Step 1: Create ApplicationUser**

`BicycleShop.Infrastructure/Identity/ApplicationUser.cs`:
```csharp
using Microsoft.AspNetCore.Identity;

namespace BicycleShop.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
}
```

- [ ] **Step 2: Create AppDbContext**

`BicycleShop.Infrastructure/Data/AppDbContext.cs`:
```csharp
using BicycleShop.Domain.Entities;
using BicycleShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Bicycle> Bicycles => Set<Bicycle>();
    public DbSet<Rental> Rentals => Set<Rental>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Bicycle>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
            entity.Property(b => b.Brand).IsRequired().HasMaxLength(100);
            entity.Property(b => b.Model).IsRequired().HasMaxLength(100);
            entity.Property(b => b.PricePerHour).HasPrecision(18, 2);
            entity.Property(b => b.PurchasePrice).HasPrecision(18, 2);
            entity.Property(b => b.BicycleType).HasConversion<string>();
        });

        builder.Entity<Rental>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TotalCost).HasPrecision(18, 2);
            entity.Property(r => r.Status).HasConversion<string>();
            entity.HasOne(r => r.Bicycle)
                  .WithMany(b => b.Rentals)
                  .HasForeignKey(r => r.BicycleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Bicycle>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add BicycleShop.Infrastructure/
git commit -m "feat: add ApplicationUser and AppDbContext with Fluent API config"
```

---

## Task 9: Infrastructure Layer — Repositories

**Files:**
- Create: `BicycleShop.Infrastructure/Repositories/BicycleRepository.cs`
- Create: `BicycleShop.Infrastructure/Repositories/RentalRepository.cs`

- [ ] **Step 1: Implement BicycleRepository**

`BicycleShop.Infrastructure/Repositories/BicycleRepository.cs`:
```csharp
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using BicycleShop.Domain.Interfaces;
using BicycleShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.Infrastructure.Repositories;

public class BicycleRepository : IBicycleRepository
{
    private readonly AppDbContext _context;

    public BicycleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Bicycle>> GetAllAsync()
        => await _context.Bicycles.AsNoTracking().ToListAsync();

    public async Task<Bicycle?> GetByIdAsync(Guid id)
        => await _context.Bicycles.FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IEnumerable<Bicycle>> GetByTypeAsync(BicycleType type)
        => await _context.Bicycles.AsNoTracking()
               .Where(b => b.BicycleType == type)
               .ToListAsync();

    public async Task<IEnumerable<Bicycle>> GetAvailableAsync()
        => await _context.Bicycles.AsNoTracking()
               .Where(b => b.IsAvailable)
               .ToListAsync();

    public async Task<IEnumerable<Bicycle>> SearchAsync(string brand, string? model)
    {
        var query = _context.Bicycles.AsNoTracking()
            .Where(b => b.Brand.ToLower().Contains(brand.ToLower()));

        if (!string.IsNullOrWhiteSpace(model))
            query = query.Where(b => b.Model.ToLower().Contains(model.ToLower()));

        return await query.ToListAsync();
    }

    public async Task AddAsync(Bicycle bicycle)
        => await _context.Bicycles.AddAsync(bicycle);

    public Task UpdateAsync(Bicycle bicycle)
    {
        _context.Bicycles.Update(bicycle);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var bicycle = await _context.Bicycles.FindAsync(id);
        if (bicycle is not null)
            _context.Bicycles.Remove(bicycle);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
```

- [ ] **Step 2: Implement RentalRepository**

`BicycleShop.Infrastructure/Repositories/RentalRepository.cs`:
```csharp
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using BicycleShop.Domain.Interfaces;
using BicycleShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.Infrastructure.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Rental?> GetByIdAsync(Guid id)
        => await _context.Rentals.Include(r => r.Bicycle)
               .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<Rental>> GetActiveAsync()
        => await _context.Rentals.Include(r => r.Bicycle).AsNoTracking()
               .Where(r => r.Status == RentalStatus.Active)
               .ToListAsync();

    public async Task<IEnumerable<Rental>> GetByBicycleIdAsync(Guid bicycleId)
        => await _context.Rentals.Include(r => r.Bicycle).AsNoTracking()
               .Where(r => r.BicycleId == bicycleId)
               .OrderByDescending(r => r.StartTime)
               .ToListAsync();

    public async Task AddAsync(Rental rental)
        => await _context.Rentals.AddAsync(rental);

    public Task UpdateAsync(Rental rental)
    {
        _context.Rentals.Update(rental);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add BicycleShop.Infrastructure/
git commit -m "feat: implement BicycleRepository and RentalRepository"
```

---

## Task 10: Infrastructure Layer — Decorator Pattern (Logging Repositories)

**Files:**
- Create: `BicycleShop.Infrastructure/Repositories/Decorators/LoggingBicycleRepository.cs`
- Create: `BicycleShop.Infrastructure/Repositories/Decorators/LoggingRentalRepository.cs`

- [ ] **Step 1: Implement LoggingBicycleRepository**

`BicycleShop.Infrastructure/Repositories/Decorators/LoggingBicycleRepository.cs`:
```csharp
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using BicycleShop.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BicycleShop.Infrastructure.Repositories.Decorators;

// TEACHING NOTE: The Decorator Pattern wraps the real repository and adds
// logging behaviour WITHOUT changing BicycleRepository at all.
// This is the Open/Closed Principle: open for extension, closed for modification.
public class LoggingBicycleRepository : IBicycleRepository
{
    private readonly IBicycleRepository _inner;
    private readonly ILogger<LoggingBicycleRepository> _logger;

    public LoggingBicycleRepository(IBicycleRepository inner, ILogger<LoggingBicycleRepository> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<IEnumerable<Bicycle>> GetAllAsync()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Fetching all bicycles...");
        try
        {
            var result = await _inner.GetAllAsync();
            _logger.LogInformation("Fetched {Count} bicycles in {Ms}ms", result.Count(), sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all bicycles after {Ms}ms", sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<Bicycle?> GetByIdAsync(Guid id)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Fetching bicycle {Id}...", id);
        try
        {
            var result = await _inner.GetByIdAsync(id);
            _logger.LogInformation("Fetched bicycle {Id} in {Ms}ms — Found: {Found}", id, sw.ElapsedMilliseconds, result is not null);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching bicycle {Id} after {Ms}ms", id, sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<IEnumerable<Bicycle>> GetByTypeAsync(BicycleType type)
    {
        _logger.LogInformation("Fetching bicycles of type {Type}...", type);
        var result = await _inner.GetByTypeAsync(type);
        _logger.LogInformation("Fetched {Count} bicycles of type {Type}", result.Count(), type);
        return result;
    }

    public async Task<IEnumerable<Bicycle>> GetAvailableAsync()
    {
        _logger.LogInformation("Fetching available bicycles...");
        var result = await _inner.GetAvailableAsync();
        _logger.LogInformation("Fetched {Count} available bicycles", result.Count());
        return result;
    }

    public async Task<IEnumerable<Bicycle>> SearchAsync(string brand, string? model)
    {
        _logger.LogInformation("Searching bicycles — brand: {Brand}, model: {Model}", brand, model);
        var result = await _inner.SearchAsync(brand, model);
        _logger.LogInformation("Search returned {Count} results", result.Count());
        return result;
    }

    public async Task AddAsync(Bicycle bicycle)
    {
        _logger.LogInformation("Adding bicycle {Name} ({Brand} {Model})", bicycle.Name, bicycle.Brand, bicycle.Model);
        await _inner.AddAsync(bicycle);
    }

    public async Task UpdateAsync(Bicycle bicycle)
    {
        _logger.LogInformation("Updating bicycle {Id}", bicycle.Id);
        await _inner.UpdateAsync(bicycle);
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting bicycle {Id}", id);
        await _inner.DeleteAsync(id);
    }

    public async Task SaveChangesAsync()
    {
        _logger.LogInformation("Saving bicycle changes to database...");
        await _inner.SaveChangesAsync();
        _logger.LogInformation("Bicycle changes saved.");
    }
}
```

- [ ] **Step 2: Implement LoggingRentalRepository**

`BicycleShop.Infrastructure/Repositories/Decorators/LoggingRentalRepository.cs`:
```csharp
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BicycleShop.Infrastructure.Repositories.Decorators;

public class LoggingRentalRepository : IRentalRepository
{
    private readonly IRentalRepository _inner;
    private readonly ILogger<LoggingRentalRepository> _logger;

    public LoggingRentalRepository(IRentalRepository inner, ILogger<LoggingRentalRepository> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Rental?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching rental {Id}...", id);
        var result = await _inner.GetByIdAsync(id);
        _logger.LogInformation("Rental {Id} — Found: {Found}", id, result is not null);
        return result;
    }

    public async Task<IEnumerable<Rental>> GetActiveAsync()
    {
        _logger.LogInformation("Fetching active rentals...");
        var result = await _inner.GetActiveAsync();
        _logger.LogInformation("Found {Count} active rentals", result.Count());
        return result;
    }

    public async Task<IEnumerable<Rental>> GetByBicycleIdAsync(Guid bicycleId)
    {
        _logger.LogInformation("Fetching rental history for bicycle {BicycleId}...", bicycleId);
        var result = await _inner.GetByBicycleIdAsync(bicycleId);
        _logger.LogInformation("Found {Count} rental records for bicycle {BicycleId}", result.Count(), bicycleId);
        return result;
    }

    public async Task AddAsync(Rental rental)
    {
        _logger.LogInformation("Creating rental for bicycle {BicycleId} by user {UserId}", rental.BicycleId, rental.UserId);
        await _inner.AddAsync(rental);
    }

    public async Task UpdateAsync(Rental rental)
    {
        _logger.LogInformation("Updating rental {Id} — Status: {Status}", rental.Id, rental.Status);
        await _inner.UpdateAsync(rental);
    }

    public async Task SaveChangesAsync()
    {
        _logger.LogInformation("Saving rental changes to database...");
        await _inner.SaveChangesAsync();
    }
}
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add BicycleShop.Infrastructure/
git commit -m "feat: add logging decorator repositories (Decorator Pattern + OCP)"
```

---

## Task 11: Infrastructure Layer — JWT Token Service and DbSeeder

**Files:**
- Create: `BicycleShop.Infrastructure/Auth/IJwtTokenService.cs`
- Create: `BicycleShop.Infrastructure/Auth/JwtTokenService.cs`
- Create: `BicycleShop.Infrastructure/Data/DbSeeder.cs`

- [ ] **Step 1: Create IJwtTokenService**

`BicycleShop.Infrastructure/Auth/IJwtTokenService.cs`:
```csharp
using BicycleShop.Infrastructure.Identity;

namespace BicycleShop.Infrastructure.Auth;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
```

- [ ] **Step 2: Implement JwtTokenService**

`BicycleShop.Infrastructure/Auth/JwtTokenService.cs`:
```csharp
using BicycleShop.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BicycleShop.Infrastructure.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"]!));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("fullName", user.FullName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

- [ ] **Step 3: Implement DbSeeder**

`BicycleShop.Infrastructure/Data/DbSeeder.cs`:
```csharp
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using BicycleShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        await context.Database.MigrateAsync();
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedBicyclesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@bicycleshop.com";
        if (await userManager.FindByEmailAsync(adminEmail) is not null) return;

        var admin = new ApplicationUser
        {
            FirstName = "Shop",
            LastName = "Admin",
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    private static async Task SeedBicyclesAsync(AppDbContext context)
    {
        if (await context.Bicycles.AnyAsync()) return;

        var bicycles = new List<Bicycle>
        {
            new() { Id = Guid.NewGuid(), Name = "Speedster Pro", Brand = "Trek", Model = "Domane SL5",
                    BicycleType = BicycleType.Road, PricePerHour = 12.00m, PurchasePrice = 2500m,
                    YearManufactured = 2023, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Trail Blazer", Brand = "Giant", Model = "Talon 29",
                    BicycleType = BicycleType.Mountain, PricePerHour = 15.00m, PurchasePrice = 1800m,
                    YearManufactured = 2022, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "City Glider", Brand = "Specialized", Model = "Sirrus 3.0",
                    BicycleType = BicycleType.Hybrid, PricePerHour = 10.00m, PurchasePrice = 1200m,
                    YearManufactured = 2023, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "E-Cruiser X", Brand = "Bosch", Model = "Active Line",
                    BicycleType = BicycleType.Electric, PricePerHour = 20.00m, PurchasePrice = 4500m,
                    YearManufactured = 2024, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Street Jumper", Brand = "Haro", Model = "BMX Race",
                    BicycleType = BicycleType.BMX, PricePerHour = 8.00m, PurchasePrice = 600m,
                    YearManufactured = 2022, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Alpine Crusher", Brand = "Cannondale", Model = "Trail 5",
                    BicycleType = BicycleType.Mountain, PricePerHour = 18.00m, PurchasePrice = 2200m,
                    YearManufactured = 2024, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Volt Rider", Brand = "Shimano", Model = "Steps E8000",
                    BicycleType = BicycleType.Electric, PricePerHour = 22.00m, PurchasePrice = 5000m,
                    YearManufactured = 2024, IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Urban Arrow", Brand = "Scott", Model = "Sub Cross 30",
                    BicycleType = BicycleType.Hybrid, PricePerHour = 11.00m, PurchasePrice = 1400m,
                    YearManufactured = 2023, IsAvailable = true },
        };

        await context.Bicycles.AddRangeAsync(bicycles);
        await context.SaveChangesAsync();
    }
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add BicycleShop.Infrastructure/
git commit -m "feat: add JwtTokenService and DbSeeder with role and bicycle seed data"
```

---

## Task 12: Infrastructure Layer — DI Extension

**Files:**
- Create: `BicycleShop.Infrastructure/Extensions/InfrastructureExtensions.cs`

- [ ] **Step 1: Create InfrastructureExtensions**

`BicycleShop.Infrastructure/Extensions/InfrastructureExtensions.cs`:
```csharp
using BicycleShop.Infrastructure.Auth;
using BicycleShop.Infrastructure.Data;
using BicycleShop.Infrastructure.Identity;
using BicycleShop.Infrastructure.Repositories;
using BicycleShop.Infrastructure.Repositories.Decorators;
using BicycleShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace BicycleShop.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core — only Infrastructure knows about the database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // JWT Token Service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // TEACHING NOTE: Decorator Pattern wiring.
        // We register the concrete BicycleRepository first, then wrap it in the
        // logging decorator when resolving IBicycleRepository.
        // The controller never sees either class — it only knows IBicycleRepository.
        services.AddScoped<BicycleRepository>();
        services.AddScoped<IBicycleRepository>(provider =>
            new LoggingBicycleRepository(
                provider.GetRequiredService<BicycleRepository>(),
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingBicycleRepository>>()));

        services.AddScoped<RentalRepository>();
        services.AddScoped<IRentalRepository>(provider =>
            new LoggingRentalRepository(
                provider.GetRequiredService<RentalRepository>(),
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingRentalRepository>>()));

        return services;
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add BicycleShop.Infrastructure/
git commit -m "feat: add infrastructure DI extension with decorator wiring"
```

---

## Task 13: Presentation Layer — appsettings and Global Exception Middleware

**Files:**
- Modify: `BicycleShop.Presentation/appsettings.json`
- Create: `BicycleShop.Presentation/appsettings.Development.json`
- Create: `BicycleShop.Presentation/Middleware/ExceptionHandlingMiddleware.cs`

- [ ] **Step 1: Configure appsettings.json**

`BicycleShop.Presentation/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BicycleShopDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Key": "BicycleShop_SuperSecret_Key_2026_MustBe32Chars!!",
    "Issuer": "BicycleShop",
    "Audience": "BicycleShopUsers",
    "ExpiryMinutes": "60"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

`BicycleShop.Presentation/appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "BicycleShop": "Debug"
    }
  }
}
```

- [ ] **Step 2: Create ExceptionHandlingMiddleware**

`BicycleShop.Presentation/Middleware/ExceptionHandlingMiddleware.cs`:
```csharp
using BicycleShop.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BicycleShop.Presentation.Middleware;

// TEACHING NOTE: All HTTP-specific concerns (status codes, ProblemDetails) live
// here in the Presentation layer. The Domain exceptions know nothing about HTTP.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            BicycleNotFoundException      => (StatusCodes.Status404NotFound,       "Resource Not Found"),
            BicycleNotAvailableException  => (StatusCodes.Status409Conflict,        "Bicycle Not Available"),
            InvalidRentalOperationException => (StatusCodes.Status400BadRequest,    "Invalid Operation"),
            _                             => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled exception [{Type}]: {Message}", exception.GetType().Name, exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
```

- [ ] **Step 3: Build Presentation to verify**

```bash
dotnet build BicycleShop.Presentation/BicycleShop.Presentation.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add BicycleShop.Presentation/
git commit -m "feat: add appsettings config and global exception handling middleware"
```

---

## Task 14: Presentation Layer — AuthController

**Files:**
- Create: `BicycleShop.Presentation/Controllers/AuthController.cs`

- [ ] **Step 1: Create AuthController**

`BicycleShop.Presentation/Controllers/AuthController.cs`:
```csharp
using BicycleShop.Application.DTOs.Auth;
using BicycleShop.Infrastructure.Auth;
using BicycleShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BicycleShop.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        var role = dto.Role is "Admin" or "Customer" ? dto.Role : "Customer";
        await _userManager.AddToRoleAsync(user, role);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user, roles);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Expiry = DateTime.UtcNow.AddMinutes(60),
            Email = user.Email!,
            FullName = user.FullName,
            Role = role
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Unauthorized("Invalid credentials.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user, roles);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Expiry = DateTime.UtcNow.AddMinutes(60),
            Email = user.Email!,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? "Customer"
        });
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build BicycleShop.Presentation/BicycleShop.Presentation.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add BicycleShop.Presentation/
git commit -m "feat: add AuthController with register and login endpoints"
```

---

## Task 15: Presentation Layer — BicyclesController

**Files:**
- Create: `BicycleShop.Presentation/Controllers/BicyclesController.cs`

- [ ] **Step 1: Create BicyclesController**

`BicycleShop.Presentation/Controllers/BicyclesController.cs`:
```csharp
using BicycleShop.Application.DTOs;
using BicycleShop.Application.Interfaces;
using BicycleShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BicycleShop.Presentation.Controllers;

// TEACHING NOTE: This controller depends only on IBicycleService (Application layer).
// It has zero knowledge of repositories, EF Core, or the database.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BicyclesController : ControllerBase
{
    private readonly IBicycleService _bicycleService;

    public BicyclesController(IBicycleService bicycleService)
    {
        _bicycleService = bicycleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BicycleDto>>> GetAll()
    {
        var bicycles = await _bicycleService.GetAllAsync();
        return Ok(bicycles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BicycleDto>> GetById(Guid id)
    {
        var bicycle = await _bicycleService.GetByIdAsync(id);
        return Ok(bicycle);
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<BicycleDto>>> GetAvailable()
    {
        var bicycles = await _bicycleService.GetAvailableAsync();
        return Ok(bicycles);
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<BicycleDto>>> GetByType(BicycleType type)
    {
        var bicycles = await _bicycleService.GetByTypeAsync(type);
        return Ok(bicycles);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<BicycleDto>>> Search(
        [FromQuery] string brand,
        [FromQuery] string? model = null)
    {
        if (string.IsNullOrWhiteSpace(brand))
            return BadRequest("Brand is required for search.");

        var bicycles = await _bicycleService.SearchAsync(brand, model);
        return Ok(bicycles);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<BicycleDto>> Create([FromBody] CreateBicycleDto dto)
    {
        var bicycle = await _bicycleService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = bicycle.Id }, bicycle);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<BicycleDto>> Update(Guid id, [FromBody] UpdateBicycleDto dto)
    {
        var bicycle = await _bicycleService.UpdateAsync(id, dto);
        return Ok(bicycle);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _bicycleService.DeleteAsync(id);
        return NoContent();
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build BicycleShop.Presentation/BicycleShop.Presentation.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add BicycleShop.Presentation/
git commit -m "feat: add BicyclesController with CRUD and custom query endpoints"
```

---

## Task 16: Presentation Layer — RentalsController

**Files:**
- Create: `BicycleShop.Presentation/Controllers/RentalsController.cs`

- [ ] **Step 1: Create RentalsController**

`BicycleShop.Presentation/Controllers/RentalsController.cs`:
```csharp
using BicycleShop.Application.DTOs;
using BicycleShop.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BicycleShop.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RentalsController : ControllerBase
{
    private readonly IRentalPricingService _rentalPricingService;

    public RentalsController(IRentalPricingService rentalPricingService)
    {
        _rentalPricingService = rentalPricingService;
    }

    [HttpPost("quote")]
    public async Task<ActionResult<RentalQuoteDto>> GetQuote(
        [FromQuery] Guid bicycleId,
        [FromQuery] int hours)
    {
        if (hours < 1)
            return BadRequest("Hours must be at least 1.");

        var quote = await _rentalPricingService.GetRentalQuoteAsync(bicycleId, hours);
        return Ok(quote);
    }

    [HttpPost("quote/discount")]
    public async Task<ActionResult<RentalQuoteDto>> GetQuoteWithDiscount(
        [FromQuery] Guid bicycleId,
        [FromQuery] int hours,
        [FromQuery] string discountCode)
    {
        if (hours < 1)
            return BadRequest("Hours must be at least 1.");

        var quote = await _rentalPricingService.ApplyDiscountAsync(bicycleId, hours, discountCode);
        return Ok(quote);
    }

    [HttpPost("start")]
    public async Task<ActionResult<RentalDto>> StartRental([FromBody] StartRentalDto dto)
    {
        // Extract the current user's ID from the JWT claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");

        var rental = await _rentalPricingService.StartRentalAsync(dto.BicycleId, userId);
        return CreatedAtAction(nameof(GetActiveRentals), null, rental);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<RentalDto>> CompleteRental(Guid id)
    {
        var rental = await _rentalPricingService.CompleteRentalAsync(id);
        return Ok(rental);
    }

    [HttpGet("active")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<RentalDto>>> GetActiveRentals()
    {
        var rentals = await _rentalPricingService.GetActiveRentalsAsync();
        return Ok(rentals);
    }

    [HttpGet("bicycle/{bicycleId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<RentalDto>>> GetRentalHistory(Guid bicycleId)
    {
        var rentals = await _rentalPricingService.GetRentalHistoryForBicycleAsync(bicycleId);
        return Ok(rentals);
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build BicycleShop.Presentation/BicycleShop.Presentation.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add BicycleShop.Presentation/
git commit -m "feat: add RentalsController with quote, start, complete, and history endpoints"
```

---

## Task 17: Presentation Layer — Program.cs (Full DI Wiring)

**Files:**
- Modify: `BicycleShop.Presentation/Program.cs`

- [ ] **Step 1: Write Program.cs**

`BicycleShop.Presentation/Program.cs`:
```csharp
using BicycleShop.Application.Interfaces;
using BicycleShop.Application.Mappings;
using BicycleShop.Application.Services;
using BicycleShop.Infrastructure.Data;
using BicycleShop.Infrastructure.Extensions;
using BicycleShop.Infrastructure.Identity;
using BicycleShop.Presentation.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Aspire: observability, health checks, service discovery ──
builder.AddServiceDefaults();

// ── Infrastructure (EF Core, Identity, Repositories, JWT) ──
builder.Services.AddInfrastructure(builder.Configuration);

// ── Application Services ──
// TEACHING NOTE: We register the interface → implementation here.
// Any class that needs IBicycleService gets BicycleService injected automatically.
builder.Services.AddScoped<IBicycleService, BicycleService>();
builder.Services.AddScoped<IRentalPricingService, RentalPricingService>();

// ── AutoMapper ──
builder.Services.AddAutoMapper(typeof(BicycleMappingProfile).Assembly);

// ── JWT Authentication ──
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});

// ── Authorization Policies ──
// TEACHING NOTE: Policies are defined once here and applied via [Authorize(Policy = "...")] on controllers.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

builder.Services.AddControllers();

// ── Swagger with JWT support ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BicycleShop API",
        Version = "v1",
        Description = "Clean Architecture teaching project — Bicycle Shop REST API"
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your JWT token. Example: eyJhbGci..."
    };
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Seed database on startup ──
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
    await DbSeeder.SeedAsync(context, roleManager, userManager);
}

// ── Middleware pipeline ──
// TEACHING NOTE: Order matters. Exception middleware must come first to catch all errors.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapDefaultEndpoints(); // Aspire health checks

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

- [ ] **Step 2: Build entire solution**

```bash
dotnet build BicycleShop.sln
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add BicycleShop.Presentation/
git commit -m "feat: wire up Program.cs with full DI, JWT, policies, Swagger, and Aspire"
```

---

## Task 18: Aspire — AppHost and ServiceDefaults

**Files:**
- Modify: `BicycleShop.AppHost/Program.cs`

- [ ] **Step 1: Configure AppHost**

`BicycleShop.AppHost/Program.cs`:
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// TEACHING NOTE: Aspire orchestrates all your services.
// Run this project (AppHost) and it starts the API + opens the dashboard.
var api = builder.AddProject<Projects.BicycleShop_Presentation>("bicycle-shop-api");

builder.Build().Run();
```

- [ ] **Step 2: Verify ServiceDefaults Extensions.cs was generated correctly**

Open `BicycleShop.ServiceDefaults/Extensions.cs` and confirm it has `AddServiceDefaults()` and `MapDefaultEndpoints()` methods. This is auto-generated by the Aspire template — no edits needed.

- [ ] **Step 3: Build the full solution**

```bash
dotnet build BicycleShop.sln
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add BicycleShop.AppHost/
git commit -m "feat: configure Aspire AppHost to orchestrate the API"
```

---

## Task 19: EF Core Migration and First Run

**Files:**
- Create: `BicycleShop.Infrastructure/Migrations/` (auto-generated)

- [ ] **Step 1: Install EF Core tools (if not already installed)**

```bash
dotnet tool install --global dotnet-ef
```

Expected: `dotnet-ef` installed or already exists.

- [ ] **Step 2: Create initial migration**

Run from the solution root:

```bash
dotnet ef migrations add InitialCreate \
  --project BicycleShop.Infrastructure/BicycleShop.Infrastructure.csproj \
  --startup-project BicycleShop.Presentation/BicycleShop.Presentation.csproj \
  --output-dir Migrations
```

Expected: `Done. To undo this action, use 'ef migrations remove'`

- [ ] **Step 3: Verify migration files exist**

```bash
ls BicycleShop.Infrastructure/Migrations/
```

Expected: Files like `20260427xxxxxx_InitialCreate.cs` and `AppDbContextModelSnapshot.cs`

- [ ] **Step 4: Run the application**

```bash
dotnet run --project BicycleShop.Presentation/BicycleShop.Presentation.csproj
```

Expected output includes:
```
info: Microsoft.EntityFrameworkCore.Database.Command[...] Applied migration 'InitialCreate'
info: BicycleShop.Infrastructure.Data.DbSeeder[...] Seeded roles: Admin, Customer
Now listening on: https://localhost:7xxx
```

- [ ] **Step 5: Test the API via Swagger**

Open `https://localhost:7xxx/swagger` in a browser.

Test sequence:
1. `POST /api/auth/login` with `{ "email": "admin@bicycleshop.com", "password": "Admin123!" }` — copy the token
2. Click "Authorize" in Swagger, paste token
3. `GET /api/bicycles` — should return 8 seeded bicycles
4. `GET /api/bicycles/available` — should return all 8 (all available)
5. `GET /api/bicycles/type/Electric` — should return 2 electric bicycles
6. `POST /api/rentals/quote?bicycleId=<id>&hours=2` — should return pricing quote
7. `POST /api/rentals/quote/discount?bicycleId=<id>&hours=2&discountCode=STUDENT10` — discounted quote

- [ ] **Step 6: Commit**

```bash
git add BicycleShop.Infrastructure/Migrations/
git commit -m "feat: add initial EF Core migration"
```

---

## Task 20: Run Aspire Dashboard

- [ ] **Step 1: Run via AppHost**

```bash
dotnet run --project BicycleShop.AppHost/BicycleShop.AppHost.csproj
```

Expected: Aspire Dashboard URL printed to console, e.g. `Dashboard: http://localhost:15174`

- [ ] **Step 2: Verify structured logs appear**

Open the Aspire Dashboard URL in a browser. Make a few API calls via Swagger, then check:
- **Logs** tab — should show structured log entries including decorator messages like `"Fetched 8 bicycles in 12ms"`
- **Traces** tab — should show request traces from Controller → Service → Repository
- **Metrics** tab — request count and duration histograms

- [ ] **Step 3: Final full test run**

```bash
dotnet test BicycleShop.Tests/BicycleShop.Tests.csproj -v normal
```

Expected: `11 passed, 0 failed`

- [ ] **Step 4: Final solution build**

```bash
dotnet build BicycleShop.sln
```

Expected: `Build succeeded. 0 Error(s) 0 Warning(s)`

- [ ] **Step 5: Final commit**

```bash
git add .
git commit -m "feat: complete BicycleShop Clean Architecture teaching project"
```

---

## Summary

| Layer | Key Teaching Concept | Files |
|---|---|---|
| Domain | Entities, interfaces, exceptions — zero dependencies | 9 files |
| Application | Services, DTOs, AutoMapper, business logic | 14 files |
| Infrastructure | EF Core, repositories, decorators, Identity, JWT | 10 files |
| Presentation | Controllers, middleware, DI wiring, Swagger | 6 files |
| Aspire | Observability, structured logging, traces | 2 files |
| Tests | TDD, mocking with Moq, unit testing services | 2 files |

**Endpoints total:** 15 (Auth: 2, Bicycles: 8, Rentals: 6)  
**Tests total:** 11 (BicycleService: 4, RentalPricingService: 7)
