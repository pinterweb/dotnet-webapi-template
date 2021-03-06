using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BusinessApp.WebApi.Json
{
    /// <summary>
    /// Logs JSON exception and convert the response to a <see cref="Result"/> type
    /// </summary>
    public class NewtonsoftJsonExceptionDecorator<TRequest, TResponse> : IHttpRequestHandler<TRequest, TResponse>
       where TRequest : notnull
    {
        private readonly IHttpRequestHandler<TRequest, TResponse> inner;
        private readonly ILogger logger;

        public NewtonsoftJsonExceptionDecorator(IHttpRequestHandler<TRequest, TResponse> inner, ILogger logger)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.logger = logger.NotNull().Expect(nameof(logger));
        }

        public async Task<Result<HandlerContext<TRequest, TResponse>, Exception>> HandleAsync(HttpContext context,
            CancellationToken cancelToken)
        {
            try
            {
                return await inner.HandleAsync(context, cancelToken);
            }
            catch (JsonSerializationException exception)
            {
                Log(exception);

                return Result.Error<HandlerContext<TRequest, TResponse>>(
                    new BadStateException("Your request could not be read because " +
                        "your payload is in an invalid format. Please review your data " +
                        "and try again"));
            }
        }

        private void Log(Exception exception) => logger.Log(LogEntry.FromException(exception));
    }
}
