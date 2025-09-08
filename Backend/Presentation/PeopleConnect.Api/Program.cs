using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PeopleConnect.Api.Middleware;
using PeopleConnect.Application;
using PeopleConnect.Infrastructure;
using System.Text;
using Asp.Versioning;

// Função auxiliar para converter a URL do Render
string ConvertPostgresUrlToConnectionString(string url)
{
    if (string.IsNullOrEmpty(url)) return string.Empty;

    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');

    var user = userInfo[0];
    var password = userInfo[1];
    var host = uri.Host;
    var port = uri.Port;
    var database = uri.AbsolutePath.TrimStart('/');

    // Formato ADO.NET clássico que o EF Core/Npgsql entendem perfeitamente.
    // SslMode=Require e Trust Server Certificate=true são recomendados para ambientes de nuvem.
    return $"Host={host};Port={port};Database={database};Username={user};Password={password};SslMode=Require;Trust Server Certificate=true;";
}


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
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "PeopleConnect API", 
        Version = "v1",
        Description = "API para gerenciamento de pessoas com Clean Architecture",
        Contact = new OpenApiContact
        {
            Name = "PeopleConnect Team",
            Email = "contato@peopleconnect.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
        new Asp.Versioning.UrlSegmentApiVersionReader(),
        new Asp.Versioning.QueryStringApiVersionReader("version"),
        new Asp.Versioning.HeaderApiVersionReader("X-Version"));
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
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("ProductionPolicy", policy =>
    {
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost";
        policy.WithOrigins(frontendUrl)
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization");
    });
});
#endregion

builder.Services.AddApplication();

// --- ALTERAÇÃO PRINCIPAL AQUI ---
var connectionStringUrl = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? 
                        builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionStringUrl))
{
    throw new InvalidOperationException("Connection String não encontrada.");
}

// Converte a URL para o formato clássico antes de passar para a camada de infraestrutura
var formattedConnectionString = ConvertPostgresUrlToConnectionString(connectionStringUrl);
builder.Services.AddInfrastructure(builder.Configuration, formattedConnectionString);

var app = builder.Build();

#region Pipeline Configuration
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PeopleConnect API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

if (app.Environment.IsProduction())
{
    app.UseCors("ProductionPolicy");
}
else
{
    app.UseCors("AllowAll");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Aplicar migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PeopleConnect.Infrastructure.Persistence.DataContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    if (app.Environment.IsProduction())
    {
        try
        {
            logger.LogInformation("Aplicando migrations do banco de dados...");
            context.Database.Migrate();
            logger.LogInformation("Migrations aplicadas com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao aplicar migrations do banco de dados.");
            throw;
        }
    }
    else
    {
        context.Database.EnsureCreated();
    }
}
#endregion

app.Run();

public partial class Program { }