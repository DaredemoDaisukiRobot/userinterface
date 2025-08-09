using userinterface.Services;
using Microsoft.EntityFrameworkCore;
using userinterface.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
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

app.UseCors();
app.UseAuthentication(); // 新增：先驗證
app.UseAuthorization();

app.MapControllers();

app.Run();
