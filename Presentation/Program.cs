using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Presentation.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Infrastructure (EF Core, Identity, Repositories with Decorator wiring, JWT) ──
// Everything database/auth related is registered inside AddInfrastructure().
// Presentation only calls the extension — it never references AppDbContext directly.
builder.Services.AddInfrastructure(builder.Configuration);

// ── 2. Application Services ──────────────────────────────────────────────────
// TEACHING NOTE: We register Interface → Implementation here.
// Any class that asks for IBicycleService receives BicycleService automatically.
// This is Dependency Injection in action.
builder.Services.AddScoped<IBicycleService, BicycleService>();
builder.Services.AddScoped<IRentalPricingService, RentalPricingService>();

// ── 3. AutoMapper ────────────────────────────────────────────────────────────
// Scans the Application assembly for all Profile classes (BicycleMappingProfile)
builder.Services.AddAutoMapper(typeof(BicycleMappingProfile).Assembly);

// ── 4. JWT Authentication ────────────────────────────────────────────────────
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

// ── 5. Authorization Policies (RBAC) ─────────────────────────────────────────
// Policies are defined once here. Controllers apply them with [Authorize(Policy = "AdminOnly")].
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",    policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

// ── 6. Controllers + Swagger ─────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BicycleShop API",
        Version = "v1",
        Description = "Clean Architecture teaching project — Bicycle Shop REST API"
    });

    // Add JWT Bearer support to Swagger UI so we can test protected endpoints
    var jwtScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Paste your JWT token here. Get one from POST /api/auth/login"
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

// ── 7. Seed the database on startup ──────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DbSeeder.SeedAsync(context, roleManager, userManager);
}

// ── 8. Middleware pipeline ────────────────────────────────────────────────────
// Order matters: exception middleware must be FIRST so it catches everything below it.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();  // Who are you?
app.UseAuthorization();   // What are you allowed to do?
app.MapControllers();

app.Run();
