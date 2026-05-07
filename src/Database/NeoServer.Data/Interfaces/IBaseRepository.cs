using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NeoServer.Data.Interfaces;

public interface IBaseRepositoryNeo<TEntity> where TEntity : class
{
    void Insert(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    IList<TEntity> GetAll();
    TEntity Get(int id);
    int CountAll(Expression<Func<TEntity, bool>> filter);
}