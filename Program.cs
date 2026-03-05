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

// Add response caching for dashboard endpoints
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

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
app.UseResponseCaching();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Map health check endpoint
app.MapHealthChecks("/health");

// API Info endpoint (JSON)
app.MapGet("/api", () => Results.Json(new
{
    name = "MigraTrack Pro API",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    status = "healthy",
    timestamp = DateTime.UtcNow,
    endpoints = new[]
    {
        new { method = "GET", path = "/", description = "Welcome page" },
        new { method = "GET", path = "/health", description = "Health check" },
        new { method = "GET", path = "/api", description = "API information" },
        new { method = "GET", path = "/api/Projects", description = "Get all projects" },
        new { method = "GET", path = "/api/ServerData", description = "Get server data" },
        new { method = "POST", path = "/api/Auth/login", description = "User login" }
    }
}));

// Root endpoint - Welcome message
app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>MigraTrack Pro - API</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        .container {
            background: white;
            border-radius: 20px;
            padding: 40px;
            max-width: 600px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }
        h1 {
            color: #667eea;
            margin-bottom: 10px;
            font-size: 2.5em;
        }
        .subtitle {
            color: #666;
            margin-bottom: 30px;
            font-size: 1.1em;
        }
        .status {
            background: #10b981;
            color: white;
            padding: 10px 20px;
            border-radius: 25px;
            display: inline-block;
            margin-bottom: 30px;
            font-weight: 600;
        }
        .info-box {
            background: #f3f4f6;
            padding: 20px;
            border-radius: 10px;
            margin-bottom: 20px;
        }
        .info-item {
            display: flex;
            justify-content: space-between;
            padding: 10px 0;
            border-bottom: 1px solid #e5e7eb;
        }
        .info-item:last-child { border-bottom: none; }
        .label {
            font-weight: 600;
            color: #374151;
        }
        .value {
            color: #6b7280;
        }
        .endpoints {
            margin-top: 20px;
        }
        .endpoint {
            background: white;
            padding: 15px;
            margin: 10px 0;
            border-radius: 8px;
            border-left: 4px solid #667eea;
        }
        .endpoint-url {
            color: #667eea;
            font-family: 'Courier New', monospace;
            font-weight: 600;
        }
        .endpoint-desc {
            color: #6b7280;
            font-size: 0.9em;
            margin-top: 5px;
        }
        .footer {
            text-align: center;
            margin-top: 30px;
            color: #9ca3af;
            font-size: 0.9em;
        }
        a { color: #667eea; text-decoration: none; }
        a:hover { text-decoration: underline; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚀 MigraTrack Pro</h1>
        <div class='subtitle'>Data Migration Tracking API</div>
        <div class='status'>✓ API is Running</div>

        <div class='info-box'>
            <div class='info-item'>
                <span class='label'>Environment:</span>
                <span class='value'>" + app.Environment.EnvironmentName + @"</span>
            </div>
            <div class='info-item'>
                <span class='label'>Version:</span>
                <span class='value'>.NET 8.0</span>
            </div>
            <div class='info-item'>
                <span class='label'>Status:</span>
                <span class='value' style='color: #10b981; font-weight: 600;'>Healthy</span>
            </div>
        </div>

        <div class='endpoints'>
            <h3 style='color: #374151; margin-bottom: 15px;'>Available Endpoints:</h3>

            <div class='endpoint'>
                <div class='endpoint-url'>GET /health</div>
                <div class='endpoint-desc'>Health check endpoint for monitoring</div>
            </div>

            <div class='endpoint'>
                <div class='endpoint-url'>GET /api/Projects</div>
                <div class='endpoint-desc'>Get all migration projects</div>
            </div>

            <div class='endpoint'>
                <div class='endpoint-url'>GET /api/ServerData</div>
                <div class='endpoint-desc'>Get all server configurations</div>
            </div>

            <div class='endpoint'>
                <div class='endpoint-url'>POST /api/Auth/login</div>
                <div class='endpoint-desc'>User authentication endpoint</div>
            </div>
        </div>

        <div class='footer'>
            <p>Powered by <strong>MigraTrack Pro</strong></p>
            <p>Deployed on Render • Connected to Vercel Frontend</p>
        </div>
    </div>
</body>
</html>
", "text/html"));

app.MapControllers();

// Display welcome message
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var environment = app.Environment;

var deploymentUrl = Environment.GetEnvironmentVariable("RENDER_EXTERNAL_URL") ?? $"http://localhost:{port}";

Console.WriteLine();
Console.WriteLine("=======================================");
Console.WriteLine("   MigraTrack Pro - Backend API");
Console.WriteLine("=======================================");
Console.WriteLine($"Environment: {environment.EnvironmentName}");
Console.WriteLine($"Application started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Port: {port}");
Console.WriteLine();
Console.WriteLine("URLs:");
Console.WriteLine($"  - API: {deploymentUrl}");
Console.WriteLine($"  - Health Check: {deploymentUrl}/health");
if (environment.IsDevelopment())
{
    Console.WriteLine($"  - Swagger UI: {deploymentUrl}/swagger");
}
Console.WriteLine();
Console.WriteLine("Press Ctrl+C to shutdown");
Console.WriteLine("=======================================");
Console.WriteLine();

logger.LogInformation("MigraTrack Pro API started successfully");
logger.LogInformation("Environment: {Environment}", environment.EnvironmentName);
logger.LogInformation("Deployment URL: {DeploymentUrl}", deploymentUrl);

app.Run();
