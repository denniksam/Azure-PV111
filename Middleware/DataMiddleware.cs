namespace Azure_PV111.Middleware
{
    public class DataMiddleware
    {
        private readonly RequestDelegate _next;

        public DataMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
        }
    }
}
