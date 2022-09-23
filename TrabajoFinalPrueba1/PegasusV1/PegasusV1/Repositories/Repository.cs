﻿using PegasusV1.DbDataContext;
using PegasusV1.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace PegasusV1.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly IConfiguration _configuration;

        public Repository(IConfiguration configuration)
        {
            _configuration = configuration;
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

        public async Task<T?> GetById(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>[]? includes = null)
        {
            using (DataContext dbContext = new DataContext(_configuration))
            {
                IQueryable<T>? query = dbContext.Set<T>().AsQueryable();

                query = query.Where(predicate);

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
    }
}
