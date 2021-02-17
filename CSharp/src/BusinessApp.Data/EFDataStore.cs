namespace BusinessApp.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Entity Framework implementation of the <see cref="IDatastore{TEntity}"/> that
    /// provides query access into the database as well as searching the local identity map
    /// </summary>
    /// <remarks>Use in your repositories</remarks>
    public class EFDatastore<TEntity> : IDatastore<TEntity> where TEntity : class
    {
        private readonly ILinqSpecificationBuilder<IQuery, TEntity> linqBuilder;
        private readonly IQueryVisitorFactory<IQuery, TEntity> queryVisitorFactory;
        private readonly BusinessAppDbContext db;

        public EFDatastore(ILinqSpecificationBuilder<IQuery, TEntity> linqBuilder,
            IQueryVisitorFactory<IQuery, TEntity> queryVisitorFactory,
            BusinessAppDbContext db)
        {
            this.linqBuilder = linqBuilder.NotNull().Expect(nameof(linqBuilder));
            this.queryVisitorFactory = queryVisitorFactory.NotNull().Expect(nameof(queryVisitorFactory));
            this.db = db.NotNull().Expect(nameof(db));
        }

        public async Task<IEnumerable<TEntity>> QueryAsync(IQuery query,
            CancellationToken cancelToken)
        {
            var filter = linqBuilder.Build(query);
            var entities = FindInIdentityMap(filter.Predicate.Compile());

            if (!entities.Any())
            {
                entities = await queryVisitorFactory
                    .Create(query)
                    .Visit(db.Set<TEntity>())
                    .ToListAsync();
            }

            return entities;
        }

        private IEnumerable<TEntity> FindInIdentityMap(Func<TEntity, bool> filter) =>
            db.Set<TEntity>().Local.Where(filter);
    }
}
