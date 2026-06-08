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

        public async Task SetServoAsync(bool on)
        {
            await GetChannel("CMD").SetServoAsync(on);
        }

        public async Task StartJogAsync(int axis, int direction)
        {
            await GetChannel("CMD").StartJogAsync(axis, direction);
        }

        public async Task StopJogAsync()
        {
            await GetChannel("CMD").StopJogAsync();
        }

        public async Task SetJogSpeedAsync(int speed)
        {
            await GetChannel("CMD").SetJogSpeedAsync(speed);
        }

        public async Task<int> GetJogSpeedAsync()
        {
            return await GetChannel("CMD").GetJogSpeedAsync();
        }

        public async Task SetOutputAsync(int bitNo, bool on)
        {
            await GetChannel("CMD").SetOutputAsync(bitNo, on);
        }

        public async Task InitSpindleAsync()
        {
            await GetChannel("CMD").InitSpindleAsync();
        }

        // [2단계] 파라미터 구현
        public async Task<double[]> GetCoordinateOffsetAsync(int index) => await GetChannel("CMD").GetCoordinateOffsetAsync(index);
        public async Task SetCoordinateOffsetAsync(int index, double[] values) => await GetChannel("CMD").SetCoordinateOffsetAsync(index, values);
        
        public async Task<double[]> GetTeachingPointAsync(int index) => await GetChannel("CMD").GetTeachingPointAsync(index);
        public async Task SetTeachingPointAsync(int index, double[] values) => await GetChannel("CMD").SetTeachingPointAsync(index, values);

        public async Task<double> GetZOriginOffsetAsync() => await GetChannel("CMD").GetZOriginOffsetAsync();
        public async Task SetZOriginOffsetAsync(double offset) => await GetChannel("CMD").SetZOriginOffsetAsync(offset);

        public async Task<int> GetToolSensingHighSpeedAsync() => await GetChannel("CMD").GetToolSensingHighSpeedAsync();
        public async Task SetToolSensingHighSpeedAsync(int speed) => await GetChannel("CMD").SetToolSensingHighSpeedAsync(speed);

        public async Task<int> GetToolSensingLowSpeedAsync() => await GetChannel("CMD").GetToolSensingLowSpeedAsync();
        public async Task SetToolSensingLowSpeedAsync(int speed) => await GetChannel("CMD").SetToolSensingLowSpeedAsync(speed);

        public async Task<double> GetToolSensingMarginAsync() => await GetChannel("CMD").GetToolSensingMarginAsync();
        public async Task SetToolSensingMarginAsync(double margin) => await GetChannel("CMD").SetToolSensingMarginAsync(margin);

        public async Task<double> GetToolPocketPutOffsetAsync() => await GetChannel("CMD").GetToolPocketPutOffsetAsync();
        public async Task SetToolPocketPutOffsetAsync(double offset) => await GetChannel("CMD").SetToolPocketPutOffsetAsync(offset);

        public async Task<double[]> GetSoftLimitPositiveAsync() => await GetChannel("CMD").GetSoftLimitPositiveAsync();
        public async Task SetSoftLimitPositiveAsync(double[] values) => await GetChannel("CMD").SetSoftLimitPositiveAsync(values);

        public async Task<double[]> GetSoftLimitNegativeAsync() => await GetChannel("CMD").GetSoftLimitNegativeAsync();
        public async Task SetSoftLimitNegativeAsync(double[] values) => await GetChannel("CMD").SetSoftLimitNegativeAsync(values);

        // [3단계] 시스템 설정 구현
        public async Task<string> GetControllerIpAsync() => await GetChannel("CMD").GetControllerIpAsync();
        public async Task SetControllerIpAsync(string ip) => await GetChannel("CMD").SetControllerIpAsync(ip);

        public async Task<string> GetIoBoardIpAsync() => await GetChannel("CMD").GetIoBoardIpAsync();
        public async Task SetIoBoardIpAsync(string ip) => await GetChannel("CMD").SetIoBoardIpAsync(ip);

        public async Task<string> GetFirmwareVersionAsync() => await GetChannel("CMD").GetFirmwareVersionAsync();
        public async Task SaveToFlashAsync() => await GetChannel("CMD").SaveToFlashAsync();
        public async Task RestoreToolInfoAsync(int toolNo, double length, bool updated) => await GetChannel("CMD").RestoreToolInfoAsync(toolNo, length, updated);

        // [2.17, 2.18] 자동 보정 및 설정 상세 구현
        public async Task StartMeasureAsync(int axisNo, double inPitch, double outPitch, int speed, int count, double maxDist, double offset)
        {
            await GetChannel("CMD").StartMeasureAsync(axisNo, inPitch, outPitch, speed, count, maxDist, offset);
        }

        public async Task<double> GetMeasureResultAsync() => await GetChannel("CMD").GetMeasureResultAsync();
        public async Task SetupSuhoAsync() => await GetChannel("CMD").SetupSuhoAsync();
        public async Task SetupSabhoAsync() => await GetChannel("CMD").SetupSabhoAsync();
        public async Task SetupSorzAsync() => await GetChannel("CMD").SetupSorzAsync();
        public async Task SetDiskThicknessAsync(double thickness) => await GetChannel("CMD").SetDiskThicknessAsync(thickness);
        
        public async Task SetM28TypeAsync(int type) => await GetChannel("CMD").SetM28TypeAsync(type);
        public async Task<int> GetM28TypeAsync() => await GetChannel("CMD").GetM28TypeAsync();

        public async Task ResetHomingStatusAsync() => await GetChannel("CMD").ResetHomingStatusAsync();

        public async Task SetAirParametersAsync(int usingAir, int interval, int usingPurge, int purgeInterval)
        {
            await GetChannel("CMD").SetAirParametersAsync(usingAir, interval, usingPurge, purgeInterval);
        }

        public async Task SetWaterFlowParametersAsync(int usingWater, int startTimeout, int sensingTimeout)
        {
            await GetChannel("CMD").SetWaterFlowParametersAsync(usingWater, startTimeout, sensingTimeout);
        }

        public async Task SetPurgeAirHoldTimeAsync(int holdTime) => await GetChannel("CMD").SetPurgeAirHoldTimeAsync(holdTime);
        public async Task<int> GetPurgeAirHoldTimeAsync() => await GetChannel("CMD").GetPurgeAirHoldTimeAsync();

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
            if (channel.IsFaulted) { return MotionStatus.NotConnected; }
            try { await channel.ReadFullStatusAsync(); return MotionStatus.NotConnected; }
            catch (Exception) { return MotionStatus.NotConnected; }
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
