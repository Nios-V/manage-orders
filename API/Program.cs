using API.Middleware;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Auth;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    npgsqlOptions => {
        npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
    }));

builder.Services.AddScoped<IOrdenRepository, OrdenRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

builder.Services.AddStackExchangeRedisCache((options) =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "manage-orders:";
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddScoped<IOrdenService, OrdenService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJWTService, JWTService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer((options) =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((c) =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Manage Orders API", Version = "v1", Description = "Tarea Técnica Voultech"});

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT. Ejemplo: Bearer eyJhbGci..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks();

builder.Services.AddMetrics();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        await DbSeeder.SeedAsync(context);
        Console.WriteLine("Aplicando migraciones");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al migrar la base de datos");
    }
}

app.UseMiddleware<ExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.MapHealthChecks("/health");
app.MapMetrics();

app.UseHttpMetrics();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();