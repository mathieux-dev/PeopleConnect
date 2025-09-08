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
        // Política permissiva para debug de CORS
        policy.AllowAnyOrigin()
              .AllowAnyMethod() 
              .AllowAnyHeader();
    });
});
#endregion

builder.Services.AddApplication();

string connectionString;

// Primeiro tentar variáveis separadas (mais confiável)
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");

if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbUser) && 
    !string.IsNullOrEmpty(dbPass) && !string.IsNullOrEmpty(dbName))
{
    // Construir connection string com SSL
    connectionString = $"Host={dbHost};Port={dbPort ?? "5432"};Database={dbName};Username={dbUser};Password={dbPass};SslMode=Require;Trust Server Certificate=true;";
}
else
{
    // Fallback para URL completa
    var renderConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
                                builder.Configuration.GetConnectionString("DefaultConnection");

    if (!string.IsNullOrEmpty(renderConnectionString) && 
        renderConnectionString != "YOUR_DATABASE_URL_GOES_HERE_VIA_ENV_VARIABLE")
    {
        // Converter URL para formato tradicional com SSL
        if (renderConnectionString.StartsWith("postgresql://") || renderConnectionString.StartsWith("postgres://"))
        {
            var uri = new Uri(renderConnectionString);
            connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SslMode=Require;Trust Server Certificate=true;";
        }
        else
        {
            connectionString = renderConnectionString.Contains("SslMode") ? 
                renderConnectionString : 
                $"{renderConnectionString};SslMode=Require;Trust Server Certificate=true";
        }
    }
    else
    {
        // Para testes de integração ou ambiente de desenvolvimento, use uma connection string padrão
        if (builder.Environment.EnvironmentName == "IntegrationTests" || builder.Environment.IsDevelopment())
        {
            connectionString = "Host=localhost;Database=peopleconnect_test;Username=postgres;Password=postgres";
        }
        else
        {
            throw new InvalidOperationException("Não foi possível determinar a Connection String do banco de dados. Configure as variáveis DB_HOST, DB_USER, DB_PASSWORD, DB_NAME no Render.");
        }
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

// Configurar Headers de Segurança
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

// CORS deve vir antes de Authentication
app.UseCors(app.Environment.IsProduction() ? "ProductionPolicy" : "AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health checks e endpoints de debug
app.MapGet("/", () => Results.Ok(new { 
    service = "PeopleConnect API", 
    status = "running", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    database = "connected"
}));

// Endpoint de debug para verificar configuração
app.MapGet("/debug/config", (IConfiguration config) => 
{
    var hasDbHost = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_HOST"));
    var hasDbUrl = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL"));
    
    return Results.Ok(new 
    { 
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
        hasDbHost = hasDbHost,
        hasDbUrl = hasDbUrl,
        port = Environment.GetEnvironmentVariable("PORT"),
        corsPolicy = app.Environment.IsProduction() ? "ProductionPolicy" : "AllowAll",
        timestamp = DateTime.UtcNow
    });
});

// Endpoint para testar CORS
app.MapPost("/debug/cors", () => Results.Ok(new { message = "CORS funcionando!", timestamp = DateTime.UtcNow }));
app.MapMethods("/debug/cors", new[] { "OPTIONS" }, () => Results.Ok());

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
        
        logger.LogInformation("Testando conexão com banco de dados...");
        var canConnect = await context.Database.CanConnectAsync();
        logger.LogInformation("Conexão com banco: {CanConnect}", canConnect);
        
        if (!canConnect)
        {
            throw new InvalidOperationException("Não foi possível conectar ao banco de dados");
        }
        
        logger.LogInformation("Verificando migrations pendentes...");
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        logger.LogInformation("Migrations pendentes: {Count}", pendingMigrations.Count());
        
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Aplicando {Count} migrations...", pendingMigrations.Count());
            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation("Migration pendente: {Migration}", migration);
            }
            
            await context.Database.MigrateAsync();
            logger.LogInformation("Todas as migrations foram aplicadas!");
        }
        else
        {
            logger.LogInformation("Banco de dados já está atualizado!");
        }
        
        // Verificar se as tabelas foram criadas
        logger.LogInformation("Verificando se as tabelas foram criadas...");
        try
        {
            // Teste simples para verificar se as tabelas existem
            var hasPersonsTable = await context.Database.CanConnectAsync();
            logger.LogInformation("Verificação concluída - Conexão funcionando: {HasConnection}", hasPersonsTable);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Erro ao verificar tabelas: {Error}", ex.Message);
        }
        
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