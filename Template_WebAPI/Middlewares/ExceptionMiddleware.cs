using System.Net;
using System.Text.Json;
using Template_WebAPI.Helpers.Errors;

namespace Template_WebAPI.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            this.next = next;
            _logger = logger;
            _env = env;
        }

        //contiene la información de la solicitud HTTP que se esta realizando
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context); // si no hay una excepción continuamos con el siguiente paso
            }
            catch (Exception ex)
            {

                var statusCode = (int)HttpStatusCode.InternalServerError;

                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;

                var response = _env.IsDevelopment()
                                ? new ExceptionAPI(statusCode, ex.Message, ex.StackTrace.ToString())
                                : new ExceptionAPI(statusCode);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
