using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Motion.Protocols.PA;

namespace PncNext.Infrastructure.Motion.Services
{
    public class PAMotionControlService : IMotionControl, IDisposable
    {
        private readonly IDictionary<string, IMotionChannel> _channels;
        private readonly IMotionStateStore _stateStore;

        public PAMotionControlService(IDictionary<string, IMotionChannel> channels, IMotionStateStore stateStore)
        {
            _channels = channels;
            _stateStore = stateStore;

            // 모든 채널의 수신 이벤트를 구독하여 상태를 자동 업데이트
            foreach (var channel in _channels.Values)
            {
                channel.MessageReceived += OnMessageReceived;
            }
        }

        private void OnMessageReceived(object? sender, byte[] data)
        {
            if (sender is IMotionChannel channel && channel.Protocol is PAMotionProtocol paProtocol)
            {
                // 실시간 수신된 데이터를 분석하여 전역 상태 저장소 업데이트
                paProtocol.UpdateStateFromResponse(data, _stateStore.PaState);
                _stateStore.NotifyStateChanged("PA");
            }
        }

        private IMotionChannel GetChannel(string purpose) 
        {
            if (_channels.TryGetValue(purpose, out var channel)) return channel;
            return _channels.Values.First(); // Fallback
        }

        public async Task MoveAsync(double x, double y, double z, double a, double b)
        {
            throw new NotImplementedException("Move feature is currently being reorganized.");
        }

        public async Task StopAsync()
        {
            throw new NotImplementedException("Stop feature is currently being reorganized.");
        }

        public async Task<MotionStatus> GetStatusAsync()
        {
            // 비동기 엔진 방식에서는 명시적인 Receive 대신 명령만 전송하거나
            // 이미 수신 루프에서 업데이트된 최신 상태를 반환함
            var response = await GetChannel("STS").ReadFullStatusAsync();
            return GetChannel("STS").Protocol.DecodeStatus(response);
        }

        public void Dispose()
        {
            foreach (var channel in _channels.Values)
            {
                channel.MessageReceived -= OnMessageReceived;
            }
        }
    }
}
