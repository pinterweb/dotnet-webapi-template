﻿namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System;

    /// <summary>
    /// Basic http handler for queries
    /// </summary>
    public class QueryResourceHandler<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
        where TRequest : class, IQuery<TResponse>, new()
    {
        private readonly IQueryHandler<TRequest, TResponse> handler;
        private readonly ISerializer serializer;

        public QueryResourceHandler(
            IQueryHandler<TRequest, TResponse> handler,
            ISerializer serializer)
        {
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
            this.serializer = Guard.Against.Null(serializer).Expect(nameof(serializer));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(HttpContext context,
            CancellationToken cancellationToken)
        {
            var query = await context.DeserializeIntoAsync<TRequest>(serializer, cancellationToken);

            if (query == null)
            {
                query = new TRequest();
            }

            return await handler.HandleAsync(query, cancellationToken);
        }
    }
}
