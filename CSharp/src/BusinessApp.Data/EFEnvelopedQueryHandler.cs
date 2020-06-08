namespace BusinessApp.Data
{
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System.Linq;
    using System.Threading;

    public class EFEnvelopedQueryHandler<TQuery, TResult> : IQueryHandler<TQuery, EnvelopeContract<TResult>>
        where TQuery : Query, IQuery<EnvelopeContract<TResult>>
        where TResult : class
    {
        private readonly BusinessAppReadOnlyDbContext db;
        private readonly IDbSetVisitorFactory<TQuery, TResult> dbSetFactory;
        private readonly IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory;

        public EFEnvelopedQueryHandler(BusinessAppReadOnlyDbContext db,
            IQueryVisitorFactory<TQuery, TResult> queryVisitorFactory,
            IDbSetVisitorFactory<TQuery, TResult> dbSetFactory)
        {
            this.db = GuardAgainst.Null(db, nameof(db));
            this.queryVisitorFactory = GuardAgainst.Null(queryVisitorFactory, nameof(queryVisitorFactory));
            this.dbSetFactory = GuardAgainst.Null(dbSetFactory, nameof(dbSetFactory));
        }

        public async Task<EnvelopeContract<TResult>> HandleAsync(TQuery query,
            CancellationToken cancellationToken)
        {
            var dbSetVisitor = dbSetFactory.Create(query);
            var queryVisitor = queryVisitorFactory.Create(query);
            var queryable = dbSetVisitor.Visit(db.Set<TResult>());

            // handle these values here
            var take = query.Limit;
            var skip = query.Offset ?? 0;
            query.Limit = null;
            query.Offset = null;

            var data = await queryVisitor.Visit(queryable).ToListAsync();

            return new EnvelopeContract<TResult>
            {
                Data = data.Skip(skip).Take(take ?? data.Count),
                Pagination = new Pagination
                {
                    ItemCount = data.Count
                }
            };
        }
    }
}