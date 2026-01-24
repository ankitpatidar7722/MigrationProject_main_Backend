using Microsoft.EntityFrameworkCore;
using MigraTrackAPI.Data;
using MigraTrackAPI.Services;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure port from environment (Render uses PORT env var)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configure Kestrel for production
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false; // Security: Don't expose server info
});

// Add services to the container
// Register Services
builder.Services.AddScoped<ICustomizationService, CustomizationService>();
// builder.Services.AddScoped<IProjectService, ProjectService>(); // Example if needed

// Configure JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add response compression for production
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MigraTrackDbContext>();

// Configure SQL Server Database
builder.Services.AddDbContext<MigraTrackDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IDataTransferService, DataTransferService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IIssueService, IssueService>();
//builder.Services.AddScoped<ICustomizationService, CustomizationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IServerDataService, ServerDataService>();
builder.Services.AddScoped<IDatabaseDetailService, DatabaseDetailService>();

// Configure CORS to allow frontend access
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Development fallback
            policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:3001", "https://localhost:3001")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Configure forwarded headers for reverse proxy support (nginx, IIS, etc.)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Enable response compression
app.UseResponseCompression();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production: Add security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
        await next();
    });

    // Enable HSTS
    var hstsMaxAge = builder.Configuration.GetValue<int>("Security:HstsMaxAge", 31536000);
    app.UseHsts();
}

// Enable HTTPS redirection in production
if (builder.Configuration.GetValue<bool>("Security:RequireHttps", false))
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseStaticFiles(); // Enable static files for uploads
app.UseAuthentication();
app.UseAuthorization();

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Display welcome message
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var environment = app.Environment;

Console.WriteLine();
Console.WriteLine("=======================================");
Console.WriteLine("   MigraTrack Pro - Backend API");
Console.WriteLine("=======================================");
Console.WriteLine($"Environment: {environment.EnvironmentName}");
Console.WriteLine($"Application started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine();
Console.WriteLine("URLs:");
Console.WriteLine("  - API: http://localhost:5000");
if (environment.IsDevelopment())
{
    Console.WriteLine("  - Swagger UI: http://localhost:5000/swagger");
}
Console.WriteLine("  - Health Check: http://localhost:5000/health");
Console.WriteLine();
Console.WriteLine("Press Ctrl+C to shutdown");
Console.WriteLine("=======================================");
Console.WriteLine();

logger.LogInformation("MigraTrack Pro API started successfully");
logger.LogInformation("Environment: {Environment}", environment.EnvironmentName);

app.Run();
