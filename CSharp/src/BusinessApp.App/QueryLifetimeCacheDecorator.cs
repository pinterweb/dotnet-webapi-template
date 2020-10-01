namespace BusinessApp.App
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Caches the query results for the lifetime of the class
    /// </summary>
    public class QueryLifetimeCacheDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly ConcurrentDictionary<TQuery, Result<TResult, IFormattable>> cache;
        private readonly IQueryHandler<TQuery, TResult> inner;

        public QueryLifetimeCacheDecorator(IQueryHandler<TQuery, TResult> inner)
        {
            this.inner = Guard.Against.Null(inner).Expect(nameof(inner));
            cache = new ConcurrentDictionary<TQuery, Result<TResult, IFormattable>>();
        }

        public async Task<Result<TResult, IFormattable>> HandleAsync(
            TQuery query, CancellationToken cancellationToken)
        {
            if (cache.TryGetValue(query, out Result<TResult, IFormattable> cachedResult))
            {
                return cachedResult;
            }

            var result = await inner.HandleAsync(query, cancellationToken);

            var _ = cache.TryAdd(query, result);

            return result;
        }
    }
}
