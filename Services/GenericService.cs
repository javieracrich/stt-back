using AutoMapper.QueryableExtensions;
using Domain;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Services
{

    public class GenericService : IGenericService
    {
        private readonly IUnitOfWork uow;
        private readonly IDateTimeService dateTimeService;
        private readonly IPrincipalProvider principalProvider;
        private readonly ToggleOptions toggleOptions;
        private readonly DbContext ctx;

        public GenericService(
            IUnitOfWork unitOfWork,
            IDateTimeService dateTimeService,
            IPrincipalProvider principalProvider,
            ToggleOptions toggleOptions)
        {
            uow = unitOfWork;
            ctx = unitOfWork.Context;
            this.dateTimeService = dateTimeService;
            this.principalProvider = principalProvider;
            this.toggleOptions = toggleOptions;
        }

        public virtual Task<T> RunStoredProcSingleResult<T>(string storedProcName, string p0) where T : class, IBaseEntity
        {
            var result = ctx.Set<T>()
                .FromSql($"EXECUTE {storedProcName} {0}", p0)
                .FirstOrDefaultAsync();
            return result;
        }

        public virtual Task<T> GetAsync<T>(int id, string includeProperties = "") where T : class, IBaseEntity
        {
            var query = GetQuery<T>(false);

            foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return query.SingleOrDefaultAsync(x => x.Id == id);
        }

        public virtual Task<int> CreateAsync<T>(T entity) where T : class, IBaseEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            entity.Created = dateTimeService.UtcNow();
            if (!entity.CreatedBy.HasValue)
            {
                entity.CreatedBy = principalProvider.GetUserCode();
            }
            ctx.Set<T>().Add(entity);
            return uow.SaveChangesAsync();
        }

        public virtual Task<int> CreateRangeAsync<T>(IEnumerable<T> entities) where T : class, IBaseEntity
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var list = entities.ToList();

            Parallel.ForEach(list, entity =>
             {
                 entity.Created = DateTime.UtcNow;
                 if (!entity.CreatedBy.HasValue)
                 {
                     entity.CreatedBy = principalProvider.GetUserCode();
                 }
             });

            ctx.Set<T>().AddRange(list);
            return uow.SaveChangesAsync();
        }

        public virtual int CreateRange<T>(IEnumerable<T> entities) where T : class, IBaseEntity
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var list = entities.ToList();

            Parallel.ForEach(list, entity =>
            {
                entity.Created = DateTime.UtcNow;
                if (!entity.CreatedBy.HasValue)
                {
                    entity.CreatedBy = principalProvider.GetUserCode();
                }
            });

            ctx.Set<T>().AddRange(list);
            return uow.SaveChanges();
        }

        public virtual Task<int> UpdateAsync<T>(T entity) where T : class, IBaseEntity, IHaveRowVersion
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.IsDisabled.HasValue && entity.IsDisabled.Value)
            {
                throw new ArgumentException("entity is disabled");
            }

            entity.Updated = dateTimeService.UtcNow();
            entity.UpdatedBy = principalProvider.GetUserCode();
            ctx.Entry(entity).State = EntityState.Modified;

            if (toggleOptions.RowVersionEnabled)
            {
                if (entity.RowVersion == null)
                {
                    throw new Exception("RowVersion must be informed for Updated entities.");
                }
                else
                {
                    // https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/handling-concurrency-with-the-entity-framework-in-an-asp-net-mvc-application#add-optimistic-concurrency
                    ctx.Entry(entity).OriginalValues["RowVersion"] = entity.RowVersion;
                }
            }

            return uow.SaveChangesAsync();

        }

        public virtual Task<int> UpsertAsync<T>(T entity) where T : class, IBaseEntity
        {
            InnerUpsert(entity);
            return uow.SaveChangesAsync();
        }

        public virtual Task<int> CountAsync<T>(Expression<Func<T, bool>> filter = null) where T : class, IBaseEntity
        {
            var query = GetQuery<T>(true);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.CountAsync();
        }

        public virtual Task<int> DeleteAsync<T>(T entity) where T : class, IBaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            ctx.Set<T>().Remove(entity);
            return uow.SaveChangesAsync();
        }

        public virtual Task<int> DeleteRangeAsync<T>(IEnumerable<T> entities) where T : class, IBaseEntity
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            ctx.Set<T>().RemoveRange(entities);
            return uow.SaveChangesAsync();
        }

        public virtual int SoftDelete<T>(T entity) where T : class, IBaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            entity.Updated = dateTimeService.UtcNow();
            entity.UpdatedBy = principalProvider.GetUserCode();
            entity.IsDisabled = true;
            ctx.Entry(entity).State = EntityState.Modified;
            return uow.SaveChanges();
        }

        public virtual Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, bool noTrack = false, params Expression<Func<T, object>>[] includes) where T : class, IBaseEntity
        {
            var query = GetQuery<T>(noTrack);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null && includes.Any())
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return orderBy != null ? orderBy(query).FirstOrDefaultAsync() : query.FirstOrDefaultAsync();
        }

        public virtual Task<List<T>> FindTakeAsync<T>(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, bool noTrack = false, int? take = null, params Expression<Func<T, object>>[] includes) where T : class, IBaseEntity
        {
            var query = GetQuery<T>(noTrack);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null && includes.Any())
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return orderBy != null ? orderBy(query).ToListAsync() : query.ToListAsync();
        }

        public virtual Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, bool noTrack = false, params Expression<Func<T, object>>[] includes) where T : class, IBaseEntity
        {
            var query = GetQuery<T>(noTrack);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null && includes.Any())
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return orderBy != null ? orderBy(query).ToListAsync() : query.ToListAsync();
        }

        public virtual Task<List<T>> GetAllAsync<T>(bool noTrack = true, params Expression<Func<T, object>>[] includes) where T : class, IBaseEntity
        {
            var query = GetQuery<T>(noTrack);

            if (includes != null && includes.Any())
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return query.ToListAsync();
        }

        public async Task<PagedResult<U>> GetAllPagedAsync<T, U>(int pageSize = 50, int pageNumber = 1, bool noTrack = true, params Expression<Func<T, object>>[] includes)
            where T : class, IBaseEntity
            where U : class
        {
            var query = GetQuery<T>(noTrack);

            var result = new PagedResult<U>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                RowCount = query.Count()
            };

            if (includes != null && includes.Any())
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
            var skip = (pageNumber - 1) * pageSize;

            result.Results = await query
                .Where(x => !x.IsDisabled.HasValue || !x.IsDisabled.Value)
                .Skip(skip)
                .Take(pageSize)
                .ProjectTo<U>()
                .ToListAsync();

            return result;
        }


        private void InnerUpsert<T>(T entity) where T : class, IBaseEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.IsDisabled.HasValue && entity.IsDisabled.Value)
            {
                throw new ArgumentException("entity is disabled");
            }

            if (entity.Id == 0)
            {
                entity.Created = dateTimeService.UtcNow();
                entity.CreatedBy = principalProvider.GetUserCode();
                ctx.Entry(entity).State = EntityState.Added;
                ctx.Set<T>().Add(entity);
            }
            else
            {
                entity.Updated = dateTimeService.UtcNow();
                entity.UpdatedBy = principalProvider.GetUserCode();
                ctx.Entry(entity).State = EntityState.Modified;
                ctx.Set<T>().Update(entity);
            }
        }

        private IQueryable<T> GetQuery<T>(bool noTrack) where T : class, IBaseEntity
        {
            var query = noTrack ? ctx.Set<T>().AsNoTracking() : ctx.Set<T>();
            //todo: make a query filter
            //https://docs.microsoft.com/en-us/ef/core/querying/filters
            return query.Where(x => !x.IsDisabled.HasValue || !x.IsDisabled.Value);
        }
    }
}
