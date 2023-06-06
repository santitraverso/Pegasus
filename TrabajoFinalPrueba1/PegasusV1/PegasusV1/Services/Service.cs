using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace PegasusV1.Services
{
    public class Service<T> : IService<T> where T : class
    {
        protected readonly IRepository<T> Repository;

        public Service(IRepository<T> repository)
        {
            Repository = repository;
        }

        public async Task<T?> GetById(int id, Expression<Func<T, object>>[]? includes = null)
        {
            string query = $"Id == {id}";
            var p = Expression.Parameter(typeof(T), query);
            var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
            var ex = (Expression<Func<T, bool>>)e;

            return await Repository.GetById(ex);
        }

        public async Task<List<T>> GetForCombo(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>[]? includes = null)
        {
            return await Repository.GetForCombo(predicate, includes);
        }

        public async Task<T> Create(T entity)
        {
            return await Repository.Create(entity);
        }

        public async Task<T> Update(T entity)
        {
            return await Repository.Update(entity);
        }

        public async Task Delete(T entity)
        {
            await Repository.Delete(entity);
        }
        
        public async Task<List<T>> CreateAll(List<T> entities)
        {
            return await Repository.CreateAll(entities);
        }

        public async Task<List<T>> UpdateAll(List<T> entities)
        {
            return await Repository.UpdateAll(entities);
        }

        public async Task DeleteAll(List<T> entities)
        {
            await Repository.DeleteAll(entities);
        }

        public async Task<List<IntegrantesMaterias>> GetIntegrantesMateriasForCombo(Expression<Func<IntegrantesMaterias, bool>>? predicate = null)
        {
            return await Repository.GetIntegrantesMateriasForCombo(predicate);
        }

        public async Task<List<IntegrantesEventos>> GetIntegrantesEventosForCombo(Expression<Func<IntegrantesEventos, bool>>? predicate = null)
        {
            return await Repository.GetIntegrantesEventosForCombo(predicate);
        }

        public async Task<List<Asistencia>> GetAsistenciasForCombo(Expression<Func<Asistencia, bool>>? predicate = null)
        {
            return await Repository.GetAsistenciasForCombo(predicate);
        }

        public async Task<List<CuadernoComunicados>> GetCuadernoComunicadosForCombo(Expression<Func<CuadernoComunicados, bool>>? predicate = null)
        {
            return await Repository.GetCuadernoComunicadosForCombo(predicate);
        }

        public async Task<List<Desempenio>> GetDesempenioForCombo(Expression<Func<Desempenio, bool>>? predicate = null)
        {
            return await Repository.GetDesempenioForCombo(predicate);
        }

        public async Task<List<Hijo>> GetHijoForCombo(Expression<Func<Hijo, bool>>? predicate = null)
        {
            return await Repository.GetHijoForCombo(predicate);
        }

        public async Task<List<Pago>> GetPagoForCombo(Expression<Func<Pago, bool>>? predicate = null)
        {
            return await Repository.GetPagoForCombo(predicate);
        }

        public async Task<List<Tarea>> GetTareaForCombo(Expression<Func<Tarea, bool>>? predicate = null)
        {
            return await Repository.GetTareaForCombo(predicate);
        }
        
        public async Task<List<Contenido>> GetContenidoForCombo(Expression<Func<Contenido, bool>>? predicate = null)
        {
            return await Repository.GetContenidoForCombo(predicate);
        }

    }
}
