﻿namespace BusinessApp.WebApi
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;

    /// <summary>
    /// Catches all errors during the execution of a http request
    /// </summary>
    public sealed class HttpExceptionMiddleware : IMiddleware
    {
        private readonly ILogger logger;
        private readonly IResponseWriter writer;

        public HttpExceptionMiddleware(ILogger logger, IResponseWriter writer)
        {
            this.logger = logger.NotNull().Expect(nameof(logger));
            this.writer = writer.NotNull().Expect(nameof(writer));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                logger.Log(new LogEntry(LogSeverity.Error, exception.Message, exception));

                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    if (exception is IFormattable f)
                    {
                        await writer.WriteResponseAsync(context, Result.Error(f).Into());
                    }
                    else
                    {
                        await writer.WriteResponseAsync(context);
                    }
                }
            }
        }
    }
}