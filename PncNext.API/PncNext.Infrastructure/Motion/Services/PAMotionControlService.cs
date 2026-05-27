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

        public async Task StopAsync(int mode)
        {
            await GetChannel("STS").StopAsync(mode);
        }

        public async Task ErrorResetAsync()
        {
            await GetChannel("STS").ErrorResetAsync();
        }

        public async Task InitControllerAsync()
        {
            await GetChannel("CMD").InitControllerAsync();
        }

        public async Task HomeAsync()
        {
            // 원점 복귀 명령은 CMD 채널 사용
            await GetChannel("CMD").HomeAsync();
        }

        public async Task<MotionStatus> GetStatusAsync()
        {
            var channel = GetChannel("STS");
            
            if (channel.IsFaulted)
            {
                UpdateStoreToNotConnected();
                return MotionStatus.NotConnected;
            }

            try 
            {
                await channel.ReadFullStatusAsync();
                return _stateStore.PaState.ControllerState;
            }
            catch (Exception)
            {
                UpdateStoreToNotConnected();
                return MotionStatus.NotConnected;
            }
        }

        private void UpdateStoreToNotConnected()
        {
            if (_stateStore.PaState.ControllerState != MotionStatus.NotConnected)
            {
                _stateStore.PaState.ControllerState = MotionStatus.NotConnected;
                _stateStore.NotifyStateChanged("PA");
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
