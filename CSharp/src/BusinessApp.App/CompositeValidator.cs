namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Runs multiple validators for one instance of <typeparam name="T">T</typeparam>
    /// </summary>
    public class CompositeValidator<T> : IValidator<T>
    {
        private readonly IEnumerable<IValidator<T>> validators;

        public CompositeValidator(IEnumerable<IValidator<T>> validators)
        {
            this.validators = Guard.Against.Null(validators).Expect(nameof(validators));
        }

        public async Task<Result> ValidateAsync(T instance, CancellationToken cancellationToken)
        {
            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(instance, cancellationToken);

                if (result.Kind == ValueKind.Error)
                {
                    return result;
                }
            }

            return Result.Ok;
        }
    }
}
