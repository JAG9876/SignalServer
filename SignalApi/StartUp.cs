using System.Threading.RateLimiting;
using Serilog;

namespace SignalApi
{
    public class StartUp
    {
        const int TIME_WINDOW = 60;
        const int MAX_REQUESTS_PER_DEVICE = 3;
        public IConfiguration Configuration { get; }

        public StartUp(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddSingleton<IMessageProducer, RabbitMQProducer>();

            services.AddRateLimiter(options =>
            {
                options.AddPolicy("PerDevicePolicy", context =>
                {
                    //var deviceId = context.Request.Headers["DeviceId"].FirstOrDefault();
                    var deviceId = "123";
                    if (string.IsNullOrEmpty(deviceId))
                    {
                        return RateLimitPartition.GetFixedWindowLimiter("InvalidDevice", _ => new FixedWindowRateLimiterOptions
                        {
                            //PermitLimit = 0, // => This fails when ctor is called. TODO: Find a better way to handle requests without device ID.
                            PermitLimit = 1, // => Workaround for now
                            Window = TimeSpan.FromSeconds(TIME_WINDOW),
                            QueueLimit = 0
                        });
                    }
                    return RateLimitPartition.GetSlidingWindowLimiter(partitionKey: deviceId, factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromSeconds(TIME_WINDOW),
                        PermitLimit = MAX_REQUESTS_PER_DEVICE,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        SegmentsPerWindow = 1
                    });
                });
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<StartUp>>();
                    var deviceId = context.HttpContext.Request.Headers["DeviceId"].FirstOrDefault();
                    logger.LogWarning($"Request from device {deviceId} was rejected due to rate limiting.");
                    context.HttpContext.Response.Headers["Retry-After"] = $"{TIME_WINDOW}"; // Suggest client to retry after TIME_WINDOW seconds
                    await Task.CompletedTask;
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SignalApi");
                });
            }
            app.UseRouting();
            app.UseSerilogRequestLogging();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
