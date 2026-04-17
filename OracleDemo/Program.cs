using Microsoft.EntityFrameworkCore;
using OracleDemo.Data;
using OracleDemo.Repositories;
using OracleDemo.Strategies;
using OracleDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

// Design Patterns
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IndexedQueryStrategy>();
builder.Services.AddScoped<NonIndexedQueryStrategy>();
builder.Services.AddScoped<RawSqlQueryStrategy>();
builder.Services.AddSingleton(BenchmarkService.Instance);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();   // <-- app is created here

// ========== SERVE NEXT.JS FRONTEND FROM frontend/out ==========
var frontendPath = Path.Combine(Directory.GetCurrentDirectory(), "frontend", "out");

if (Directory.Exists(frontendPath))
{
    // Serve static files (CSS, JS, images)
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath)
    });

    // Explicitly serve index.html at the root
    app.MapGet("/", async context =>
    {
        var indexPath = Path.Combine(frontendPath, "index.html");
        if (File.Exists(indexPath))
        {
            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(indexPath);
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Not Found");
        }
    });

    // Fallback: any non‑API request -> serve index.html (for client‑side routing)
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath)
    });

    Console.WriteLine($"Serving frontend from: {frontendPath}");
}
else
{
    Console.WriteLine($"WARNING: Frontend not found at {frontendPath}. Run: cd frontend && npm run build");
}
// ================================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();