using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OnlineVoting.Api.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (object? argument in context.ActionArguments.Values)
            {
                if (argument is null)
                    continue;

                Type modelType = argument.GetType();

                Type validatorType = typeof(IValidator<>).MakeGenericType(modelType);

                object? validatorService = _serviceProvider.GetService(validatorType);

                if (validatorService is not IValidator validator)
                    continue;

                ValidationContext<object> validationContext = ValidationContext<object>.CreateWithOptions(argument, options => { });

                ValidationResult validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                if (validationResult.IsValid)
                    continue;

                Dictionary<string, string[]> errors = validationResult.Errors.GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

                ValidationProblemDetails problemDetails = new(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed.",
                    Detail = "One or more validation errors occurred.",
                    Type = "https://tools.ietf.org/html/" + "rfc9110#section-15.5.1",
                    Instance = context.HttpContext.Request.Path
                };

                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                BadRequestObjectResult result = new(problemDetails);

                result.ContentTypes.Add("application/problem+json");

                context.Result = result;

                return;
            }

            await next();
        }
    }
}