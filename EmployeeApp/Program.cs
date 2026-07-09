using EmployeeApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddControllers();

// InMemory DB — koi SQL Server install nahi chahiye
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("EmployeeDb"));

// Swagger — API documentation + browser se test karo
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "EmployeeApp API",
        Version     = "v1",
        Description = "Employee & Department CRUD — DevOps Demo Project"
    });
    // XML comments se Swagger mein descriptions aati hain
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// CORS — Angular ya koi bhi frontend se access ke liye
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── App Pipeline ──────────────────────────────────────────────────────────────

var app = builder.Build();

// Swagger — dev aur prod dono mein ON rakho (demo ke liye)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmployeeApp v1");
    c.RoutePrefix = string.Empty; // Root URL pe Swagger khule: http://localhost:8080
});

app.UseCors();
app.MapControllers();

// Health check — Docker aur Azure ke liye
app.MapGet("/health", () => Results.Ok(new
{
    status  = "healthy",
    service = "EmployeeApp",
    time    = DateTime.UtcNow
}));

// Seed data — startup pe dummy data daalo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    AppDbContext.Seed(db);
}

app.Run();

// Tests ke liye partial class zaroori hai
public partial class Program { }
