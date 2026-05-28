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
