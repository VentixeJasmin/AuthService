using AuthService.Data;
using Microsoft.AspNetCore.Identity;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using AuthService.Services;
using Azure.Messaging.ServiceBus;
using AuthService.ServiceBus;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<UserService>();

//Got help with the ServiceBus configuring by Claude AI
builder.Services.AddSingleton<ServiceBusClient>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("ServiceBus");
    Console.WriteLine($"ServiceBus connection string: {connectionString}"); // Debug line

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("ServiceBus connection string is null or empty");
    }

    return new ServiceBusClient(connectionString);
});

builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("VentixeDatabaseConnection")));

builder.Services.AddHostedService<AccountCreatedMessageHandler>();

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