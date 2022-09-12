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

        public async Task<T> GetById(int id, Expression<Func<T, object>>[]? includes = null)
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
    }
}
