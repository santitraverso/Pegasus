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
        public DbSet<CalificacionMateria> CalificacionMateria { get; set; }
        public DbSet<Contenido> Contenido { get; set; }
        public DbSet<Curso> Curso { get; set; }
        public DbSet<IntegrantesCursos> IntegrantesCursos { get; set; }
        public DbSet<Calificaciones> Calificaciones { get; set; }
        public DbSet<Modulos> Modulos { get; set; }
        public DbSet<Perfiles> Roles { get; set; }
        public DbSet<ContenidoMaterias> ContenidoMaterias { get; set; }

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        { 
            options.UseSqlServer(Configuration.GetConnectionString("WebApiDatabase"));
        }

        //Testear esto para no tener que cargar por separado todo
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Asistencia>()
                .HasOne(x => x.Alumno);
            modelBuilder.Entity<Asistencia>()
                .HasOne(x => x.Materia);
        }
    }
}
