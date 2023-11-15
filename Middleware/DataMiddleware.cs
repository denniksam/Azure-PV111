namespace Azure_PV111.Middleware
{
    public class DataMiddleware : IMiddleware
    {
        public static List<String> Data { get; set; } = new();
        /*
        private readonly RequestDelegate _next;

        public DataMiddleware(RequestDelegate next)
        {
            _next = next;
            Data = new();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
        }
        */
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return next.Invoke(context);
        }
    }
}
