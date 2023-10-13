namespace EngineBay.DemoApi
{
    using System.Net;
    using FluentValidation;
    using Microsoft.AspNetCore.Diagnostics;

    public static class ExceptionMiddleware
    {
        public static RequestDelegate HandleError()
        {
            return async context =>
            {
                context.Response.ContentType = "application/problem+json";
                var title = "Unhandled Internal Server Error";
                var type = "UnhandledInternalServerError";

                if (context.RequestServices.GetService<IProblemDetailsService>() is
                    { } problemDetailsService)
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                    var exceptionType = exceptionHandlerFeature?.Error;
                    var messages = new List<string>();

                    switch (exceptionType)
                    {
                        case ValidationException validationException:
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            title = "Validation error";
                            type = HttpStatusCode.BadRequest.ToString();
                            messages.Add(validationException.Message);
                            break;
                        case KeyNotFoundException:
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            break;
                        default:
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            title = "Internal Server Error";
                            type = HttpStatusCode.InternalServerError.ToString();
                            messages.Add("An unhandled error occured. See logs for more details."); // we purposefull omit the details here so as to not over expose inner system workings.
                            break;
                    }

                    var detail = string.Join(" ", messages);
                    var protocol = context.Request.IsHttps ? "https://" : "http://";

                    await problemDetailsService.WriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = context,
                        ProblemDetails =
                        {
                            Title = title,
                            Detail = detail,
                            Type = type,
                            Status = context.Response.StatusCode,
                            Instance = $"{protocol}{context.Request.Host}{context.Request.Path}",
                        },
                    }).ConfigureAwait(false);
                }
            };
        }
    }
}