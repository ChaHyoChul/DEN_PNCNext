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

        /// <summary>
        /// 백그라운드 서비스에서 주기적으로 갱신되는 장비의 최신 상태를 조회합니다.
        /// </summary>
        [HttpGet("status")]
        public ActionResult<MotionStatusResponseDto> GetStatus()
        {
            try
            {
                // 하드웨어에 직접 묻지 않고, 메모리 저장소(StateStore)에 있는 최신 데이터를 반환합니다.
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

        /// <summary>
        /// 입력된 축만 선택적으로 상대 위치 이동을 수행합니다.
        /// </summary>
        [HttpPost("move-incremental")]
        public async Task<IActionResult> MoveIncremental([FromBody] OptionalMoveRequest request)
        {
            if (request == null) return BadRequest("요청 데이터가 비어있습니다.");
            try
            {
                await _motionControl.MoveIncrementalAsync(request.X, request.Y, request.Z, request.A, request.B);
                return Ok(new { message = "상대 이동 명령이 전송되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "상대 이동 명령 전송 중 오류 발생");
                return StatusCode(500, "상대 이동 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 입력된 축만 선택적으로 절대 위치 이동을 수행합니다.
        /// </summary>
        [HttpPost("move-absolute")]
        public async Task<IActionResult> MoveAbsolute([FromBody] OptionalMoveRequest request)
        {
            if (request == null) return BadRequest("요청 데이터가 비어있습니다.");
            try
            {
                await _motionControl.MoveAbsoluteAsync(request.X, request.Y, request.Z, request.A, request.B);
                return Ok(new { message = "절대 이동 명령이 전송되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "절대 이동 명령 전송 중 오류 발생");
                return StatusCode(500, "절대 이동 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// G-Code 명령(MDA)을 실행합니다.
        /// </summary>
        /// <param name="gcode">실행할 G-Code 문자열</param>
        [HttpPost("mda")]
        public async Task<IActionResult> Mda([FromQuery] string gcode)
        {
            if (string.IsNullOrWhiteSpace(gcode)) return BadRequest("G-Code가 비어있습니다.");
            try
            {
                await _motionControl.MdaAsync(gcode);
                return Ok(new { message = $"MDA 명령({gcode})이 전송되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MDA 명령 전송 중 오류 발생");
                return StatusCode(500, "MDA 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 장비의 원점 복귀(Homing)를 수행합니다.
        /// </summary>
        [HttpPost("home")]
        public async Task<IActionResult> Home()
        {
            try
            {
                await _motionControl.HomeAsync();
                return Ok(new { message = "원점 복귀 명령이 전송되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "원점 복귀 명령 전송 중 오류 발생");
                return StatusCode(500, "원점 복귀 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 장비의 동작 모드를 설정합니다.
        /// </summary>
        /// <param name="mode">"OFF", "AUTO", "STEP", "MDA"</param>
        [HttpPost("mode")]
        public async Task<IActionResult> SetMode([FromQuery] string mode)
        {
            try
            {
                await _motionControl.SetModeAsync(mode);
                return Ok(new { message = $"동작 모드가 {mode}로 설정되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "모드 전환 명령 전송 중 오류 발생");
                return StatusCode(500, "모드 전환 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 장비를 일시 정지시킵니다.
        /// </summary>
        [HttpPost("pause")]
        public async Task<IActionResult> Pause()
        {
            try
            {
                await _motionControl.PauseAsync();
                return Ok(new { message = "일시 정지 명령이 전송되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "일시 정지 명령 전송 중 오류 발생");
                return StatusCode(500, "일시 정지 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 일시 정지된 장비를 재개시킵니다.
        /// </summary>
        [HttpPost("continue")]
        public async Task<IActionResult> Continue()
        {
            try
            {
                await _motionControl.ContinueAsync();
                return Ok(new { message = "재개 명령이 전송되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "재개 명령 전송 중 오류 발생");
                return StatusCode(500, "재개 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 장비의 에러 상태를 해제(Reset)합니다.
        /// </summary>
        [HttpPost("error-reset")]
        public async Task<IActionResult> ErrorReset()
        {
            try
            {
                await _motionControl.ErrorResetAsync();
                return Ok(new { message = "에러 리셋 명령이 전송되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "에러 리셋 명령 전송 중 오류 발생");
                return StatusCode(500, "에러 리셋 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 장비를 즉시 정지시킵니다.
        /// </summary>
        /// <param name="mode">정지 모드 (기본값: 0)</param>
        [HttpPost("stop")]
        public async Task<IActionResult> Stop([FromQuery] int mode = 0)
        {
            try
            {
                await _motionControl.StopAsync(mode);
                return Ok(new { message = $"정지 명령(Mode: {mode})이 전송되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "정지 명령 전송 중 오류 발생");
                return StatusCode(500, "정지 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 서보 전원을 제어합니다.
        /// </summary>
        [HttpPost("servo")]
        public async Task<IActionResult> SetServo([FromQuery] bool on)
        {
            try
            {
                await _motionControl.SetServoAsync(on);
                return Ok(new { message = $"서보가 {(on ? "ON" : "OFF")} 되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "서보 제어 명령 전송 중 오류 발생");
                return StatusCode(500, "서보 제어 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 조그 이동을 시작합니다.
        /// </summary>
        [HttpPost("jog/start")]
        public async Task<IActionResult> StartJog([FromQuery] int axis, [FromQuery] int direction)
        {
            try
            {
                await _motionControl.StartJogAsync(axis, direction);
                return Ok(new { message = "조그 이동이 시작되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "조그 시작 명령 전송 중 오류 발생");
                return StatusCode(500, "조그 시작 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 조그 이동을 정지합니다.
        /// </summary>
        [HttpPost("jog/stop")]
        public async Task<IActionResult> StopJog()
        {
            try
            {
                await _motionControl.StopJogAsync();
                return Ok(new { message = "조그 이동이 정지되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "조그 정지 명령 전송 중 오류 발생");
                return StatusCode(500, "조그 정지 명령 전송에 실패했습니다.");
            }
        }

        /// <summary>
        /// 조그 이동 속도를 설정합니다. (0~100)
        /// </summary>
        [HttpPost("jog/speed")]
        public async Task<IActionResult> SetJogSpeed([FromQuery] int speed)
        {
            try
            {
                await _motionControl.SetJogSpeedAsync(speed);
                return Ok(new { message = $"조그 속도가 {speed}%로 설정되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "조그 속도 설정 중 오류 발생");
                return StatusCode(500, "조그 속도 설정에 실패했습니다.");
            }
        }

        /// <summary>
        /// 디지털 출력을 제어합니다.
        /// </summary>
        [HttpPost("output")]
        public async Task<IActionResult> SetOutput([FromQuery] int bitNo, [FromQuery] bool on)
        {
            try
            {
                await _motionControl.SetOutputAsync(bitNo, on);
                return Ok(new { message = $"Output {bitNo}번이 {(on ? "ON" : "OFF")} 되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "출력 제어 중 오류 발생");
                return StatusCode(500, "출력 제어에 실패했습니다.");
            }
        }

        /// <summary>
        /// 스핀들 시스템을 초기화합니다.
        /// </summary>
        [HttpPost("spindle/init")]
        public async Task<IActionResult> InitSpindle()
        {
            try
            {
                await _motionControl.InitSpindleAsync();
                return Ok(new { message = "스핀들 초기화 명령이 전송되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "스핀들 초기화 중 오류 발생");
                return StatusCode(500, "스핀들 초기화에 실패했습니다.");
            }
        }

        // [2단계] 파라미터 관련 API
        [HttpGet("config/{index}")]
        public async Task<IActionResult> GetConfig(int index) => await SafeExecute(() => _motionControl.GetCoordinateOffsetAsync(index));

        [HttpPost("config/{index}")]
        public async Task<IActionResult> SetConfig(int index, [FromBody] double[] values) => await SafeExecute(() => _motionControl.SetCoordinateOffsetAsync(index, values));

        [HttpGet("teaching/{index}")]
        public async Task<IActionResult> GetTeaching(int index) => await SafeExecute(() => _motionControl.GetTeachingPointAsync(index));

        [HttpPost("teaching/{index}")]
        public async Task<IActionResult> SetTeaching(int index, [FromBody] double[] values) => await SafeExecute(() => _motionControl.SetTeachingPointAsync(index, values));

        [HttpGet("offset/z-origin")]
        public async Task<IActionResult> GetZOriginOffset() => await SafeExecute(() => _motionControl.GetZOriginOffsetAsync());

        [HttpPost("offset/z-origin")]
        public async Task<IActionResult> SetZOriginOffset([FromQuery] double offset) => await SafeExecute(() => _motionControl.SetZOriginOffsetAsync(offset));

        [HttpGet("sensing/high-speed")]
        public async Task<IActionResult> GetSensingHighSpeed() => await SafeExecute(() => _motionControl.GetToolSensingHighSpeedAsync());

        [HttpPost("sensing/high-speed")]
        public async Task<IActionResult> SetSensingHighSpeed([FromQuery] int speed) => await SafeExecute(() => _motionControl.SetToolSensingHighSpeedAsync(speed));

        [HttpGet("limit/positive")]
        public async Task<IActionResult> GetLimitPositive() => await SafeExecute(() => _motionControl.GetSoftLimitPositiveAsync());

        [HttpPost("limit/positive")]
        public async Task<IActionResult> SetLimitPositive([FromBody] double[] values) => await SafeExecute(() => _motionControl.SetSoftLimitPositiveAsync(values));

        // [3단계] 시스템 관련 API
        [HttpGet("system/ip/controller")]
        public async Task<IActionResult> GetControllerIp() => await SafeExecute(() => _motionControl.GetControllerIpAsync());

        [HttpPost("system/ip/controller")]
        public async Task<IActionResult> SetControllerIp([FromQuery] string ip) => await SafeExecute(() => _motionControl.SetControllerIpAsync(ip));

        [HttpGet("system/version")]
        public async Task<IActionResult> GetVersion() => await SafeExecute(() => _motionControl.GetFirmwareVersionAsync());

        [HttpPost("system/save-flash")]
        public async Task<IActionResult> SaveFlash() => await SafeExecute(() => _motionControl.SaveToFlashAsync());

        private async Task<IActionResult> SafeExecute(Func<Task> action)
        {
            try { await action(); return Ok(); }
            catch (Exception ex) { _logger.LogError(ex, "명령 실행 중 오류"); return StatusCode(500, ex.Message); }
        }

        private async Task<IActionResult> SafeExecute<T>(Func<Task<T>> action)
        {
            try { var result = await action(); return Ok(result); }
            catch (Exception ex) { _logger.LogError(ex, "조회 실행 중 오류"); return StatusCode(500, ex.Message); }
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
