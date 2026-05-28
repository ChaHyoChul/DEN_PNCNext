using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Motion.Protocols.PA;
using PncNext.Infrastructure.Motion.Channels;

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
            if (sender is PAMotionChannel paChannel)
            {
                paChannel.UpdateState(data, _stateStore.PaState);
                _stateStore.NotifyStateChanged("PA");
            }
        }

        private IMotionChannel GetChannel(string purpose) 
        {
            if (_channels.TryGetValue(purpose, out var channel)) return channel;
            return _channels.Values.First();
        }

        public async Task StopAsync(int mode)
        {
            await GetChannel("CMD").StopAsync(mode);
        }

        public async Task ErrorResetAsync()
        {
            await GetChannel("CMD").ErrorResetAsync();
        }

        public async Task InitControllerAsync()
        {
            await GetChannel("CMD").InitControllerAsync();
        }

        public async Task HomeAsync()
        {
            await GetChannel("CMD").HomeAsync();
        }

        public async Task SetModeAsync(string mode)
        {
            await GetChannel("CMD").SetModeAsync(mode);
        }

        public async Task PauseAsync()
        {
            await GetChannel("CMD").PauseAsync();
        }

        public async Task ContinueAsync()
        {
            await GetChannel("CMD").ContinueAsync();
        }

        public async Task MdaAsync(string gcode)
        {
            await GetChannel("CMD").MdaAsync(gcode);
        }

        public async Task MoveIncrementalAsync(double? x, double? y, double? z, double? a, double? b)
        {
            // MMI: 입력되지 않은 축은 0으로 설정하여 이동하지 않도록 함
            x ??= 0;
            y ??= 0;
            z ??= 0;
            a ??= 0;
            b ??= 0;
            await GetChannel("CMD").MoveIncrementalAsync(x, y, z, a, b);
        }

        public async Task MoveAbsoluteAsync(double? x, double? y, double? z, double? a, double? b)
        {
            // MMA: 입력되지 않은 축은 현재 위치를 입력하여 해당 축이 이동하지 않도록 함
            var currentPos = _stateStore.PaState.Position;
            x ??= currentPos[0];
            y ??= currentPos[1];
            z ??= currentPos[2];
            a ??= currentPos[3];
            b ??= currentPos[4];
            await GetChannel("CMD").MoveAbsoluteAsync(x, y, z, a, b);
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
