using System.Linq.Expressions;

namespace PegasusV1.Interfaces
{
    public interface IService<T> where T : class
    {
        Task<List<T>> GetForCombo(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>[]? includes = null);

        Task<T> Create(T entity);

        Task<T> Update(T entity);

        Task Delete(T entity);

        Task<T> GetById(int id, Expression<Func<T, object>>[]? includes = null);
    }
}
