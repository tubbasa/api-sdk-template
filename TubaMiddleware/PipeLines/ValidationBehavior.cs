using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace TubaMiddleware.PipeLines
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;


        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var failRules = _validators.Select(v => v.Validate(request))
                .SelectMany(result => result.Errors)
                .Where(f => f != null).ToList();
            if (failRules.Any()) throw new ValidationException("ValidationBehavior", failRules);

            return next();
        }
    }
}