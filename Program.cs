using Microsoft.EntityFrameworkCore;
using RacedayAPI.Data;
using RacedayAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Register the DbContext
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the AuthService
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<RacedayAPI.Services.BlobService>();

// Add Session Support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Session";
});

// Add Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configured Swagger Generation with API Documentation and XML Comments
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "A RESTful API for the RaceDay event management system. " +
                      "Supports Organisers (creating events) and Participants (enrolling in events). " +
                      "Authentication uses session-based cookies (log in via /api/auth/login first).",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "PROG6212 Student",
            Email = "student@raceday.com"
        }
    });

    // Include XML comments in Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Must come BEFORE UseAuthorization
app.UseSession();
app.UseAuthorization();

app.MapControllers();

app.Run();

// make the implicit Program class public so that integration tests can access it
public partial class Program { }