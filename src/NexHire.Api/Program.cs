using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Npgsql;
using NexHire.Infrastructure.Persistence;
using NexHire.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Jwt settings (expecting Jwt:SigningKey in config or env)
var signingKey = configuration["Jwt:Key"] ?? configuration["Jwt:SigningKey"] ?? "ReplaceWithSecureSigningKey";
var accessExpiry = configuration["Jwt:AccessTokenExpiryMinutes"] ?? "15";

// DbContext
var connectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration["DATABASE_CONNECTION_STRING"];
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Warning: No DATABASE_CONNECTION_STRING configured. Use Supabase connection string in env or appsettings.");
}
else
{
    // Validate connection string early so we fail fast with a clear error
    try
    {
        var builderNs = new NpgsqlConnectionStringBuilder(connectionString);
        // mask password for logs
        var masked = connectionString.Replace(builderNs.Password ?? string.Empty, "****");
        Console.WriteLine($"Using database: Host={builderNs.Host}; Database={builderNs.Database}; User={builderNs.Username}");
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"The configured DefaultConnection is not a valid Postgres connection string: {ex.Message}", ex);
    }

    builder.Services.AddDbContext<NexHireDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });
}

// Services
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Run Dev Seeder in Development
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    if (env.IsDevelopment())
    {
        var db = scope.ServiceProvider.GetRequiredService<NexHireDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        // Run seeder synchronously and surface original exceptions (avoid AggregateException)
        NexHire.Infrastructure.Persistence.Seed.DevSeeder.SeedAsync(db, hasher).GetAwaiter().GetResult();
    }
}

app.Run();
