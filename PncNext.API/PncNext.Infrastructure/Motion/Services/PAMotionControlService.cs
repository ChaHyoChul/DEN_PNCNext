using PncNext.Domain.Interfaces;

namespace PncNext.Infrastructure.Motion.Services
{
    public class PAMotionControlService : IMotionControl
    {
        private readonly IDictionary<string, IMotionChannel> _channels;

        // PA 제어기는 여러 채널을 사용함 (CMD, STS, LOG, EVT 등)
        // 각 채널은 이미 자신의 전용 프로토콜을 내장하고 있음
        public PAMotionControlService(IDictionary<string, IMotionChannel> channels)
        {
            _channels = channels;
        }

        private IMotionChannel GetChannel(string purpose) 
        {
            if (_channels.TryGetValue(purpose, out var channel)) return channel;
            return _channels.Values.First(); // Fallback
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
