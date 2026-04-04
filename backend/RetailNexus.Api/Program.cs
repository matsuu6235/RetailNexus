using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RetailNexus.Application.Features.Auth.Login;
using RetailNexus.Application.Interfaces;
using RetailNexus.Infrastructure.Persistence;
using RetailNexus.Infrastructure.Repositories;
using RetailNexus.Api.Authorization;
using RetailNexus.Infrastructure.Security;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000" };

    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// DI
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IJwtService, JwtService>();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"]!;
var issuer = builder.Configuration["Jwt:Issuer"]!;
var audience = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

builder.Services.AddDbContext<RetailNexusDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IStoreTypeRepository, StoreTypeRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IStoreRequestRepository, StoreRequestRepository>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RetailNexus.Api.Middleware.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 開発環境: 初期管理者ユーザーの自動作成
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RetailNexusDbContext>();

    // Admin ロール（マイグレーションで投入済み）
    var adminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var defaultAdminPassword = builder.Configuration["SeedAdmin:Password"]
        ?? throw new InvalidOperationException("SeedAdmin:Password is not configured. Set it via environment variable or appsettings.");

    var existingAdmin = await db.Users.FirstOrDefaultAsync(u => u.LoginId == "admin");
    if (existingAdmin is null)
    {
        var admin = new RetailNexus.Domain.Entities.User(
            "admin", "管理者", "admin@example.com", passwordHasher.Hash(defaultAdminPassword), true, null, null);
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        db.UserRoles.Add(new RetailNexus.Domain.Entities.UserRole
        {
            UserId = admin.UserId,
            RoleId = adminRoleId
        });
        await db.SaveChangesAsync();
    }
    else if (!await db.UserRoles.AnyAsync(ur => ur.UserId == existingAdmin.UserId))
    {
        db.UserRoles.Add(new RetailNexus.Domain.Entities.UserRole
        {
            UserId = existingAdmin.UserId,
            RoleId = adminRoleId
        });

        // 既存の平文パスワードをBCryptハッシュに変換
        if (!existingAdmin.PasswordHash.StartsWith("$2"))
        {
            existingAdmin.PasswordHash = passwordHasher.Hash(existingAdmin.PasswordHash);
        }

        await db.SaveChangesAsync();
    }
}

app.Run();
