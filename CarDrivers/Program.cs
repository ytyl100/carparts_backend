using ChargingStationManagement.API.BackgroundServices;
using ChargingStationManagement.API.Middleware;
using ChargingStationManagement.Application.ApplicationServices;
using ChargingStationManagement.Application.Interfaces;
using ChargingStationManagement.Infrastructure.Extensions;
using ChargingStationManagement.Infrastructure.Persistence;
using ChargingStationManagement.Services.ApplicationServices;
using ChargingStationManagement.Services.Interfaces;
using ChargingStationManagement.Services.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("🚀 Charging Station Management API Starting...");

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// 🔥 Configure Database (提供默认值，失败不中断)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=charging_station.db";

Console.WriteLine($"📊 Database: {connectionString}");

try
{
    builder.Services.AddChargingDataLayer(connectionString);
    Console.WriteLine("✅ Database configured");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Database error: {ex.Message}");
}

// 🔥 Configure Redis (失败时不注册，仅使用 MemoryCache)
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
Console.WriteLine($"📡 Redis: {redisConnection}");

var redisAvailable = false;
try
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var configuration = ConfigurationOptions.Parse(redisConnection, true);
        configuration.AbortOnConnectFail = false;
        configuration.ConnectTimeout = 2000;
        configuration.SyncTimeout = 1000;
        configuration.ConnectRetry = 1;
        
        var multiplexer = ConnectionMultiplexer.Connect(configuration);
        logger.LogInformation("✅ Redis connected");
        return multiplexer;
    });
    redisAvailable = true;
    Console.WriteLine("✅ Redis configured");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Redis unavailable: {ex.Message}");
    Console.WriteLine("⚠️ Using MemoryCache only");
}

builder.Services.AddMemoryCache();
Console.WriteLine("✅ MemoryCache configured");

// Add HttpClient
builder.Services.AddHttpClient<IApiThirdPartyIntegrationService, ThirdPartyIntegrationService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// 🔥 Configure JWT (提供默认值)
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? "YourSuperSecretKeyHereAtLeast32CharactersLongForHS256Algorithm";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ChargingStationAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ChargingStationClient";

Console.WriteLine($"🔐 JWT Issuer: {jwtIssuer}, Audience: {jwtAudience}");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
    options.AddPolicy("Operator", policy => policy.RequireRole("operator", "admin"));
    options.AddPolicy("User", policy => policy.RequireRole("user", "operator", "admin"));
});

Console.WriteLine("✅ Authentication configured");

// Register Services
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IStationManagementService, StationManagementService>();
builder.Services.AddScoped<IApiChargingService, ChargingService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IApiThirdPartyIntegrationService, ThirdPartyIntegrationService>();
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
Console.WriteLine("✅ Services registered");

//builder.Services.AddHostedService<ThirdPartySyncService>(); 
//builder.Services.AddHostedService<SessionMonitorService>();  
//Console.WriteLine("✅ Background services registered");

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Charging Station Management API",
        Version = "v1",
        Description = "API for managing electric vehicle charging stations"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

Console.WriteLine("✅ Swagger configured");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

Console.WriteLine("🏗️ Application built successfully");
Console.WriteLine($"🌍 Environment: {app.Environment.EnvironmentName}");

// 🔥 关键修复：始终启用 Swagger（像 webApi 项目一样）
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Charging Station Management API v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root
});
Console.WriteLine("✅ Swagger UI enabled at http://localhost:5274");

// 🔥 禁用 HTTPS 重定向 (开发环境)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 🔥 中间件顺序（与 webApi 一致）
app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 在路由之后，端点之前
app.UseMiddleware<EvcsProtocolMiddleware>();

app.MapControllers();

Console.WriteLine("✅ Middleware configured");

// 🔥 Initialize Database (失败不中断启动)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // 确保数据库已创建
        dbContext.Database.EnsureCreated();
        
        // 尝试初始化种子数据
        SeedData.Initialize(dbContext);
        
        Console.WriteLine("✅ Database initialized");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Database initialization failed: {ex.Message}");
}

Console.WriteLine("================================================");
Console.WriteLine("📍 API URL: http://localhost:5274");
Console.WriteLine("📚 Swagger: http://localhost:5274");
Console.WriteLine("================================================");

app.Run();