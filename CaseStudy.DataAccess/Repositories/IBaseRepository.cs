using System.Linq.Expressions;
using CaseStudy.Core.Common;

namespace CaseStudy.DataAccess.Repositories;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    IQueryable<TEntity> AsQueryable();

    Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

    Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

    Task<TEntity> AddAsync(TEntity entity);

    Task<TEntity> UpdateAsync(TEntity entity);

    Task<TEntity> DeleteAsync(TEntity entity);

    Task<List<History<TEntity>>> GetAllHistoryAsync(
        Expression<Func<History<TEntity>, bool>> filter);

    Task<TEntity?> GetByRefIdAsync(Guid refId);

    Task<History<TEntity>> CreateHistoryAsync(History<TEntity> entity);
}