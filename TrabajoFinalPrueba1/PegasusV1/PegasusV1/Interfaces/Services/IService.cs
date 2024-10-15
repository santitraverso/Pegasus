using PegasusV1.Entities;
using System.Linq.Expressions;

namespace PegasusV1.Interfaces
{
    public interface IService<T> where T : class
    {
        Task<T?> GetById(int id, Expression<Func<T, object>>[]? includes = null);

        Task<List<T>> GetForCombo(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>[]? includes = null);

        Task<T> Create(T entity);

        Task<T> Update(T entity);

        Task Delete(T entity);

        Task<List<T>> CreateAll(List<T> entities);

        Task<List<T>> UpdateAll(List<T> entities);

        Task DeleteAll(List<T> entities);

        Task<List<CursoMateria>> GetCursoMateriaForCombo(Expression<Func<CursoMateria, bool>>? predicate = null);

        Task<List<IntegrantesEventos>> GetIntegrantesEventosForCombo(Expression<Func<IntegrantesEventos, bool>>? predicate = null);

        Task<List<Asistencia>> GetAsistenciasForCombo(Expression<Func<Asistencia, bool>>? predicate = null);

        Task<List<CuadernoComunicados>> GetCuadernoComunicadosForCombo(Expression<Func<CuadernoComunicados, bool>>? predicate = null);

        Task<List<Desempenio>> GetDesempenioForCombo(Expression<Func<Desempenio, bool>>? predicate = null);

        Task<List<Desempeno>> GetDesempenoForCombo(Expression<Func<Desempeno, bool>>? predicate = null);

        Task<List<Hijo>> GetHijoForCombo(Expression<Func<Hijo, bool>>? predicate = null);

        Task<List<Pago>> GetPagoForCombo(Expression<Func<Pago, bool>>? predicate = null);

        Task<List<Tarea>> GetTareaForCombo(Expression<Func<Tarea, bool>>? predicate = null);

        Task<List<Contenido>> GetContenidoForCombo(Expression<Func<Contenido, bool>>? predicate = null);

        Task<List<Curso>> GetCursoForCombo(Expression<Func<Curso, bool>>? predicate = null);
        Task<List<IntegrantesCursos>> GetIntegrantesCursosForCombo(Expression<Func<IntegrantesCursos, bool>>? predicate = null);

        Task<List<Calificaciones>> GetCalificacionesForCombo(Expression<Func<Calificaciones, bool>>? predicate = null);

        Task<List<Modulos>> GetModulosForCombo(Expression<Func<Modulos, bool>>? predicate = null);

        Task<List<Perfiles>> GetPerfilesForCombo(Expression<Func<Perfiles, bool>>? predicate = null);

        Task<List<DesempenoAlumnos>> GetDesempenoAlumnosForCombo(Expression<Func<DesempenoAlumnos, bool>>? predicate = null);

        Task<List<ComunicadoAlumnos>> GetComunicadoAlumnosForCombo(Expression<Func<ComunicadoAlumnos, bool>>? predicate = null);
        
        Task<List<Calificaciones>> GetCalificacionesByUser(int userId);

        Task<List<Calificaciones>> GetCalificacionesByMateria(int materiaId);

        Task<List<Calificaciones>> GetCalificacionesByUserAndMateria(int userId, int materiaId);
        Task<List<ContenidoMaterias>> GetContenidoMateriasForCombo(Expression<Func<ContenidoMaterias, bool>>? predicate = null);
    }
}
