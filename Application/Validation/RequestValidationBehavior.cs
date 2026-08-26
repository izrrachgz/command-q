using Cqrs.Abstractions;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Validation
{
    internal sealed class RequestValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TRequest, TResponse> next,
            CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);
            List<ValidationFailure> failures = validators
                .Select(validator => validator.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .Select(error => new ValidationFailure(error.PropertyName, error.ErrorMessage))
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }

            return next(request, cancellationToken);
        }
    }
}
