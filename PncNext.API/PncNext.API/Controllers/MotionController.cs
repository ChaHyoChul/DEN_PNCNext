using Microsoft.AspNetCore.Mvc;
using PncNext.Domain.Interfaces;

namespace PncNext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotionController : ControllerBase
    {
        private readonly IMotionControl _motionControl;
        private readonly ILogger<MotionController> _logger;

        public MotionController(IMotionControl motionControl, ILogger<MotionController> logger)
        {
            _motionControl = motionControl;
            _logger = logger;
        }

        /// <summary>
        /// 장비의 현재 상태(Idle, Running, Error 등)를 조회합니다.
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<string>> GetStatus()
        {
            try
            {
                var status = await _motionControl.GetStatusAsync();
                return Ok(status.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "모션 상태 조회 중 오류 발생");
                return StatusCode(500, "상태를 읽어오는 중 내부 오류가 발생했습니다.");
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
        /// 장비를 즉시 정지시킵니다.
        /// </summary>
        [HttpPost("stop")]
        public async Task<IActionResult> Stop()
        {
            try
            {
                await _motionControl.StopAsync();
                return Ok(new { message = "정지 명령이 전송되었습니다." });
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
