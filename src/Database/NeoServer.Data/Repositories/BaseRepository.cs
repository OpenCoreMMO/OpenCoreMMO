using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    public void CommitChanges(DbContext context)
    {
        if (context is null) return;
        context.SaveChanges();
    }

    #endregion

    #region public methods implementation

    /// <summary>
    ///     This method is responsible for insert generic entity in database.
    /// </summary>
    /// <param name="entity">The generic entity to insert.</param>
    public void Insert(TEntity entity)
    {
        using var context = NewDbContext;
        context.Add(entity);
        CommitChanges(context);
    }

    /// <summary>
    ///     This method is responsible for update generic entity in database.
    /// </summary>
    /// <param name="entity">The generic entity to update.</param>
    public void Update(TEntity entity)
    {
        using var context = NewDbContext;
        context.Update(entity);
        CommitChanges(context);
    }

    /// <summary>
    ///     This method is responsible for insert generic entity in database.
    /// </summary>
    /// <param name="entity">The generic entity to insert.</param>
    public void Delete(TEntity entity)
    {
        using var context = NewDbContext;
        context.Remove(entity);
        CommitChanges(context);
    }

    /// <summary>
    ///     This method is responsible for get all registers from entity table.
    /// </summary>
    public IList<TEntity> GetAll()
    {
        using var context = NewDbContext;
        var entity = context.Set<TEntity>();
        return entity.ToList();
    }

    /// <summary>
    ///     This method is responsible for get all registers from entity table.
    /// </summary>
    public TEntity Get(int id)
    {
        using var context = NewDbContext;
        var entity = context.Set<TEntity>();
        return entity.Find(id);
    }

    public int CountAll(Expression<Func<TEntity, bool>> filter)
    {
        using var context = NewDbContext;
        var entity = context.Set<TEntity>();
        return entity.Count(filter);
    }

    #endregion
}