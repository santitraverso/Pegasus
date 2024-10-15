using PegasusV1.DbDataContext;
using PegasusV1.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PegasusV1.Entities;

namespace PegasusV1.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly IConfiguration _configuration;

        public Repository(IConfiguration configuration)
        {
            _configuration = configuration;
        }        
        public async Task<List<T>> GetForCombo(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>[]? includes = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<T>? query = dbContext.Set<T>().AsQueryable();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                if (includes != null)
                {
                    foreach (var include in includes)
                    {
                        query = query.Include(include);
                    }
                }

                return await query.ToListAsync();
            }
        }

        public async Task<T> Create(T entity)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                DbSet<T> set = dbContext.Set<T>();

                set.Add(entity);

                await dbContext.SaveChangesAsync();

                return entity;
            }
        }

        public async Task<T> Update(T entity)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                DbSet<T> set = dbContext.Set<T>();

                set.Update(entity);

                await dbContext.SaveChangesAsync();

                return entity;
            }
        }

        public async Task Delete(T entity)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                DbSet<T> set = dbContext.Set<T>();

                set.Remove(entity);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<T>> CreateAll(List<T> entities)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                DbSet<T> set = dbContext.Set<T>();

                foreach (T entity in entities)
                {
                    set.Add(entity);

                    await dbContext.SaveChangesAsync();
                }

                return entities;
            }
        }

        public async Task<List<T>> UpdateAll(List<T> entities)
        {       
            using (DataContext dbContext = new DataContext(_configuration))
            {
                DbSet<T> set = dbContext.Set<T>();

                foreach (T entity in entities)
                {
                    set.Update(entity);

                    await dbContext.SaveChangesAsync();
                }

                return entities;
            }
        }

        public async Task DeleteAll(List<T> entities)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                DbSet<T> set = dbContext.Set<T>();

                foreach (T entity in entities)
                {
                    set.Remove(entity);

                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task<List<CursoMateria>> GetCursoMateriaForCombo(Expression<Func<CursoMateria, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<CursoMateria>? query = dbContext.Set<CursoMateria>().AsQueryable();

                query = query.Include(x => x.Materia);
                query = query.Include(x => x.Curso);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<IntegrantesEventos>> GetIntegrantesEventosForCombo(Expression<Func<IntegrantesEventos, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<IntegrantesEventos>? query = dbContext.Set<IntegrantesEventos>().AsQueryable();

                query = query.Include(x => x.Evento);
                query = query.Include(x => x.Usuario);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<Asistencia>> GetAsistenciasForCombo(Expression<Func<Asistencia, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Asistencia>? query = dbContext.Set<Asistencia>().AsQueryable();

                query = query.Include(x => x.Alumno);
                query = query.Include(x => x.Materia);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<CuadernoComunicados>> GetCuadernoComunicadosForCombo(Expression<Func<CuadernoComunicados, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<CuadernoComunicados>? query = dbContext.Set<CuadernoComunicados>().AsQueryable();

                query = query.Include(x => x.Profesor);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<Desempenio>> GetDesempenioForCombo(Expression<Func<Desempenio, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Desempenio>? query = dbContext.Set<Desempenio>().AsQueryable();

                query = query.Include(x => x.Alumno);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<Desempeno>> GetDesempenoForCombo(Expression<Func<Desempeno, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Desempeno>? query = dbContext.Set<Desempeno>().AsQueryable();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }


        public async Task<List<Hijo>> GetHijoForCombo(Expression<Func<Hijo, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Hijo>? query = dbContext.Set<Hijo>().AsQueryable();

                query = query.Include(x => x.Padre);
                query = query.Include(x => x.HijoUsuario);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<Pago>> GetPagoForCombo(Expression<Func<Pago, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Pago>? query = dbContext.Set<Pago>().AsQueryable();

                query = query.Include(x => x.Alumno);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<DesempenoAlumnos>> GetDesempenoAlumnosForCombo(Expression<Func<DesempenoAlumnos, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<DesempenoAlumnos>? query = dbContext.Set<DesempenoAlumnos>().AsQueryable();

                query = query.Include(x => x.Alumno);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<ComunicadoAlumnos>> GetComunicadoAlumnosForCombo(Expression<Func<ComunicadoAlumnos, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<ComunicadoAlumnos>? query = dbContext.Set<ComunicadoAlumnos>().AsQueryable();

                query = query.Include(x => x.Alumno);
                query = query.Include(x => x.Comunicado);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<Tarea>> GetTareaForCombo(Expression<Func<Tarea, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Tarea>? query = dbContext.Set<Tarea>().AsQueryable();

                query = query.Include(x => x.Alumno);
                query = query.Include(x => x.Materia);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }


        public async Task<List<Contenido>> GetContenidoForCombo(Expression<Func<Contenido, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Contenido>? query = dbContext.Set<Contenido>().AsQueryable();

                query = query.Include(x => x.Materia);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<T?> GetById(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>[]? includes = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<T>? query = dbContext.Set<T>().AsQueryable();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                if (includes != null)
                {
                    foreach (var include in includes)
                    {
                        query = query.Include(include);
                    }
                }

                return await query.SingleOrDefaultAsync();
            }
        }

        public async Task<List<Curso>> GetCursoForCombo(Expression<Func<Curso, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Curso>? query = dbContext.Set<Curso>().AsQueryable();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<IntegrantesCursos>> GetIntegrantesCursosForCombo(Expression<Func<IntegrantesCursos, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<IntegrantesCursos>? query = dbContext.Set<IntegrantesCursos>().AsQueryable();

                query = query.Include(x => x.Curso);
                query = query.Include(x => x.Usuario);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }


        public async Task<List<Calificaciones>> GetCalificacionesForCombo(Expression<Func<Calificaciones, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Calificaciones>? query = dbContext.Set<Calificaciones>().AsQueryable();

                query = query.Include(x => x.Materia);
                query = query.Include(x => x.Usuario);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<Modulos>> GetModulosForCombo(Expression<Func<Modulos, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Modulos>? query = dbContext.Set<Modulos>().AsQueryable();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<Perfiles>> GetPerfilesForCombo(Expression<Func<Perfiles, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Perfiles>? query = dbContext.Set<Perfiles>().AsQueryable();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }

        public async Task<List<Calificaciones>> GetCalificacionesByUser(int userId)
        { 
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Calificaciones> calificaciones = dbContext.Set<Calificaciones>().AsQueryable()
                    .Where(x => x.Id_Alumno == userId);

                return await calificaciones.ToListAsync();
            }
        }


        public async Task<List<Calificaciones>> GetCalificacionesByMateria(int materiaId)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Calificaciones> calificaciones = dbContext.Set<Calificaciones>().AsQueryable()
                    .Where(x => x.Id_Materia == materiaId);

                return await calificaciones.ToListAsync();
            }
        }

        public async Task<List<Calificaciones>> GetCalificacionesByUserAndMateria(int userId, int materiaId)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<Calificaciones> calificaciones = dbContext.Set<Calificaciones>().AsQueryable();

                calificaciones = calificaciones.Where(x => x.Id_Materia == materiaId && x.Id_Alumno == userId);

                return await calificaciones.ToListAsync();
            }
        }

        public async Task<List<ContenidoMaterias>> GetContenidoMateriasForCombo(Expression<Func<ContenidoMaterias, bool>>? predicate = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<ContenidoMaterias>? query = dbContext.Set<ContenidoMaterias>().AsQueryable();

                query = query.Include(x => x.Materia);

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync();
            }
        }
    }
}
