using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Assistant_API.Filters;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var arg = context.Arguments.SingleOrDefault(a => a?.GetType() == typeof(T)) as T;
        if (arg == null)
        {
            return Results.BadRequest("Invalid request data.");
        }

        var validationResult = await _validator.ValidateAsync(arg);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        return await next(context);
    }
}
