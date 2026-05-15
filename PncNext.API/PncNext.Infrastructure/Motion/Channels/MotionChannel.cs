using PncNext.Domain.Interfaces;

namespace PncNext.Infrastructure.Motion.Channels
{
    /// <summary>
    /// ICommPort(전송)와 IMotionProtocol(인코딩)을 결합한 IMotionChannel 구현체.
    /// Decorator 패턴 또는 Wrapper 형태로 동작합니다.
    /// </summary>
    public class MotionChannel : IMotionChannel
    {
        private readonly ICommPort _port;
        private readonly IMotionProtocol _protocol;

        public MotionChannel(ICommPort port, IMotionProtocol protocol)
        {
            _port = port;
            _protocol = protocol;
        }

        public bool IsOpen => _port.IsOpen;

        public async Task OpenAsync() => await _port.OpenAsync();

        public async Task CloseAsync() => await _port.CloseAsync();

        public async Task MoveAsync(double x, double y, double z, double a, double b)
        {
            if (!IsOpen) await OpenAsync();
            var data = _protocol.EncodeMove(x, y, z, a, b);
            await _port.SendAsync(data);
        }

        public async Task StopAsync()
        {
            if (!IsOpen) await OpenAsync();
            var data = _protocol.EncodeStop();
            await _port.SendAsync(data);
        }

        public async Task<MotionStatus> GetStatusAsync()
        {
            if (!IsOpen) await OpenAsync();
            var request = _protocol.EncodeStatusRequest();
            await _port.SendAsync(request);
            var response = await _port.ReceiveAsync();
            return _protocol.DecodeStatus(response);
        }

        public async Task<byte[]> SendCustomCommandAsync(string command, params object[] args)
        {
            if (!IsOpen) await OpenAsync();
            var data = _protocol.EncodeCustom(command, args);
            await _port.SendAsync(data);
            return await _port.ReceiveAsync();
        }
    }
}
