using PncNext.Domain.Interfaces;

namespace PncNext.Infrastructure.Motion.Services
{
    public class INTHMotionControlService : IMotionControl
    {
        private readonly IDictionary<string, IMotionChannel> _channels;

        public INTHMotionControlService(IDictionary<string, IMotionChannel> channels)
        {
            _channels = channels;
        }

        private IMotionChannel GetChannel(string purpose)
        {
            if (_channels.TryGetValue(purpose, out var channel)) return channel;
            return _channels.Values.First();
        }

        public async Task MoveAsync(double x, double y, double z, double a, double b)
        {
            await GetChannel("CMD").MoveAsync(x, y, z, a, b);
        }

        public async Task StopAsync()
        {
            await GetChannel("CMD").StopAsync();
        }

        public async Task<MotionStatus> GetStatusAsync()
        {
            return await GetChannel("STS").GetStatusAsync();
        }
    }
}
