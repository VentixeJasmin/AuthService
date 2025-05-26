using AuthService.Data;
using Microsoft.AspNetCore.Identity;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using AuthService.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<UserService>();

builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("AuthDatabaseConnection")));

builder.Services.AddIdentity<UserEntity, IdentityRole>(x =>
{
    x.Password.RequiredLength = 8;
    x.Password.RequireNonAlphanumeric = true;
    x.Password.RequireDigit = true;
    x.User.RequireUniqueEmail = true;
    x.SignIn.RequireConfirmedEmail = true;
    x.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
