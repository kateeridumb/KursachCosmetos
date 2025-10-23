using CosmeticShopAPI.Models;
using CosmeticShopAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<CosmeticsShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<BusinessMetricsService>();
builder.Services.AddScoped<IEmailService, EmailService>(); // Добавляем EmailService

var app = builder.Build();

Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var metricsService = scope.ServiceProvider.GetRequiredService<BusinessMetricsService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    while (true)
    {
        try
        {
            metricsService.UpdateMetrics();
            logger.LogInformation("Metrics updated successfully (Program.cs loop)");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating metrics");
        }

        await Task.Delay(TimeSpan.FromSeconds(30)); 
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpMetrics();
app.MapMetrics();
app.MapControllers();

app.Run();