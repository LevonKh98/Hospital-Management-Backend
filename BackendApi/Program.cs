using System.Text;
using BackendApi.Data;
using BackendApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ----- Render dynamic port (Render sets PORT) -----
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// ----- Connection string (prefer env, fallback to appsettings) -----
string? envCs = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
string? fileCs = builder.Configuration.GetConnectionString("Default");
string cs = !string.IsNullOrWhiteSpace(envCs)
    ? envCs
    : !string.IsNullOrWhiteSpace(fileCs)
        ? fileCs
        : throw new InvalidOperationException("Database connection string not configured.");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs)));

// ----- Controllers -----
builder.Services.AddControllers();

// ----- Swagger (always enabled for this class project) -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BackendApi", Version = "v1" });

    // JWT auth button in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ----- Password hasher -----
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// ----- JWT Auth (reads Jwt:* from configuration/env) -----
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not configured.");
var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience not configured.");
var signingKeyS = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey not configured.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKeyS));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

// ----- CORS (reads Cors:AllowedOrigins or uses sensible localhost defaults) -----
var allowedOrigins = builder.Configuration
    .GetValue<string>("Cors:AllowedOrigins")?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "http://localhost:5173", "http://localhost:19006", "http://localhost:3000" };

builder.Services.AddCors(o => o.AddPolicy("AllowApp", p =>
    p.WithOrigins(allowedOrigins)
     .AllowAnyHeader()
     .AllowAnyMethod()
));

// ----- Authorization policies -----
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("Clinician", p => p.RequireRole("Doctor", "Nurse"));
    options.AddPolicy("Reception", p => p.RequireRole("Receptionist"));
});

var app = builder.Build();

// ----- Auto-apply EF migrations (helpful on Render) -----
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database migration failed at startup. Continuing so /health works.");
    }
}

// ----- Health endpoints -----
app.MapGet("/", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// ----- HTTPS redirect only in Development (keeps Render happy) -----
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ----- Middleware order -----
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowApp");      // CORS must be before auth for preflight
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
