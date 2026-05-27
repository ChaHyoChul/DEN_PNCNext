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
        /// 장비에 이동 명령을 내립니다.
        /// </summary>
        [HttpPost("move")]
        public async Task<IActionResult> Move([FromBody] MoveRequest request)
        {
            try
            {
                await _motionControl.MoveAsync(request.X, request.Y, request.Z, request.A, request.B);
                return Ok(new { message = "이동 명령이 전송되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "이동 명령 전송 중 오류 발생");
                return StatusCode(500, "이동 명령 전송에 실패했습니다.");
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

    public class MoveRequest
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double A { get; set; }
        public double B { get; set; }
    }
}
