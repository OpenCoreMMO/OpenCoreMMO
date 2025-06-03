using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Interfaces;
using Serilog;

namespace NeoServer.Data.Repositories;

/// <summary>
///     This class represents the generic repository base from application.
/// </summary>
/// <typeparam name="TEntity">Generic type for repository entity.</typeparam>
public class BaseRepository<TEntity> : IBaseRepositoryNeo<TEntity>
    where TEntity : class
{
    private readonly DbContextOptions<NeoContext> _contextOptions;
    private readonly ILogger _logger;

    #region constructors

    public BaseRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger)
    {
        _contextOptions = contextOptions;
        _logger = logger;
    }

    #endregion

    public NeoContext NewDbContext => new(_contextOptions, _logger);


    #region private methods implementation

    /// <summary>
    ///     This method is responsible for save changes in database.
    /// </summary>
    /// <returns></returns>
    public async Task CommitChanges(DbContext context)
    {
        if (context is null) return;
        await context.SaveChangesAsync();
    }

    #endregion

    #region public methods implementation

    /// <summary>
    ///     This method is responsible for insert generic entity in database.
    /// </summary>
    /// <param name="entity">The generic entity to insert.</param>
    public async Task Insert(TEntity entity)
    {
        await using var context = NewDbContext;
        context.Add(entity);
        await CommitChanges(context);
    }

    /// <summary>
    ///     This method is responsible for update generic entity in database.
    /// </summary>
    /// <param name="entity">The generic entity to update.</param>
    public async Task Update(TEntity entity)
    {
        await using var context = NewDbContext;
        context.Update(entity);
        await CommitChanges(context);
    }

    /// <summary>
    ///     This method is responsible for insert generic entity in database.
    /// </summary>
    /// <param name="entity">The generic entity to insert.</param>
    public async Task Delete(TEntity entity)
    {
        await using var context = NewDbContext;
        context.Remove(entity);
        await CommitChanges(context);
    }

    /// <summary>
    ///     This method is responsible for get all registers from entity table.
    /// </summary>
    public async Task<IList<TEntity>> GetAllAsync()
    {
        await using var context = NewDbContext;
        var entity = context.Set<TEntity>();
        return await entity.ToListAsync();
    }

    /// <summary>
    ///     This method is responsible for get all registers from entity table.
    /// </summary>
    public async Task<TEntity> GetAsync(int id)
    {
        await using var context = NewDbContext;
        var entity = context.Set<TEntity>();
        return await entity.FindAsync(id);
    }

    public async Task<int> CountAllAsync(Expression<Func<TEntity, bool>> filter)
    {
        await using var context = NewDbContext;
        var entity = context.Set<TEntity>();
        return await entity.CountAsync(filter);
    }

    #endregion
}