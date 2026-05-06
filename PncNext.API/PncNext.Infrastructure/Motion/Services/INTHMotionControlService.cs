using PncNext.Domain.Interfaces;

namespace PncNext.Infrastructure.Motion.Services
{
    public class INTHMotionControlService : IMotionControl
    {
        private readonly ICommPort _commPort;
        private readonly IMotionProtocol _protocol;

        public INTHMotionControlService(ICommPort commPort, IMotionProtocol protocol)
        {
            _commPort = commPort;
            _protocol = protocol;
        }

        public async Task MoveAsync(double x, double y, double z, double a, double b)
        {
            if (!_commPort.IsOpen) await _commPort.OpenAsync();
            var data = _protocol.EncodeMove(x, y, z, a, b);
            await _commPort.SendAsync(data);
        }

        public async Task StopAsync()
        {
            if (!_commPort.IsOpen) await _commPort.OpenAsync();
            var data = _protocol.EncodeStop();
            await _commPort.SendAsync(data);
        }

        public async Task<MotionStatus> GetStatusAsync()
        {
            if (!_commPort.IsOpen) await _commPort.OpenAsync();
            var request = _protocol.EncodeStatusRequest();
            await _commPort.SendAsync(request);
            var response = await _commPort.ReceiveAsync();
            return _protocol.DecodeStatus(response);
        }
    }
}
