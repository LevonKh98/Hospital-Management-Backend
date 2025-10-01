using Microsoft.EntityFrameworkCore;
using BackendApi.Data;
using BackendApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ----- DB CONNECTION (User Secrets in dev, env var in Render/Prod) -----
var cs = builder.Configuration.GetConnectionString("Default")
         ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
         ?? throw new InvalidOperationException("Database connection string not configured.");

// ----- EF Core MySQL -----
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Show Swagger everywhere (helpful for school/testing)
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Redirect to HTTPS only when running locally during development
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// --- Minimal endpoints that always return 200 ---
// (Useful for Render health check; also a quick sanity ping)
app.MapGet("/", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// --- Auto-apply migrations; don't crash app if DB is unreachable ---
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
        // Keep the app running so Render can pass health check while you fix DB/networking
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

// Swagger UI in all environments (handy on Render)
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();
