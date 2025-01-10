using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NeoServer.Data.Interfaces;

public interface IBaseRepositoryNeo<TEntity> where TEntity : class
{
    Task Insert(TEntity entity);
    Task Update(TEntity entity);
    Task Delete(TEntity entity);
    Task<IList<TEntity>> GetAllAsync();
    Task<TEntity> GetAsync(int id);
    Task<int> CountAllAsync(Expression<Func<TEntity, bool>> filter);
}