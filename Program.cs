using userinterface.Services;
using Microsoft.EntityFrameworkCore;
using userinterface.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 監聽所有網卡的 8964 port
builder.WebHost.UseUrls("http://0.0.0.0:8964");

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

// 加入 EF Core DbContext（分別對應兩個資料庫）
builder.Services.AddDbContext<UserDbContext>(options =>
{
    var csUser = builder.Configuration.GetConnectionString("UserDb"); // userdatabase
    options.UseMySql(csUser, ServerVersion.AutoDetect(csUser));
});
builder.Services.AddDbContext<MemoryDbContext>(options =>
{
    var csMemory = builder.Configuration.GetConnectionString("DefaultConnection"); // memory
    options.UseMySql(csMemory, ServerVersion.AutoDetect(csMemory));
});

// 服務註冊（若已存在可忽略）
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMemoryService, MemoryService>();

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
