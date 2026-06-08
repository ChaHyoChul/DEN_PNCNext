using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using PncNext.Infrastructure.Motion.Protocols.PA;
using System.Text;
using System.Globalization;

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

        public async Task SetServoAsync(bool on)
        {
            await SendAndReceiveAsync(_protocol.EncodeServo(on));
        }

        public async Task StartJogAsync(int axis, int direction)
        {
            await SendAndReceiveAsync(_protocol.EncodeJog(axis, direction));
        }

        public async Task StopJogAsync()
        {
            await SendAndReceiveAsync(_protocol.EncodeJogStop());
        }

        public async Task SetJogSpeedAsync(int speed)
        {
            await SendAndReceiveAsync(_protocol.EncodeSetJogSpeed(speed));
        }

        public async Task<int> GetJogSpeedAsync()
        {
            var response = await SendAndReceiveAsync(_protocol.EncodeGetJogSpeed());
            if (response == null || response.Length == 0) return 0;
            string resStr = Encoding.ASCII.GetString(response).Trim();
            var parts = resStr.Split(' ', ',');
            if (parts.Length > 1 && int.TryParse(parts[1], out int speed)) return speed;
            return 0;
        }

        public async Task SetOutputAsync(int bitNo, bool on)
        {
            await SendAndReceiveAsync(_protocol.EncodeOutput(bitNo, on));
        }

        public async Task InitSpindleAsync()
        {
            await SendAndReceiveAsync(_protocol.EncodeInitSpindle());
        }

        // [2단계] 파라미터 구현
        public async Task<double[]> GetCoordinateOffsetAsync(int index) => await SendAndParseArrayAsync(_protocol.EncodeReadConfig(index));
        public async Task SetCoordinateOffsetAsync(int index, double[] values) => await SendAndReceiveAsync(_protocol.EncodeWriteConfig(index, values));
        
        public async Task<double[]> GetTeachingPointAsync(int index) => await SendAndParseArrayAsync(_protocol.EncodeReadTeaching(index));
        public async Task SetTeachingPointAsync(int index, double[] values) => await SendAndReceiveAsync(_protocol.EncodeWriteTeaching(index, values));

        public async Task<double> GetZOriginOffsetAsync() => await SendAndParseDoubleAsync(_protocol.EncodeReadZOriginOffset());
        public async Task SetZOriginOffsetAsync(double offset) => await SendAndReceiveAsync(_protocol.EncodeWriteZOriginOffset(offset));

        public async Task<int> GetToolSensingHighSpeedAsync() => await SendAndParseIntAsync(_protocol.EncodeReadToolSensingHighSpeed());
        public async Task SetToolSensingHighSpeedAsync(int speed) => await SendAndReceiveAsync(_protocol.EncodeWriteToolSensingHighSpeed(speed));

        public async Task<int> GetToolSensingLowSpeedAsync() => await SendAndParseIntAsync(_protocol.EncodeReadToolSensingLowSpeed());
        public async Task SetToolSensingLowSpeedAsync(int speed) => await SendAndReceiveAsync(_protocol.EncodeWriteToolSensingLowSpeed(speed));

        public async Task<double> GetToolSensingMarginAsync() => await SendAndParseDoubleAsync(_protocol.EncodeReadToolSensingMargin());
        public async Task SetToolSensingMarginAsync(double margin) => await SendAndReceiveAsync(_protocol.EncodeWriteToolSensingMargin(margin));

        public async Task<double> GetToolPocketPutOffsetAsync() => await SendAndParseDoubleAsync(_protocol.EncodeReadToolPocketPutOffset());
        public async Task SetToolPocketPutOffsetAsync(double offset) => await SendAndReceiveAsync(_protocol.EncodeWriteToolPocketPutOffset(offset));

        public async Task<double[]> GetSoftLimitPositiveAsync() => await SendAndParseArrayAsync(_protocol.EncodeReadSoftLimitPositive());
        public async Task SetSoftLimitPositiveAsync(double[] values) => await SendAndReceiveAsync(_protocol.EncodeWriteSoftLimitPositive(values));

        public async Task<double[]> GetSoftLimitNegativeAsync() => await SendAndParseArrayAsync(_protocol.EncodeReadSoftLimitNegative());
        public async Task SetSoftLimitNegativeAsync(double[] values) => await SendAndReceiveAsync(_protocol.EncodeWriteSoftLimitNegative(values));

        // [3단계] 시스템 설정 구현
        public async Task<string> GetControllerIpAsync() => await SendAndParseStringAsync(_protocol.EncodeReadControllerIp());
        public async Task SetControllerIpAsync(string ip) => await SendAndReceiveAsync(_protocol.EncodeWriteControllerIp(ip));

        public async Task<string> GetIoBoardIpAsync() => await SendAndParseStringAsync(_protocol.EncodeReadIoBoardIp());
        public async Task SetIoBoardIpAsync(string ip) => await SendAndReceiveAsync(_protocol.EncodeWriteIoBoardIp(ip));

        public async Task<string> GetFirmwareVersionAsync() => await SendAndParseStringAsync(_protocol.EncodeReadVersion());
        public async Task SaveToFlashAsync() => await SendAndReceiveAsync(_protocol.EncodeSaveFlash());
        public async Task RestoreToolInfoAsync(int toolNo, double length, bool updated) => await SendAndReceiveAsync(_protocol.EncodeRestoreToolInfo(toolNo, length, updated));

        // [2.17, 2.18] 자동 보정 및 설정 상세 구현
        public async Task StartMeasureAsync(int axisNo, double inPitch, double outPitch, int speed, int count, double maxDist, double offset)
        {
            await SendAndReceiveAsync(_protocol.EncodeMeasure(axisNo, inPitch, outPitch, speed, count, maxDist, offset));
        }

        public async Task<double> GetMeasureResultAsync() => await SendAndParseDoubleAsync(_protocol.EncodeGetMeasureResult());
        public async Task SetupSuhoAsync() => await SendAndReceiveAsync(_protocol.EncodeSetupSuho());
        public async Task SetupSabhoAsync() => await SendAndReceiveAsync(_protocol.EncodeSetupSabho());
        public async Task SetupSorzAsync() => await SendAndReceiveAsync(_protocol.EncodeSetupSorz());
        public async Task SetDiskThicknessAsync(double thickness) => await SendAndReceiveAsync(_protocol.EncodeSetDiskThickness(thickness));
        
        public async Task SetM28TypeAsync(int type) => await SendAndReceiveAsync(_protocol.EncodeSetM28Type(type));
        public async Task<int> GetM28TypeAsync() => await SendAndParseIntAsync(_protocol.EncodeGetM28Type());

        public async Task ResetHomingStatusAsync() => await SendAndReceiveAsync(_protocol.EncodeResetHomingStatus());

        public async Task SetAirParametersAsync(int usingAir, int interval, int usingPurge, int purgeInterval)
        {
            await SendAndReceiveAsync(_protocol.EncodeSetAirParameters(usingAir, interval, usingPurge, purgeInterval));
        }

        public async Task SetWaterFlowParametersAsync(int usingWater, int startTimeout, int sensingTimeout)
        {
            await SendAndReceiveAsync(_protocol.EncodeSetWaterFlowParameters(usingWater, startTimeout, sensingTimeout));
        }

        public async Task SetPurgeAirHoldTimeAsync(int holdTime) => await SendAndReceiveAsync(_protocol.EncodeSetPurgeAirHoldTime(holdTime));
        public async Task<int> GetPurgeAirHoldTimeAsync() => await SendAndParseIntAsync(_protocol.EncodeReadPurgeAirHoldTime());

        // 파싱 헬퍼 메서드들
        private async Task<double[]> SendAndParseArrayAsync(MotionCommandInfo cmd)
        {
            var response = await SendAndReceiveAsync(cmd);
            string resStr = Encoding.ASCII.GetString(response).Trim();
            var parts = resStr.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Skip(parts.Length > 6 ? 2 : 1)
                        .Select(p => double.TryParse(p, CultureInfo.InvariantCulture, out var d) ? d : 0.0)
                        .ToArray();
        }

        private async Task<double> SendAndParseDoubleAsync(MotionCommandInfo cmd)
        {
            var response = await SendAndReceiveAsync(cmd);
            string resStr = Encoding.ASCII.GetString(response).Trim();
            var parts = resStr.Split(' ');
            return parts.Length > 1 && double.TryParse(parts[1], CultureInfo.InvariantCulture, out var d) ? d : 0.0;
        }

        private async Task<int> SendAndParseIntAsync(MotionCommandInfo cmd)
        {
            var response = await SendAndReceiveAsync(cmd);
            string resStr = Encoding.ASCII.GetString(response).Trim();
            var parts = resStr.Split(' ');
            return parts.Length > 1 && int.TryParse(parts[1], out var i) ? i : 0;
        }

        private async Task<string> SendAndParseStringAsync(MotionCommandInfo cmd)
        {
            var response = await SendAndReceiveAsync(cmd);
            string resStr = Encoding.ASCII.GetString(response).Trim();
            var parts = resStr.Split(new[] { ' ' }, 2);
            return parts.Length > 1 ? parts[1] : resStr;
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

        public void UpdateState(byte[] data, PAMotionControllerState state)
        {
            _protocol.UpdateStateFromResponse(data, state);
        }
    }
}
