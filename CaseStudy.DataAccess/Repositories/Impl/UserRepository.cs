using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using CaseStudy.Core.Common;
using CaseStudy.Core.Entities;
using CaseStudy.Core.Enums;
using CaseStudy.Core.Exceptions;
using CaseStudy.DataAccess.Persistence;
using System.Linq.Expressions;

namespace CaseStudy.DataAccess.Repositories.Impl;

public class UserRepository : IUserRepository
{
    private readonly DatabaseContext _context;
    private readonly DbSet<User> _dbSet;
    private readonly IMongoCollection<History<User>> _mongoCollection;

    public UserRepository(DatabaseContext context, IMongoClient client)
    {
        _context = context;
        _dbSet = context.Set<User>();
        _mongoCollection = client.GetDatabase("studyio_db").GetCollection<History<User>>(typeof(User).Name);
    }

    public IQueryable<User> AsQueryable() => _dbSet.AsQueryable();

    public async Task<User> AddAsync(User entity)
    {
        entity.RefId = Guid.NewGuid();
        entity.CreatedOn = DateTime.Now;
        var addedEntity = (await _dbSet.AddAsync(entity)).Entity;
        await _context.SaveChangesAsync();
        return addedEntity;
    }

    public async Task<User> DeleteAsync(User entity)
    {
        entity.DeletedOn = DateTime.Now;
        entity.DataStatus = EDataStatus.Deleted;
        var removedEntity = _dbSet.Remove(entity).Entity;
        await _context.SaveChangesAsync();
        return removedEntity;
    }

    public async Task<List<User>> GetAllAsync(Expression<Func<User, bool>> predicate, params Expression<Func<User, object>>[] includeProperties)
    {
        IQueryable<User> query = _dbSet;
        if (includeProperties != null)
        {
            query = includeProperties.Aggregate(query, (current, include) => current.Include(include));
        }
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.ToListAsync();
    }

    public async Task<User?> GetFirstOrDefaultAsync(Expression<Func<User, bool>> predicate, params Expression<Func<User, object>>[] includeProperties)
    {
        IQueryable<User> query = _dbSet;
        if (includeProperties != null)
        {
            query = includeProperties.Aggregate(query, (current, include) => current.Include(include));
        }
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.FirstOrDefaultAsync();
    }

    public async Task<User> GetFirstAsync(Expression<Func<User, bool>> predicate, params Expression<Func<User, object>>[] includeProperties)
    {
        IQueryable<User> query = _dbSet;
        if (includeProperties != null)
        {
            query = includeProperties.Aggregate(query, (current, include) => current.Include(include));
        }
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.FirstOrDefaultAsync() ??
               throw new ResourceNotFoundException(typeof(User));
    }

    private static readonly Func<DbContext, Guid, Task<User?>>
        GetByRefIdCompiled =
            EF.CompileAsyncQuery((DbContext context, Guid refId) =>
                context.Set<User>()
                    .FirstOrDefault(x => x.RefId == refId && x.DataStatus == EDataStatus.Active));

    public async Task<User?> GetByRefIdAsync(Guid refId)
    {
        return await GetByRefIdCompiled(_context, refId);
    }

    public async Task<User> UpdateAsync(User entity)
    {
        entity.UpdatedOn = DateTime.Now;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<List<History<User>>> GetAllHistoryAsync(
        Expression<Func<History<User>, bool>> filter)
    {
        return await _mongoCollection.Find(filter ?? (u => true)).ToListAsync();
    }

    public async Task<History<User>> CreateHistoryAsync(History<User> entity)
    {
        await _mongoCollection.InsertOneAsync(entity);
        return await _mongoCollection.Find(x => x.Id == entity.Id).FirstOrDefaultAsync();
    }
}