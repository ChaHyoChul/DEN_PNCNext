using PncNext.Domain.Interfaces;

namespace PncNext.API.Services
{
    public class MotionStatusBackgroundService : BackgroundService
    {
        private readonly ILogger<MotionStatusBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MotionStatusBackgroundService(
            ILogger<MotionStatusBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Motion Status Monitoring Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var motionControl = scope.ServiceProvider.GetService<IMotionControl>();
                        if (motionControl != null)
                        {
                            // In a real scenario, we might check status and notify clients via SignalR
                            // var status = await motionControl.GetStatusAsync();
                            // _logger.LogInformation("Current Motion Status: {Status}", status);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while monitoring motion status.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation("Motion Status Monitoring Service is stopping.");
        }
    }
}
