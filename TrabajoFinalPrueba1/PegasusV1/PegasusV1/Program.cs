using Microsoft.EntityFrameworkCore;
using PegasusV1.DbDataContext;
using PegasusV1.Interfaces;
using PegasusV1.Services;
using PegasusV1.Repositories;
using Microsoft.Extensions.Configuration;
using PegasusV1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var stringBuilder = new PostgreSqlConnectionStringBuilder("postgres://ueawfruuvgyqgh:6376c3620d377d618bc3d46c93ebfff116fb19c8df3138199a8dfa94a395a1ee@ec2-52-72-56-59.compute-1.amazonaws.com:5432/dds00uirj4bh9h")
{
    Pooling = true,
    TrustServerCertificate = true,
    SslMode = SslMode.Require
};

builder.Services.AddEntityFrameworkNpgsql()
        .AddDbContext<DataContext>(options => options.UseNpgsql(stringBuilder.ConnectionString)); 
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


