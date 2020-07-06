using Domain;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Services
{
    public interface IGenericService
    {

        Task<T> GetAsync<T>(int id, string includeProperties = "") where T : class, IBaseEntity;

        Task<int> CreateAsync<T>(T entity) where T : class, IBaseEntity;

        Task<int> CreateRangeAsync<T>(IEnumerable<T> entities) where T : class, IBaseEntity;

        int CreateRange<T>(IEnumerable<T> entities) where T : class, IBaseEntity;

        Task<int> UpdateAsync<T>(T entity) where T : class, IBaseEntity, IHaveRowVersion;

        Task<int> UpsertAsync<T>(T entity) where T : class, IBaseEntity;

        Task<int> DeleteAsync<T>(T entity) where T : class, IBaseEntity;

        Task<int> DeleteRangeAsync<T>(IEnumerable<T> entities) where T : class, IBaseEntity;

        Task<int> CountAsync<T>(Expression<Func<T, bool>> filter = null) where T : class, IBaseEntity;

        Task<List<T>> GetAllAsync<T>(bool noTrack = true, params Expression<Func<T, object>>[] includes) where T : class, IBaseEntity;

        Task<PagedResult<U>> GetAllPagedAsync<T, U>(int pageSize = 50, int pageNumber = 1, bool noTrack = true, params Expression<Func<T, object>>[] includes)
            where T : class, IBaseEntity
            where U : class;

        Task<List<T>> FindTakeAsync<T>(Expression<Func<T, bool>> filter = null,
                                    Func<IQueryable<T>,
                                    IOrderedQueryable<T>> orderBy = null,
                                    bool noTrack = false,
                                    int? take = null,
                                    params Expression<Func<T, object>>[] includes)
            where T : class, IBaseEntity;


        Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> filter = null,
                                   Func<IQueryable<T>,
                                   IOrderedQueryable<T>> orderBy = null,
                                   bool noTrack = false,
                                   params Expression<Func<T, object>>[] includes)
            where T : class, IBaseEntity;

        Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, bool noTrack = false,
            params Expression<Func<T, object>>[] includes) where T : class, IBaseEntity;

    }
}