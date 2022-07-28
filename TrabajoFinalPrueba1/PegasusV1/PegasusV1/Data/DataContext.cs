using Microsoft.EntityFrameworkCore;
using PegasusV1.Entities;
using Microsoft.Extensions.Configuration;

namespace PegasusV1.DbDataContext
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Asistencia> Asistencia { get; set; }
        public DbSet<Contactos> Contactos { get; set; }
        public DbSet<CuadernoComunicados> CuadernoComunicados { get; set; }
        public DbSet<Desempenio> Desempenios { get; set; }
        public DbSet<Hijo> Hijos { get; set; }
        public DbSet<IntegrantesEventos> IntegrantesEventos { get; set; }
        public DbSet<IntegrantesMaterias> IntegrantesMaterias { get; set; }
        public DbSet<Pago> Pago { get; set; }
        public DbSet<Tarea> Tarea { get; set; }

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var stringBuilder = new PostgreSqlConnectionStringBuilder("postgres://ueawfruuvgyqgh:6376c3620d377d618bc3d46c93ebfff116fb19c8df3138199a8dfa94a395a1ee@ec2-52-72-56-59.compute-1.amazonaws.com:5432/dds00uirj4bh9h")
            {
                Pooling = true,
                TrustServerCertificate = true,
                SslMode = SslMode.Require
            };
            options.UseNpgsql(stringBuilder.ConnectionString);
        }

        //No me dio bola a esto
      /*  protected internal virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CuadernoComunicados>()
                .ToTable("CUADERNO_COMUNICADOS");
        }*/
    }
}
