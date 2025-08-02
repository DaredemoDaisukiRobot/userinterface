using userinterface.Services;
using Microsoft.EntityFrameworkCore;
using userinterface.Models;

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

var app = builder.Build();

app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
