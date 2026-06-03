// Program.cs
using CarPartsInventory.API.Models;
using CarPartsInventory.API.Services;
using CarPartsInventory.API.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure JWT settings
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("Jwt").Bind(jwtSettings);

// 🔍 添加调试输出
Console.WriteLine($"🔍 JWT Config Loaded:");
Console.WriteLine($"   SecretKey: {jwtSettings.SecretKey?.Substring(0, 10)}...");
Console.WriteLine($"   Issuer: {jwtSettings.Issuer}");
Console.WriteLine($"   Audience: {jwtSettings.Audience}");
Console.WriteLine($"   ExpirationMinutes: {jwtSettings.AccessTokenExpirationMinutes}");

builder.Services.AddSingleton(jwtSettings);

// Register all services
builder.Services.AddSingleton(typeof(IJsonFileService<>), typeof(JsonFileService<>));
builder.Services.AddScoped<ICarPartService, CarPartService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IVehicleHierarchyService, VehicleHierarchyService>();
builder.Services.AddScoped<IMainCategoryService, MainCategoryService>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
builder.Services.AddScoped<IPartService, PartService>();

builder.Services.AddSingleton<JwtTokenGenerator>();
builder.Services.AddSingleton<EmailService>();

// Configure JSON storage
builder.Services.Configure<JsonStorageOptions>(builder.Configuration.GetSection("JsonStorage"));

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

        // 🔍 添加详细的事件日志
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ JWT Authentication Failed: {context.Exception.GetType().Name}");
                Console.WriteLine($"   Message: {context.Exception.Message}");
                if (context.Exception.InnerException != null)
                {
                    Console.WriteLine($"   Inner: {context.Exception.InnerException.Message}");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"✅ JWT Token Validated!");
                Console.WriteLine($"   User: {context.Principal?.Identity?.Name}");
                Console.WriteLine($"   Claims: {context.Principal?.Claims.Count()}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].ToString();
                Console.WriteLine($"🔍 Token Received: {token?.Substring(0, Math.Min(50, token?.Length ?? 0))}...");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"⚠️ Authentication Challenge:");
                Console.WriteLine($"   Error: {context.Error}");
                Console.WriteLine($"   ErrorDescription: {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
    options.AddPolicy("Operator", policy => policy.RequireRole("operator", "admin"));
    options.AddPolicy("User", policy => policy.RequireRole("user", "operator", "admin"));
});
// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Car Parts Inventory API", 
        Version = "v1",
        Description = "API for managing car parts inventory, vehicle hierarchies, and user authentication"
    });

    // 配置 Swagger 支持 JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
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

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
    
//}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Car Parts Inventory API v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root
});
//app.UseHttpsRedirection();

// 🔥 中间件顺序很重要
app.UseCors();
app.UseRouting();
app.UseAuthentication();  // ← 必须在 UseAuthorization 之前
app.UseAuthorization();

app.MapControllers();

// 确保数据目录存在
var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
Console.WriteLine($"📁 Data directory path: {dataPath}");
Console.WriteLine($"📁 Directory exists: {Directory.Exists(dataPath)}");
if (!Directory.Exists(dataPath))
{
    Directory.CreateDirectory(dataPath);
}

app.Run();