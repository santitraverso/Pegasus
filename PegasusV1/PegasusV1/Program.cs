using Microsoft.EntityFrameworkCore;
using PegasusV1.DbDataContext;
using PegasusV1.Interfaces;
using PegasusV1.Services;
using PegasusV1.Repositories;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using PegasusV1.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Registrar JwtGenerator como servicio
builder.Services.AddScoped<JwtGenerator>();

// Configure Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Configure logging properly
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddApplicationInsights();

// Set minimum log level to capture all our custom logs
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configure specific log levels
builder.Services.Configure<LoggerFilterOptions>(options =>
{
    options.MinLevel = LogLevel.Information;
    options.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
    options.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
    options.AddFilter("System", LogLevel.Warning);
});

// ===== CONFIGURACIÓN EXPLÍCITA DE KESTREL PARA DUAL PROTOCOL =====
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP en puerto 5130 (para React Native/Android)
    options.ListenLocalhost(5130, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });

    // HTTPS en puerto 7130 (para aplicación web)
    options.ListenLocalhost(7130, listenOptions =>
    {
        listenOptions.UseHttps();
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger con soporte para JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PegasusV1", Version = "v1" });

    // Configurar Swagger para usar JWT
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
                }
            },
            new string[] {}
        }
    });
});

//// Habilitar registro detallado para autenticación
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug();
//builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Debug);

// Configuración de la base de datos
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WebApiDatabase")));

// ===== CORS MEJORADO PARA AMBOS PROTOCOLOS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "https://pegasus-api-v2.azurewebsites.net",
            "https://localhost:7063",  // Tu aplicación web HTTPS existente
            "http://localhost:5130",   // HTTP local
            "http://10.0.2.2:5130",   // Android emulator
            "http://192.168.1.100:5130" // Dispositivos físicos (ajusta tu IP)
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Configuración de repositorios y servicios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped(typeof(IService<>), typeof(Service<>));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Configuración de autenticación unificada
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiScheme";
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "MultiScheme";
})
.AddPolicyScheme("MultiScheme", "Cookie or JWT", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        string authorization = context.Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
            return JwtBearerDefaults.AuthenticationScheme;
        return CookieAuthenticationDefaults.AuthenticationScheme;
    };
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddJwtBearer(options =>
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
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada")))
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection(); // Solo en producción
}

// Orden correcto de middleware
app.UseRouting(); // Añadido explícitamente
app.UseCors("AllowAll");

// ===== REDIRECCIÓN HTTPS CONDICIONAL =====
app.Use(async (context, next) =>
{
    // Si la request viene del puerto 7130 y no es HTTPS, redirigir
    if (context.Request.Host.Port == 7130 && !context.Request.IsHttps)
    {
        var httpsUrl = $"https://{context.Request.Host}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(httpsUrl, permanent: true);
        return;
    }

    // Si viene del puerto 5130, permitir HTTP
    await next();
});

// IMPORTANTE: Middleware de autenticación y autorización en el orden correcto
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();