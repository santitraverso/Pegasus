using Microsoft.EntityFrameworkCore;
using PegasusV1.DbDataContext;
using PegasusV1.Interfaces;
using PegasusV1.Services;
using PegasusV1.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer("Server=tcp:pegasusdavinci.database.windows.net,1433;Initial Catalog=Pegasus;Persist Security Info=False;User ID=pegasus;Password=davinci123.;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer("Server=.;Initial Catalog=PEGASUS;TrustServerCertificate=true;Integrated Security=True;"));
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped(typeof(IService<>), typeof(Service<>));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


