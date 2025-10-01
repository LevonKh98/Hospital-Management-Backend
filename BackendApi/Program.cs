using Microsoft.EntityFrameworkCore;
using BackendApi.Data;
using BackendApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Bind to Render's dynamic port if provided (avoids port-binding issues) ---
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// --- Connection string: prefer env var, then appsettings/User Secrets; ignore empty values ---
string? envCs  = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
string? fileCs = builder.Configuration.GetConnectionString("Default");

string cs = !string.IsNullOrWhiteSpace(envCs)
    ? envCs
    : !string.IsNullOrWhiteSpace(fileCs)
        ? fileCs
        : throw new InvalidOperationException("Database connection string not configured.");

// --- EF Core MySQL ---
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use HTTPS redirection only in Development (Render terminates TLS for you)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// --- Health endpoints (for Render health checks and quick sanity ping) ---
app.MapGet("/",       () => Results.Ok(new { status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// --- Auto-apply migrations; don't bring the app down if DB isn't reachable yet ---
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
        // Keep the app running so Render can pass health check while you fix DB/networking.
    }
}

// --- Minimal API endpoints ---
app.MapGet("/api/patients", async (AppDbContext db) =>
    await db.Patients.AsNoTracking().ToListAsync());

app.MapPost("/api/patients", async (AppDbContext db, Patient p) =>
{
    db.Patients.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/api/patients/{p.Id}", p);
});

app.MapPost("/api/appointments", async (AppDbContext db, Appointment a) =>
{
    var newEnd = a.StartsAt.AddMinutes(a.DurationMinutes);

    var conflict = await db.Appointments.AnyAsync(x =>
        x.DoctorId == a.DoctorId &&
        x.StartsAt < newEnd &&
        a.StartsAt < x.StartsAt.AddMinutes(x.DurationMinutes));

    if (conflict)
        return Results.BadRequest("Time conflict for this doctor.");

    db.Appointments.Add(a);
    await db.SaveChangesAsync();
    return Results.Created($"/api/appointments/{a.Id}", a);
});

// Swagger everywhere (handy for school/testing and on Render)
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();
