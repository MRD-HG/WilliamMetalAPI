using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.IO;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Only configure Kestrel with a PFX if the file exists (prevents startup crash when file is missing)
var certPath = Path.Combine(builder.Environment.ContentRootPath, "certs", "dev-cert.pfx");
if (File.Exists(certPath))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(55837, listenOptions =>
        {
            listenOptions.UseHttps(certPath, "yourPfxPassword");
        });
    });
}
else
{
    Console.WriteLine($"Warning: certificate not found at '{certPath}'. Skipping custom Kestrel HTTPS configuration.");
    Console.WriteLine("Use `dotnet dev-certs https --trust` or put your PFX at the path above to enable custom HTTPS.");
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Simple Swagger doc (no JWT security)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "William Metal API - NoAuth", Version = "v1" });
});

// CORS - permissive for quick testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Database Context
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conn))
{
    Console.WriteLine("Warning: DefaultConnection is not configured.");
}
builder.Services.AddDbContext<WilliamMetalContext>(options =>
    options.UseNpgsql(conn));

// Keep DI registrations for app services
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Authentication removed: do not call UseAuthentication/UseAuthorization here.

app.MapControllers();

// Initialize Database (catch and print errors so startup doesn't fail silently)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<WilliamMetalContext>();

        // Apply migrations
        context.Database.Migrate();

        // Seed initial data
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Database initialization error:");
        Console.WriteLine(ex);
    }
}

app.Run();

// Example of using HttpClient to make a request to the Products API
// var client = new HttpClient();
// client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
// var resp = await client.GetAsync("https://localhost:5062/api/Products");