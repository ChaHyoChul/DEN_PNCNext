using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using PncNext.Infrastructure.Motion.Protocols.PA;

namespace PncNext.Infrastructure.Motion.Channels
{
    /// <summary>
    /// PA 모션 제어기 전용 통신 채널.
    /// </summary>
    public class PAMotionChannel : MotionChannelBase, IMotionChannel
    {
        private readonly PAMotionProtocol _protocol = new();

        public PAMotionChannel(ICommPort port) : base(port)
        {
        }

        protected override string ExtractCommandKey(byte[] response)
        {
            return _protocol.ExtractCommandKey(response);
        }

        public async Task StopAsync(int mode)
        {
            await SendAndReceiveAsync(_protocol.EncodeStop(mode));
            await SendAndReceiveAsync(_protocol.EncodeHalt());
        }

        public async Task ErrorResetAsync()
        {
            await SendAndReceiveAsync(_protocol.EncodeErrorReset());
        }

        public async Task InitControllerAsync()
        {
            await SendAndReceiveAsync(_protocol.EncodeInitController());
        }

        public async Task HomeAsync()
        {
            await SendAndReceiveAsync(_protocol.EncodeInitController());
            await SendAndReceiveAsync(_protocol.EncodeHome());
        }

        public async Task SetModeAsync(string mode)
        {
            await SendAndReceiveAsync(_protocol.EncodeMode(mode));
        }

        public async Task PauseAsync()
        {
            await SendAndReceiveAsync(_protocol.EncodePause());
        }

        public async Task ContinueAsync()
        {
            await SendAndReceiveAsync(_protocol.EncodeContinue());
        }

        public async Task MdaAsync(string gcode)
        {
            await SendAndReceiveAsync(_protocol.EncodeMda(gcode));
        }

        public async Task MoveIncrementalAsync(double? x, double? y, double? z, double? a, double? b)
        {
            await SendAndReceiveAsync(_protocol.EncodeMoveIncremental(x, y, z, a, b));
        }

        public async Task MoveAbsoluteAsync(double? x, double? y, double? z, double? a, double? b)
        {
            await SendAndReceiveAsync(_protocol.EncodeMoveAbsolute(x, y, z, a, b));
        }

        public async Task<MotionStatus> GetStatusAsync()
        {
            await ReadFullStatusAsync();
            return MotionStatus.Ready; // 상위 서비스에서 StateStore를 통해 판단
        }

        public async Task<byte[]> ReadFullStatusAsync()
        {
            return await SendAndReceiveAsync(_protocol.EncodeStatusRequest());
        }

        public async Task<byte[]> SendCustomCommandAsync(string command, params object[] args)
        {
            var cmdInfo = _protocol.EncodeCustom(command, args);
            return await SendAndReceiveAsync(cmdInfo);
        }

        public async Task<byte[]> ReceiveRawAsync()
        {
            return await ReadFullStatusAsync();
        }

        // PA 전용: 응답 데이터를 기반으로 상태 객체 업데이트
        public void UpdateState(byte[] data, PAMotionControllerState state)
        {
            _protocol.UpdateStateFromResponse(data, state);
        }
    }
}
