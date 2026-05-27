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

        public async Task MoveAsync(double x, double y, double z, double a, double b)
        {
            throw new NotImplementedException("Move feature is currently being reorganized.");
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
