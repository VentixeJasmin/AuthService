using AuthService.Data;
using Microsoft.AspNetCore.Identity;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using AuthService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<UserService>();
builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("VentixeDatabaseConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://jolly-ocean-090980503.6.azurestaticapps.net", "http://localhost:5173") // <-- Put your real frontend URL here
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // <-- Important for cookies
    });
});

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

//Cookie config below by Claude AI 
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/auth/signin";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Events.OnRedirectToLogin = context =>
    {
        // Return 401 instead of redirect for API calls
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();