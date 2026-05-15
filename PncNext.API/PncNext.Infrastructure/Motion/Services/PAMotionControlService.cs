using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Motion.Protocols.PA;

namespace PncNext.Infrastructure.Motion.Services
{
    public class PAMotionControlService : IMotionControl
    {
        private readonly IDictionary<string, IMotionChannel> _channels;
        private readonly IMotionStateStore _stateStore;

        public PAMotionControlService(IDictionary<string, IMotionChannel> channels, IMotionStateStore stateStore)
        {
            _channels = channels;
            _stateStore = stateStore;
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
            var channel = GetChannel("STS");
            
            // 1. 하드웨어로부터 원시 응답 데이터를 읽어옴
            var response = await channel.ReceiveRawAsync();
            
            // 2. 프로토콜 분석 및 상세 상태 저장 (PA 전용 로직)
            if (channel.Protocol is PAMotionProtocol paProtocol)
            {
                paProtocol.UpdateStateFromResponse(response, _stateStore.PaState);
                _stateStore.NotifyStateChanged(); // 변경 알림 (필요 시 공유 메모리 기록)
            }

            // 3. 도메인 공통 상태 반환
            return channel.Protocol.DecodeStatus(response);
        }
    }
}
