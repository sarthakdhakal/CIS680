namespace NewDotnet.Middleware
{
    public class CacheDisablerMiddleware
    {
        private readonly RequestDelegate _next;

        public CacheDisablerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Intercept the response start to modify headers
            context.Response.OnStarting(() =>
            {
                var response = context.Response;
                response.Headers["Cache-Control"] = "private, max-age=0, no-cache";
                response.Headers["Pragma"] = "no-cache";
                response.Headers["ETag"] = Guid.NewGuid().ToString();
                return Task.CompletedTask;
            });

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

    public static class CacheDisablerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCacheDisabler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CacheDisablerMiddleware>();
        }
    }
}
