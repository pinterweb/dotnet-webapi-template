﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BusinessApp.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Decorator to add the web link headers for the particular response
    /// </summary>
    public class WeblinkingHeaderRequestDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
       where TRequest : notnull
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> handler;
        private readonly IEnumerable<HateoasLink<TRequest, TResponse>> links;

        public WeblinkingHeaderRequestDecorator(IHttpRequestHandler<TRequest, TResponse> handler,
            IEnumerable<HateoasLink<TRequest, TResponse>> links)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
            this.links = links.NotNull().Expect(nameof(links));
        }

        public virtual Task<Result<HandlerContext<TRequest, TResponse>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken)
            => handler.HandleAsync(context, cancelToken)
                    .MapAsync(okVal =>
                    {
                        var headerLinks = links.Select(l =>
                            l.ToHeaderValue(context.Request, okVal.Request, okVal.Response));

                        if (headerLinks.Any())
                        {
                            context.Response.Headers.Add("Link", headerLinks.ToArray());
                        }

                        return okVal;
                    });
    }
}
