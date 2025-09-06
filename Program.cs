using userinterface.Services;
using Microsoft.EntityFrameworkCore;
using userinterface.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 讀取 CORS 白名單
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 加入 EF Core DbContext
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=26.9.28.191;Port=13306;Database=userdatabase;Uid=ccc;Pwd=bigred;",
        ServerVersion.AutoDetect("Server=26.9.28.191;Port=13306;Database=userdatabase;Uid=ccc;Pwd=bigred;")
    )
);

builder.Services.AddScoped<IUserService, UserService>();

// 加入 JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var cfg = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = cfg["Jwt:Issuer"],
            ValidAudience = cfg["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!))
        };
    });

var app = builder.Build();

app.UseCors("CorsPolicy"); // 套用指定的 CORS 原則
app.UseAuthentication(); // 新增：先驗證
app.UseAuthorization();

// 新增固定回應端點 /114514
app.MapGet("/114514", () => new Dictionary<string, object>
{
    ["name"] = "田所浩二",
    ["age"] = 24,
    ["BloodType"] = "A",
    ["Job"] = "student",
    ["BirthPlace"] = "東京都世田谷區北澤3-23-14",
    ["height"] = 170,
    ["weight"] = 74,
    ["hobbies"] = new[] { "Working out", "Sunbathing" },
    ["favorites"] = new[] { "Black tea", "Ramen" }
});

app.MapControllers();

app.Run();
