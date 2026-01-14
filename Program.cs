using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Option 1 (simple): run everything over HTTP on a single port.
// The front-end will be served from wwwroot and call the API on the same origin.
builder.WebHost.UseUrls("http://localhost:5062");

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Consistent JSON for the front-end
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
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
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

var app = builder.Build();

// QuestPDF license (community)
QuestPDF.Settings.License = LicenseType.Community;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve the front-end (static files)
app.UseDefaultFiles();
app.UseStaticFiles();

// Health check used by the front-end (main.js)
app.MapGet("/api/health", () => Results.Ok(new { ok = true, utc = DateTime.UtcNow }));

app.UseCors("AllowAll");

// Authentication removed: do not call UseAuthentication/UseAuthorization here.

app.MapControllers();

// If someone hits an unknown route, return the dashboard.
// (This is safe for your multi-page front-end too.)
app.MapFallbackToFile("index.html");

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
