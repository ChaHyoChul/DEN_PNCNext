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

            foreach (var channel in _channels.Values)
            {
                channel.MessageReceived += OnMessageReceived;
            }
        }

        public async Task OpenAsync()
        {
            foreach (var channel in _channels.Values)
            {
                await channel.OpenAsync();
            }
        }

        public async Task CloseAsync()
        {
            foreach (var channel in _channels.Values)
            {
                await channel.CloseAsync();
            }
        }

        private void OnMessageReceived(object? sender, byte[] data)
        {
            if (sender is IMotionChannel channel && channel.Protocol is PAMotionProtocol paProtocol)
            {
                paProtocol.UpdateStateFromResponse(data, _stateStore.PaState);
                _stateStore.NotifyStateChanged("PA");
            }
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
            var channel = GetChannel("STS");
            
            // 통신 단절 상태 우선 체크
            if (!channel.IsOpen || channel.IsFaulted)
            {
                return MotionStatus.NotConnected;
            }

            try 
            {
                var response = await channel.ReadFullStatusAsync();
                return channel.Protocol.DecodeStatus(response);
            }
            catch (Exception)
            {
                return MotionStatus.NotConnected;
            }
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
