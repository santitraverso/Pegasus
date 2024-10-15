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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();  // Para ver los logs en la consola
builder.Logging.AddDebug();    // Para ver los logs en el depurador de Visual Studio

// Configuración de la base de datos
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer("Server=.;Initial Catalog=PEGASUS;TrustServerCertificate=true;Integrated Security=True;"));


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.WithOrigins("https://localhost:7063") 
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
});

// Configuración de repositorios y servicios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped(typeof(IService<>), typeof(Service<>));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Configura la autenticación de cookies y Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();



//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Add("Cross-Origin-Opener-Policy", "unsafe-none"); // Ajusta según necesites
//    context.Response.Headers.Add("Cross-Origin-Embedder-Policy", "unsafe-none"); // Ajusta según necesites
//    context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors 'self' https://accounts.google.com");
//    await next();
//});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();