using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Npgsql;
using NexHire.Infrastructure.Persistence;
using NexHire.Infrastructure.Services;
using Microsoft.OpenApi.Models;
using NexHire.Application.Interfaces;
using NexHire.Application.Services;
using NexHire.Infrastructure.Persistence.Repositories;
using NexHire.Infrastructure.DocumentExtraction;
using NexHire.Infrastructure.Llm;
using NexHire.Api.Middleware;

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
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IResumeParsingService, ResumeParsingService>();
builder.Services.AddScoped<ITextExtractor, TextExtractor>();

builder.Services.AddHttpClient<ILlmClient, GitHubModelsClient>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://127.0.0.1:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NexHire API", Version = "v1" });

    // Add JWT Bearer Authorization to Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\nExample: 'Bearer eyJhbGci...'",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<OnboardingGuardMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NexHire API V1");
        c.RoutePrefix = "swagger";
    });
}

app.MapControllers();

// Run Dev Seeder in Development
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    if (env.IsDevelopment())
    {
        var db = scope.ServiceProvider.GetService<NexHireDbContext>();
        if (db != null)
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            // Run seeder synchronously and surface original exceptions (avoid AggregateException)
            await NexHire.Infrastructure.Persistence.Seed.DevSeeder.SeedAsync(db, hasher);
        }
    }
}

app.Run();
