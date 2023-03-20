﻿using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

namespace Deveel.Data {
    public class EntityRepository<TEntity> : 
        IRepository<TEntity>,
        IFilterableRepository<TEntity>,
        IQueryableRepository<TEntity>
        where TEntity : class, IDataEntity {

        public EntityRepository(DbContext context) {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        Type IRepository.EntityType => typeof(TEntity);

        protected DbContext Context { get; }

        protected DbSet<TEntity> Entities => Context.Set<TEntity>();

        public async Task<string> CreateAsync(TEntity entity, CancellationToken cancellationToken = default) {
            Entities.Add(entity);
            await Context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        public async Task<IList<string>> CreateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
            foreach (var entity in entities) {
                Entities.Add(entity);
            }

            await Context.SaveChangesAsync(cancellationToken);

            return entities.Select(x => x.Id).ToList();
        }

        Task<string> IRepository.CreateAsync(IDataEntity entity, CancellationToken cancellationToken)
            => CreateAsync(AssertEntity(entity), cancellationToken);

        private static TEntity AssertEntity(IDataEntity entity) {
            if (!(entity is TEntity dataEntity))
                throw new ArgumentException($"The entity is not assignable from '{typeof(TEntity)}'");

            return dataEntity;
        }

        Task<IList<string>> IRepository.CreateAsync(IEnumerable<IDataEntity> entities, CancellationToken cancellationToken)
            => CreateAsync(entities.Select(x =>  AssertEntity(x)), cancellationToken);

        public async Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) {
            var remove = Entities.Remove(entity);
            if (remove.State != EntityState.Deleted)
                return false;

            await Context.SaveChangesAsync(cancellationToken);

            return true;
        }

        Task<bool> IRepository.DeleteAsync(IDataEntity entity, CancellationToken cancellationToken)
            => DeleteAsync(AssertEntity(entity), cancellationToken);

        public async Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
            => await Entities.FindAsync(id, cancellationToken);

        public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }


        Task<bool> IRepository.UpdateAsync(IDataEntity entity, CancellationToken cancellationToken)
            => UpdateAsync(AssertEntity(entity), cancellationToken);

        async Task<IDataEntity?> IRepository.FindByIdAsync(string id, CancellationToken cancellationToken)
            => await FindByIdAsync(id, cancellationToken);

        async Task<TEntity?> IFilterableRepository<TEntity>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => await FindAsync(AssertExpression(filter), cancellationToken);

        async Task<IList<TEntity>> IFilterableRepository<TEntity>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => await FindAllAsync(AssertExpression(filter), cancellationToken);

        Task<bool> IFilterableRepository.ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => ExistsAsync(AssertExpression(filter), cancellationToken);

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
            => await Entities.AnyAsync(EnsureFilter(filter), cancellationToken);

        Task<long> IFilterableRepository.CountAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => CountAsync(AssertExpression(filter), cancellationToken);

        public async Task<long> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
            => await Entities.LongCountAsync(EnsureFilter(filter), cancellationToken);

        async Task<IDataEntity?> IFilterableRepository.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => await FindAsync(AssertExpression(filter), cancellationToken);

        public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) {
            return await Entities.FirstOrDefaultAsync(EnsureFilter(filter), cancellationToken);
        }

        async Task<IList<IDataEntity>> IFilterableRepository.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => (await FindAllAsync(AssertExpression(filter), cancellationToken)).Cast<IDataEntity>().ToList();

        private Expression<Func<TEntity, bool>> EnsureFilter(Expression<Func<TEntity, bool>>? filter) {
            if (filter == null)
                filter = e => true;

            return filter;
        }

        private Expression<Func<TEntity, bool>> AssertExpression(IQueryFilter filter) {
            if (filter == null || filter.IsEmpty())
                return e => true;

            if (!(filter is ExpressionQueryFilter<TEntity> queryFilter))
                throw new ArgumentException($"The filter of type {filter.GetType()} is not supported");

            return queryFilter.Expression;
        }

        public async Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) {
            return await Entities.Where(filter).ToListAsync(cancellationToken);
        }

        public IQueryable<TEntity> AsQueryable() {
            return Entities.AsQueryable();
        }
    }
}
