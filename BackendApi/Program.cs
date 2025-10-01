using Microsoft.EntityFrameworkCore;
using BackendApi.Data;
using BackendApi.Models;


var builder = WebApplication.CreateBuilder(args);

// ---- DB CONNECTION (reads from User Secrets in dev, or env var in Render) ----
var cs = builder.Configuration.GetConnectionString("Default")
         ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
         ?? throw new InvalidOperationException("Database connection string not configured.");

// ---- EF Core MySQL ----
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs)));     // ✅ add

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// OPTIONAL: auto-apply migrations on startup (safe to keep)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();  // applies pending migrations
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database migration failed at startup.");
        // Keep running so /health works while you fix DB networking
    }
}





// List all patients
app.MapGet("/api/patients", async (AppDbContext db) =>
    await db.Patients.AsNoTracking().ToListAsync());

// Create a patient
app.MapPost("/api/patients", async (AppDbContext db, Patient p) =>
{
    db.Patients.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/api/patients/{p.Id}", p);
});



// Create appointment with simple conflict check per doctor
app.MapPost("/api/appointments", async (AppDbContext db, Appointment a) =>
{
    // end time for the new appointment
    var end = a.StartsAt.AddMinutes(a.DurationMinutes);

    // conflict = same doctor, any overlap
    var conflict = await db.Appointments.AnyAsync(x =>
        x.DoctorId == a.DoctorId &&
        x.StartsAt < end &&
        a.StartsAt < x.StartsAt.AddMinutes(x.DurationMinutes));

    if (conflict) return Results.BadRequest("Time conflict for this doctor.");

    db.Appointments.Add(a);
    await db.SaveChangesAsync();
    return Results.Created($"/api/appointments/{a.Id}", a);
});



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();

app.MapControllers();

// Simple health endpoint for Render
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));  // ✅ add

app.Run();
