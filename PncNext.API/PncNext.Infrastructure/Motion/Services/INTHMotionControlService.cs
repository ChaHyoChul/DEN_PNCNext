using PncNext.Domain.Interfaces;

namespace PncNext.Infrastructure.Motion.Services
{
    public class INTHMotionControlService : IMotionControl
    {
        private readonly IDictionary<string, IMotionChannel> _channels;
        private readonly IMotionStateStore _stateStore;

        public INTHMotionControlService(IDictionary<string, IMotionChannel> channels, IMotionStateStore stateStore)
        {
            _channels = channels;
            _stateStore = stateStore;
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
            var channel = GetChannel("STS");
            
            // 1. 하드웨어로부터 원시 응답 데이터를 읽어옴
            var response = await channel.ReceiveRawAsync();
            
            // 2. INTH 제어기 상태 업데이트 (향후 INTH 전용 파싱 로직 추가 가능)
            // if (channel.Protocol is INTHMotionProtocol inthProtocol) { ... }

            // 3. 도메인 공통 상태 반환
            return channel.Protocol.DecodeStatus(response);
        }
    }
}
