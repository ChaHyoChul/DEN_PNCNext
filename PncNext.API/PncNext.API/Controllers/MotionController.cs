using Microsoft.AspNetCore.Mvc;
using PncNext.Domain.Interfaces;
using PncNext.Domain.Contracts;

namespace PncNext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotionController : ControllerBase
    {
        private readonly IMotionControl _motionControl;
        private readonly IMotionStateStore _stateStore;
        private readonly ILogger<MotionController> _logger;

        public MotionController(
            IMotionControl motionControl, 
            IMotionStateStore stateStore,
            ILogger<MotionController> logger)
        {
            _motionControl = motionControl;
            _stateStore = stateStore;
            _logger = logger;
        }

        [HttpGet("status")]
        public ActionResult<MotionStatusResponseDto> GetStatus()
        {
            try
            {
                var paState = _stateStore.PaState;
                var response = new MotionStatusResponseDto
                {
                    OverallStatus = paState.ControllerState.ToString(),
                    Position = paState.Position,
                    IsServoOn = paState.IsServoOn,
                    IsHomComplete = paState.IsHomComplete,
                    IsSpindleRun = paState.IsSpindleRun,
                    CurrentLine = paState.MillingLineNumber,
                    ToolNo = paState.CurrentToolNo,
                    GPLErrorCode = paState.GPLErrorCode,
                    LastErrorCode = paState.LastErrorCode,
                    LastErrorMessage = paState.LastErrorMessage,
                    Timestamp = DateTime.Now
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API 상태 조회 중 오류 발생");
                return StatusCode(500, "상태 정보를 가져오는 중 내부 오류가 발생했습니다.");
            }
        }

        [HttpPost("move-incremental")]
        public async Task<IActionResult> MoveIncremental([FromBody] OptionalMoveRequest request)
        {
            if (request == null) return BadRequest("요청 데이터가 비어있습니다.");
            return await SafeExecute(() => _motionControl.MoveIncrementalAsync(request.X, request.Y, request.Z, request.A, request.B));
        }

        [HttpPost("move-absolute")]
        public async Task<IActionResult> MoveAbsolute([FromBody] OptionalMoveRequest request)
        {
            if (request == null) return BadRequest("요청 데이터가 비어있습니다.");
            return await SafeExecute(() => _motionControl.MoveAbsoluteAsync(request.X, request.Y, request.Z, request.A, request.B));
        }

        [HttpPost("home")]
        public async Task<IActionResult> Home() => await SafeExecute(() => _motionControl.HomeAsync());

        [HttpPost("mode")]
        public async Task<IActionResult> SetMode([FromQuery] string mode) => await SafeExecute(() => _motionControl.SetModeAsync(mode));

        [HttpPost("pause")]
        public async Task<IActionResult> Pause() => await SafeExecute(() => _motionControl.PauseAsync());

        [HttpPost("continue")]
        public async Task<IActionResult> Continue() => await SafeExecute(() => _motionControl.ContinueAsync());

        [HttpPost("error-reset")]
        public async Task<IActionResult> ErrorReset() => await SafeExecute(() => _motionControl.ErrorResetAsync());

        [HttpPost("stop")]
        public async Task<IActionResult> Stop([FromQuery] int mode = 0) => await SafeExecute(() => _motionControl.StopAsync(mode));

        [HttpPost("mda")]
        public async Task<IActionResult> Mda([FromQuery] string gcode)
        {
            if (string.IsNullOrWhiteSpace(gcode)) return BadRequest("G-Code가 비어있습니다.");
            return await SafeExecute(() => _motionControl.MdaAsync(gcode));
        }

        [HttpPost("servo")]
        public async Task<IActionResult> SetServo([FromQuery] bool on) => await SafeExecute(() => _motionControl.SetServoAsync(on));

        [HttpPost("jog/start")]
        public async Task<IActionResult> StartJog([FromQuery] int axis, [FromQuery] int direction) => await SafeExecute(() => _motionControl.StartJogAsync(axis, direction));

        [HttpPost("jog/stop")]
        public async Task<IActionResult> StopJog() => await SafeExecute(() => _motionControl.StopJogAsync());

        [HttpPost("jog/speed")]
        public async Task<IActionResult> SetJogSpeed([FromQuery] int speed) => await SafeExecute(() => _motionControl.SetJogSpeedAsync(speed));

        [HttpPost("output")]
        public async Task<IActionResult> SetOutput([FromQuery] int bitNo, [FromQuery] bool on) => await SafeExecute(() => _motionControl.SetOutputAsync(bitNo, on));

        [HttpPost("spindle/init")]
        public async Task<IActionResult> InitSpindle() => await SafeExecute(() => _motionControl.InitSpindleAsync());

        // [2.17, 2.18] 신규 API
        [HttpPost("measure/start")]
        public async Task<IActionResult> StartMeasure(int axisNo, double inPitch, double outPitch, int speed, int count, double maxDist, double offset)
            => await SafeExecute(() => _motionControl.StartMeasureAsync(axisNo, inPitch, outPitch, speed, count, maxDist, offset));

        [HttpGet("measure/result")]
        public async Task<IActionResult> GetMeasureResult() => await SafeExecute(() => _motionControl.GetMeasureResultAsync());

        [HttpPost("system/save-flash")]
        public async Task<IActionResult> SaveFlash() => await SafeExecute(() => _motionControl.SaveToFlashAsync());

        [HttpPost("setup/disk-thickness")]
        public async Task<IActionResult> SetDiskThickness([FromQuery] double thickness) => await SafeExecute(() => _motionControl.SetDiskThicknessAsync(thickness));

        [HttpPost("system/m28-type")]
        public async Task<IActionResult> SetM28Type([FromQuery] int type) => await SafeExecute(() => _motionControl.SetM28TypeAsync(type));

        [HttpGet("system/m28-type")]
        public async Task<IActionResult> GetM28Type() => await SafeExecute(() => _motionControl.GetM28TypeAsync());

        [HttpPost("system/homing-reset")]
        public async Task<IActionResult> ResetHomingStatus() => await SafeExecute(() => _motionControl.ResetHomingStatusAsync());

        [HttpPost("setup/suho")]
        public async Task<IActionResult> SetupSuho() => await SafeExecute(() => _motionControl.SetupSuhoAsync());

        [HttpPost("setup/sabho")]
        public async Task<IActionResult> SetupSabho() => await SafeExecute(() => _motionControl.SetupSabhoAsync());

        [HttpPost("setup/sorz")]
        public async Task<IActionResult> SetupSorz() => await SafeExecute(() => _motionControl.SetupSorzAsync());

        [HttpPost("system/air-params")]
        public async Task<IActionResult> SetAirParameters(int usingAir, int interval, int usingPurge, int purgeInterval)
            => await SafeExecute(() => _motionControl.SetAirParametersAsync(usingAir, interval, usingPurge, purgeInterval));

        [HttpPost("system/water-params")]
        public async Task<IActionResult> SetWaterFlowParameters(int usingWater, int startTimeout, int sensingTimeout)
            => await SafeExecute(() => _motionControl.SetWaterFlowParametersAsync(usingWater, startTimeout, sensingTimeout));

        [HttpGet("system/purge-hold-time")]
        public async Task<IActionResult> GetPurgeAirHoldTime() => await SafeExecute(() => _motionControl.GetPurgeAirHoldTimeAsync());

        [HttpPost("system/purge-hold-time")]
        public async Task<IActionResult> SetPurgeAirHoldTime([FromQuery] int holdTime) => await SafeExecute(() => _motionControl.SetPurgeAirHoldTimeAsync(holdTime));

        [HttpPost("system/tool-info/restore")]
        public async Task<IActionResult> RestoreToolInfo(int toolNo, double length, bool updated)
            => await SafeExecute(() => _motionControl.RestoreToolInfoAsync(toolNo, length, updated));

        private async Task<IActionResult> SafeExecute(Func<Task> action)
        {
            try { await action(); return Ok(new { message = "명령이 성공적으로 전송되었습니다." }); }
            catch (Exception ex) { _logger.LogError(ex, "명령 실행 중 오류"); return StatusCode(500, new { error = ex.Message }); }
        }

        private async Task<IActionResult> SafeExecute<T>(Func<Task<T>> action)
        {
            try { var result = await action(); return Ok(result); }
            catch (Exception ex) { _logger.LogError(ex, "조회 실행 중 오류"); return StatusCode(500, new { error = ex.Message }); }
        }
    }

    public class OptionalMoveRequest
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }
        public double? A { get; set; }
        public double? B { get; set; }
    }
}
