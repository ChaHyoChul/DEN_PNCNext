using Microsoft.AspNetCore.Mvc;
using PncNext.Domain.Entities;
using PncNext.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PncNext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobManagementService _jobService;
        private readonly ILogger<JobController> _logger;

        public JobController(IJobManagementService jobService, ILogger<JobController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        /// <summary>
        /// 새로운 가공 작업을 시작합니다.
        /// </summary>
        [HttpPost("start")]
        public async Task<ActionResult<JobHistory>> StartJob([FromBody] StartJobRequest request)
        {
            try
            {
                var job = await _jobService.StartJobAsync(request.NcFileId, request.DiskSeq);
                return Ok(job);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// 가공 작업을 성공적으로 완료 처리합니다.
        /// </summary>
        [HttpPost("{jobId}/complete")]
        public async Task<IActionResult> CompleteJob(int jobId, [FromBody] CompleteJobRequest request)
        {
            try
            {
                await _jobService.CompleteJobAsync(jobId, request.UsedAreaJson);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// 가공 작업 중 에러 발생 시 실패 처리합니다.
        /// </summary>
        [HttpPost("{jobId}/fail")]
        public async Task<IActionResult> FailJob(int jobId, [FromBody] FailJobRequest request)
        {
            try
            {
                await _jobService.FailJobAsync(jobId, request.ErrorCode, request.LineNumber);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// 가공 작업을 취소 처리합니다.
        /// </summary>
        [HttpPost("{jobId}/cancel")]
        public async Task<IActionResult> CancelJob(int jobId)
        {
            try
            {
                await _jobService.CancelJobAsync(jobId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// 최근 가공 이력을 조회합니다.
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<JobHistory>>> GetRecentJobs([FromQuery] int count = 50)
        {
            var jobs = await _jobService.GetRecentJobsAsync(count);
            return Ok(jobs);
        }
    }

    // DTOs
    public class StartJobRequest
    {
        public int NcFileId { get; set; }
        public int DiskSeq { get; set; }
    }

    public class CompleteJobRequest
    {
        public string UsedAreaJson { get; set; } = "{}";
    }

    public class FailJobRequest
    {
        public string ErrorCode { get; set; } = "Unknown";
        public int LineNumber { get; set; }
    }
}
