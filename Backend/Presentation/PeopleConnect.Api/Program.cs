using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PeopleConnect.Api.Middleware;
using PeopleConnect.Application;
using PeopleConnect.Infrastructure;
using System.Text;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole();
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

#region Service Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PeopleConnect API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            Scheme = "oauth2", Name = "Bearer", In = ParameterLocation.Header
        },
        new List<string>()
    }});
});

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Version"));
}).AddApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey") ??
                jwtSettings["SecretKey"] ??
                throw new InvalidOperationException("JWT Secret Key não configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "PeopleConnectAPI",
            ValidAudience = jwtSettings["Audience"] ?? "PeopleConnectAPI",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    options.AddPolicy("ProductionPolicy", policy =>
    {
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            policy.WithOrigins(frontendUrl)
                  .WithMethods("GET", "POST", "PUT", "DELETE")
                  .WithHeaders("Content-Type", "Authorization");
        }
    });
});
#endregion

builder.Services.AddApplication();

string connectionString;

// Tentar múltiplas formas de obter a connection string
var renderConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
                            builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(renderConnectionString) && 
    renderConnectionString != "YOUR_DATABASE_URL_GOES_HERE_VIA_ENV_VARIABLE")
{
    // Se for uma URL do PostgreSQL, adicionar SSL
    if (renderConnectionString.StartsWith("postgresql://") || renderConnectionString.StartsWith("postgres://"))
    {
        // Adicionar SSL Mode se não estiver presente
        if (!renderConnectionString.Contains("sslmode") && !renderConnectionString.Contains("SslMode"))
        {
            var separator = renderConnectionString.Contains("?") ? "&" : "?";
            connectionString = $"{renderConnectionString}{separator}sslmode=require";
        }
        else
        {
            connectionString = renderConnectionString;
        }
    }
    else
    {
        // Se for formato tradicional, adicionar SSL
        connectionString = renderConnectionString.Contains("SslMode") ? 
            renderConnectionString : 
            $"{renderConnectionString};SslMode=Require;Trust Server Certificate=true";
    }
}
else
{
    // Fallback para variáveis separadas
    var dbHost = Environment.GetEnvironmentVariable("PGHOST");
    var dbPort = Environment.GetEnvironmentVariable("PGPORT");
    var dbUser = Environment.GetEnvironmentVariable("PGUSER");
    var dbPass = Environment.GetEnvironmentVariable("PGPASSWORD");
    var dbName = Environment.GetEnvironmentVariable("PGDATABASE");

    if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPass) && !string.IsNullOrEmpty(dbName))
    {
        connectionString = $"Host={dbHost};Port={dbPort ?? "5432"};Database={dbName};Username={dbUser};Password={dbPass};SslMode=Require;Trust Server Certificate=true;";
    }
    else
    {
        throw new InvalidOperationException("Não foi possível determinar a Connection String do banco de dados. Configure DATABASE_URL ou ConnectionStrings__DefaultConnection no Render.");
    }
}

builder.Services.AddInfrastructure(builder.Configuration, connectionString);


var app = builder.Build();

#region Pipeline Configuration
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PeopleConnect API v1"));
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors(app.Environment.IsProduction() ? "ProductionPolicy" : "AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Aplicar migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PeopleConnect.Infrastructure.Persistence.DataContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Verificando e aplicando migrations...");
        
        // Log de debug da connection string (sem mostrar senha)
        var debugConnectionString = connectionString.Length > 50 ? 
            connectionString.Substring(0, 50) + "..." : connectionString;
        logger.LogInformation("Debug - Connection string (início): {DebugString}", debugConnectionString);
        
        context.Database.Migrate();
        logger.LogInformation("Migrations verificadas/aplicadas com sucesso.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro fatal ao aplicar migrations do banco de dados.");
        throw;
    }
}
#endregion

app.Run();

public partial class Program { }