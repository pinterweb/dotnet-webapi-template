namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class MacroBatchRequestDelegator<TMacro, TRequest, TResponse>
        : IRequestHandler<TMacro, TResponse>
        where TMacro : IMacro<TRequest>
    {
        private readonly IBatchMacro<TMacro, TRequest> expander;
        private readonly IRequestHandler<IEnumerable<TRequest>, TResponse> handler;

        public MacroBatchRequestDelegator(
            IBatchMacro<TMacro, TRequest> expander,
            IRequestHandler<IEnumerable<TRequest>, TResponse> handler)
        {
            this.expander = expander.NotNull().Expect(nameof(expander));
            this.handler = handler.NotNull().Expect(nameof(handler));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(
            TMacro macro,
            CancellationToken cancellationToken)
        {
            macro.NotNull().Expect(nameof(macro));

            var payloads = await expander.ExpandAsync(macro, cancellationToken);

            if (!payloads.Any())
            {
                throw new BusinessAppAppException(
                    "The macro you ran expected to find records to change, but none were " +
                    "found"
                );
            }

            return await handler.HandleAsync(payloads, cancellationToken);
        }
    }
}
