using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Validates the command prior to handling
    /// </summary>
    public class ValidationRequestDecorator<TRequest, TResult> :
        IRequestHandler<TRequest, TResult>
        where TRequest : notnull
    {
        private readonly IValidator<TRequest> validator;
        private readonly IRequestHandler<TRequest, TResult> inner;

        public ValidationRequestDecorator(IValidator<TRequest> validator, IRequestHandler<TRequest, TResult> inner)
        {
            this.validator = validator.NotNull().Expect(nameof(validator));
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<TResult, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            request.NotNull().Expect(nameof(request));

            var result = await validator.ValidateAsync(request, cancelToken);

            return result.Kind switch
            {
                ValueKind.Error => Result.Error<TResult>(result.UnwrapError()),
                ValueKind.Ok => await inner.HandleAsync(request, cancelToken),
                _ => throw new NotImplementedException()
            };
        }
    }
}
