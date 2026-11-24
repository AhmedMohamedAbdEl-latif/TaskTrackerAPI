namespace TaskTrackerAPI.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static WebApplication ConfigurePipeline(
            this WebApplication app)
        {
            // Must run before rate limiting and authentication so they use the
            // real client address, and only trusts proxies configured in DI.
            app.UseForwardedHeaders();

            // Global Exception Handling
            app.UseExceptionHandler();

            // Ensure empty framework responses (for example, an unknown route)
            // follow the same JSON error contract as controller responses.
            app.UseStatusCodePages(async statusContext =>
            {
                var httpContext = statusContext.HttpContext;
                var statusCode = httpContext.Response.StatusCode;
                var errorType = statusCode switch
                {
                    StatusCodes.Status400BadRequest => ServiceErrorType.Validation,
                    StatusCodes.Status401Unauthorized => ServiceErrorType.Unauthorized,
                    StatusCodes.Status403Forbidden => ServiceErrorType.Forbidden,
                    StatusCodes.Status404NotFound => ServiceErrorType.NotFound,
                    StatusCodes.Status409Conflict => ServiceErrorType.Conflict,
                    StatusCodes.Status429TooManyRequests => ServiceErrorType.TooManyRequests,
                    _ => ServiceErrorType.InternalServerError
                };

                var message = statusCode == StatusCodes.Status404NotFound
                    ? "The requested endpoint was not found."
                    : "The request could not be completed.";

                await httpContext.Response.WriteAsJsonAsync(
                    new ServiceResult
                    {
                        Success = false,
                        ErrorType = errorType,
                        Errors = new List<string> { message },
                        TraceId = httpContext.TraceIdentifier
                    },
                    httpContext.RequestAborted);
            });

            // Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AngularPolicy");

            app.UseRateLimiter();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
