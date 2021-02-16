namespace BusinessApp.WebApi
{
    using BusinessApp.App;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using System;
    using System.Threading;

    /// <summary>
    /// Wraps the actual request handler implementation that is used
    /// by the <see cref="BatchRequestDelegator{TRequest, TResponse}" /> so the decorator
    /// pipeline is not injected.
    /// </summary>
    [Proxy(typeof(ValidationRequestDecorator<,>))]
    // TODO this is really a request proxy
    internal class BatchScopeWrappingHandler<TConsumer, TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TConsumer : IRequestHandler<TRequest, TResponse>
    {
        private readonly TConsumer inner;

        public BatchScopeWrappingHandler(TConsumer inner)
        {
            this.inner = inner;
        }

        public Task<Result<TResponse, IFormattable>> HandleAsync(TRequest command,
            CancellationToken cancellationToken)
        {
            return inner.HandleAsync(command, cancellationToken);
        }
    }
}
