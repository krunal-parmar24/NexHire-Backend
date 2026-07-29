using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NexHire.Application.Exceptions;

namespace NexHire.Api.Filters
{
    /// <summary>
    /// Global MVC exception filter that maps domain exceptions to the API's standard
    /// <c>{ error: { code, message } }</c> envelope, replacing the identical per-action
    /// try/catch blocks previously duplicated across controllers.
    /// </summary>
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            context.Result = context.Exception switch
            {
                NotFoundException ex => new NotFoundObjectResult(new { error = new { code = ex.Code, message = ex.Message } }),
                ConflictException ex => new ConflictObjectResult(new { error = new { code = ex.Code, message = ex.Message } }),
                AuthenticationException ex => new UnauthorizedObjectResult(new { error = new { code = ex.Code, message = ex.Message } }),
                ArgumentException ex => new BadRequestObjectResult(new { error = new { code = "VALIDATION_ERROR", message = ex.Message } }),
                UnauthorizedAccessException => new ForbidResult(),
                _ => null
            };

            if (context.Result != null)
            {
                context.ExceptionHandled = true;
                _logger.LogWarning(context.Exception, "Handled {ExceptionType} as {ResultType}", context.Exception.GetType().Name, context.Result.GetType().Name);
            }
        }
    }
}
