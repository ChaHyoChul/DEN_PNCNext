using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using PncNext.Infrastructure.Motion.Protocols.Dummy;

namespace PncNext.Infrastructure.Motion.Channels
{
    /// <summary>
    /// 시뮬레이션용 Dummy 통신 채널.
    /// </summary>
    public class DummyMotionChannel : MotionChannelBase, IMotionChannel
    {
        private readonly DummyProtocol _protocol = new();

        public DummyMotionChannel(ICommPort port) : base(port)
        {
        }

        protected override string ExtractCommandKey(byte[] response)
        {
            return _protocol.ExtractCommandKey(response);
        }

        public async Task StopAsync(int mode)
        {
            await SendAndReceiveAsync(_protocol.EncodeStop(mode));
        }

        public async Task SetServoAsync(bool on) { }
        public async Task StartJogAsync(int axis, int direction) { }
        public async Task StopJogAsync() { }
        public async Task SetJogSpeedAsync(int speed) { }
        public async Task<int> GetJogSpeedAsync() => 100;
        public async Task SetOutputAsync(int bitNo, bool on) { }
        public async Task InitSpindleAsync() { }

        // [2단계] 파라미터 스텁
        public async Task<double[]> GetCoordinateOffsetAsync(int index) => new double[5];
        public async Task SetCoordinateOffsetAsync(int index, double[] values) { }
        public async Task<double[]> GetTeachingPointAsync(int index) => new double[5];
        public async Task SetTeachingPointAsync(int index, double[] values) { }
        public async Task<double> GetZOriginOffsetAsync() => 0;
        public async Task SetZOriginOffsetAsync(double offset) { }
        public async Task<int> GetToolSensingHighSpeedAsync() => 1000;
        public async Task SetToolSensingHighSpeedAsync(int speed) { }
        public async Task<int> GetToolSensingLowSpeedAsync() => 100;
        public async Task SetToolSensingLowSpeedAsync(int speed) { }
        public async Task<double> GetToolSensingMarginAsync() => 5.0;
        public async Task SetToolSensingMarginAsync(double margin) { }
        public async Task<double> GetToolPocketPutOffsetAsync() => 1.5;
        public async Task SetToolPocketPutOffsetAsync(double offset) { }
        public async Task<double[]> GetSoftLimitPositiveAsync() => new double[6] { 100, 100, 100, 360, 360, 100 };
        public async Task SetSoftLimitPositiveAsync(double[] values) { }
        public async Task<double[]> GetSoftLimitNegativeAsync() => new double[6] { -100, -100, -100, -360, -360, -100 };
        public async Task SetSoftLimitNegativeAsync(double[] values) { }

        // [3단계] 시스템 설정 스텁
        public async Task<string> GetControllerIpAsync() => "192.168.0.100";
        public async Task SetControllerIpAsync(string ip) { }
        public async Task<string> GetIoBoardIpAsync() => "192.168.0.101";
        public async Task SetIoBoardIpAsync(string ip) { }
        public async Task<string> GetFirmwareVersionAsync() => "DUMMY-V1";
        public async Task SaveToFlashAsync() { }
        public async Task RestoreToolInfoAsync(int toolNo, double length, bool updated) { }

        // [2.17, 2.18] 자동 보정 및 설정 스텁
        public async Task StartMeasureAsync(int axisNo, double inPitch, double outPitch, int speed, int count, double maxDist, double offset) { }
        public async Task<double> GetMeasureResultAsync() => 0;
        public async Task SetupSuhoAsync() { }
        public async Task SetupSabhoAsync() { }
        public async Task SetupSorzAsync() { }
        public async Task SetDiskThicknessAsync(double thickness) { }
        public async Task SetM28TypeAsync(int type) { }
        public async Task<int> GetM28TypeAsync() => 0;
        public async Task ResetHomingStatusAsync() { }
        public async Task SetAirParametersAsync(int usingAir, int interval, int usingPurge, int purgeInterval) { }
        public async Task SetWaterFlowParametersAsync(int usingWater, int startTimeout, int sensingTimeout) { }
        public async Task SetPurgeAirHoldTimeAsync(int holdTime) { }
        public async Task<int> GetPurgeAirHoldTimeAsync() => 0;

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
            return MotionStatus.Ready;
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
    }
}
