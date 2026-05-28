using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Motion.Channels;

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
            x ??= 0;
            y ??= 0;
            z ??= 0;
            a ??= 0;
            b ??= 0;
            await GetChannel("CMD").MoveIncrementalAsync(x, y, z, a, b);
        }

        public async Task MoveAbsoluteAsync(double? x, double? y, double? z, double? a, double? b)
        {
            // INTH 제어기용 상태 저장소 프로퍼티가 아직 명확하지 않으므로 임시로 0 처리하거나
            // 추후 INTHState가 정의되면 해당 값을 참조하도록 수정 필요.
            // 현재는 인터페이스 일관성을 위해 0으로 기본값 설정.
            x ??= 0;
            y ??= 0;
            z ??= 0;
            a ??= 0;
            b ??= 0;
            await GetChannel("CMD").MoveAbsoluteAsync(x, y, z, a, b);
        }

        public async Task<MotionStatus> GetStatusAsync()
        {
            var channel = GetChannel("STS");
            
            if (channel.IsFaulted)
            {
                return MotionStatus.NotConnected;
            }

            try 
            {
                await channel.ReadFullStatusAsync();
                return MotionStatus.NotConnected; // 임시
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
