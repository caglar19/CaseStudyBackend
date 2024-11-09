using CaseStudy.Core.Common;
using CaseStudy.Core.Entities;
using System.Linq.Expressions;

namespace CaseStudy.DataAccess.Repositories;

public interface IUserRepository
{
    IQueryable<User> AsQueryable();

    Task<User?> GetFirstOrDefaultAsync(Expression<Func<User, bool>> predicate, params Expression<Func<User, object>>[] includeProperties);

    Task<User> GetFirstAsync(Expression<Func<User, bool>> predicate, params Expression<Func<User, object>>[] includeProperties);

    Task<List<User>> GetAllAsync(Expression<Func<User, bool>> predicate, params Expression<Func<User, object>>[] includeProperties);

    Task<User> AddAsync(User entity);

    Task<User> UpdateAsync(User entity);

    Task<User> DeleteAsync(User entity);

    Task<List<History<User>>> GetAllHistoryAsync(
        Expression<Func<History<User>, bool>> filter);

    Task<User?> GetByRefIdAsync(Guid refId);

    Task<History<User>> CreateHistoryAsync(History<User> entity);
}