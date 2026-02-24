using domain.interfaces;
using Domain.Entities;
using infrastructure.cache;
using infrastructure.persistence;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using OpenTelemetry.Resources;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using services.migration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var SQL_SERVER_CON = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextPool<LocaldbContext>(opt =>
{
    opt.UseSqlServer(SQL_SERVER_CON, o =>
    {
        o.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        o.CommandTimeout(30);
    })
    .ConfigureWarnings(w =>
        w.Ignore(RelationalEventId.PendingModelChangesWarning));

    opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// =============================
// REDIS
// =============================

var REDIS_CONFIG = builder.Configuration.GetSection("Redis:Configuration");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(REDIS_CONFIG.Value!));

builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = REDIS_CONFIG.Value!;
});

// =============================
// REPOSITORIES & CACHE
// =============================
builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
builder.Services.AddScoped(typeof(ICacheService<>), typeof(RedisCacheService<>));
builder.Services.AddScoped(typeof(ICacheVersionService<>), typeof(RedisCacheService<>));

// =============================
// OPENTELEMETRY
// =============================
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(
        serviceName: "k8api",
        serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
            serviceInstanceId: Environment.MachineName))
     .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation()
         .AddHttpClientInstrumentation()
         .AddEntityFrameworkCoreInstrumentation()
         .AddRedisInstrumentation()
         .AddOtlpExporter(o =>
         {
             o.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint")!);
         });
    })
    .WithMetrics(m =>
    {
        m.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint")!);
        });
    });

// =============================
// HEALTH CHECKS
// =============================
builder.Services.AddHealthChecks()
    .AddCheck("sql", new services.health.SQLHealthCheck(SQL_SERVER_CON!), tags: new[] { "ready" })
    .AddCheck("redis", new services.health.RedisPingHealthCheck(REDIS_CONFIG.Value!), tags: new[] { "ready" });

builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenAnyIP(8080);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();


app.MapOpenApi();
app.UseExceptionHandler("/error");

// =============================
// HEALTH ENDPOINTS
// =============================
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});


app.ApplyMigrationsAsync<LocaldbContext>().Wait();

// =============================
// CRUD ENDPOINTS
// =============================
app.MapCrudEndpoints<Product, LocaldbContext>("api/products");

app.Run();