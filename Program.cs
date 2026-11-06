using KodaStore.Api.Data;
using KodaStore.Api.Files;
using KodaStore.Api.Middlewares;
using KodaStore.Api.Repositories;
using KodaStore.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(
    option =>
    {
        option.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance ??= context.HttpContext.Request.Path.Value;

            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        };
    }
);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// builder.Services.AddSingleton<InMemoryProductCatalog>();

// builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
// builder.Services.AddSingleton<IProductService, ProductService>();

// Repository
builder.Services.AddScoped<IProductRepository, SqlProductRepository>();

// Service
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductImageStorage, LocalProductImageStorage>();

var app = builder.Build();
app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();