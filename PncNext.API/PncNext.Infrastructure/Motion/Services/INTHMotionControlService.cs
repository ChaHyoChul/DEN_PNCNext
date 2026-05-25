using PncNext.Domain.Interfaces;

namespace PncNext.Infrastructure.Motion.Services
{
    public class INTHMotionControlService : IMotionControl, IDisposable
    {
        private readonly IDictionary<string, IMotionChannel> _channels;
        private readonly IMotionStateStore _stateStore;

        public INTHMotionControlService(IDictionary<string, IMotionChannel> channels, IMotionStateStore stateStore)
        {
            _channels = channels;
            _stateStore = stateStore;

            foreach (var channel in _channels.Values)
            {
                channel.MessageReceived += OnMessageReceived;
            }
        }

        private void OnMessageReceived(object? sender, byte[] data)
        {
            // INTH 전용 파싱 및 업데이트 로직 (미구현)
            // _stateStore.NotifyStateChanged("INTH");
        }

        private IMotionChannel GetChannel(string purpose)
        {
            if (_channels.TryGetValue(purpose, out var channel)) return channel;
            return _channels.Values.First();
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
