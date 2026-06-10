using Microsoft.AspNetCore.Mvc;
using PncNext.Domain.Entities;
using PncNext.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PncNext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NcFileController : ControllerBase
    {
        private readonly INcFileService _ncFileService;
        private readonly ILogger<NcFileController> _logger;

        public NcFileController(INcFileService ncFileService, ILogger<NcFileController> logger)
        {
            _ncFileService = ncFileService;
            _logger = logger;
        }

        /// <summary>
        /// 가공 가능한 NC 파일 목록을 조회합니다.
        /// </summary>
        /// <param name="includeArchived">아카이브된 파일 포함 여부</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NcFileInventory>>> GetFiles([FromQuery] bool includeArchived = false)
        {
            var files = await _ncFileService.GetAvailableFilesAsync(includeArchived);
            return Ok(files);
        }

        /// <summary>
        /// 특정 NC 파일의 상세 정보를 조회합니다.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<NcFileInventory>> GetById(int id)
        {
            var file = await _ncFileService.GetFileByIdAsync(id);
            if (file == null) return NotFound("파일을 찾을 수 없습니다.");
            return Ok(file);
        }

        /// <summary>
        /// 외부 경로의 NC 파일을 시스템에 추가합니다.
        /// </summary>
        /// <param name="fullPath">추가할 파일의 전체 경로</param>
        [HttpPost("add")]
        public async Task<ActionResult<NcFileInventory>> AddFile([FromQuery] string fullPath)
        {
            try
            {
                var newFile = await _ncFileService.AddNcFileAsync(fullPath);
                return CreatedAtAction(nameof(GetById), new { id = newFile.Id }, newFile);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NC 파일 추가 중 오류 발생");
                return StatusCode(500, "파일을 추가하는 중 서버 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 파일을 목록에서 삭제합니다. (Soft Delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _ncFileService.DeleteFileAsync(id);
            return NoContent();
        }

        /// <summary>
        /// 파일의 무결성 검증을 수동으로 요청합니다. (스텁)
        /// </summary>
        [HttpPost("{id}/validate")]
        public async Task<IActionResult> Validate(int id)
        {
            // 실제 G-Code 파싱 엔진 연동 전까지는 성공으로 마킹하는 시뮬레이션 수행
            await _ncFileService.UpdateValidationStatusAsync(id, true);
            return Ok(new { message = "파일 검증이 완료되었습니다." });
        }

        /// <summary>
        /// NC 파일에 디스크 정보를 수동으로 매핑합니다.
        /// </summary>
        /// <param name="id">NC 파일 ID</param>
        /// <param name="diskId">매핑할 디스크 ID (DiskInventory.Id)</param>
        [HttpPost("{id}/map-disk/{diskId}")]
        public async Task<IActionResult> MapDisk(int id, int diskId)
        {
            try
            {
                var result = await _ncFileService.UpdateNcFileDiskMappingAsync(id, diskId);
                if (!result) return NotFound("대상 NC 파일을 찾을 수 없습니다.");
                
                return Ok(new { message = "디스크 매핑 및 파일명 동기화가 완료되었습니다." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"디스크 매핑 중 오류 발생 (FileId: {id}, DiskId: {diskId})");
                return StatusCode(500, "매핑 처리 중 서버 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 가공 에러 상태인 파일을 초기화하여 재시작 가능(Ready) 상태로 복구합니다.
        /// </summary>
        /// <param name="id">NC 파일 ID</param>
        [HttpPost("{id}/error-reset")]
        public async Task<IActionResult> ErrorReset(int id)
        {
            try
            {
                var result = await _ncFileService.ResetErrorAsync(id);
                if (!result) return NotFound("대상 NC 파일을 찾을 수 없습니다.");
                
                return Ok(new { message = "에러 상태가 해제되어 가공 준비 상태로 복구되었습니다." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"에러 초기화 중 오류 발생 (FileId: {id})");
                return StatusCode(500, "초기화 처리 중 서버 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 파일을 처음부터 다시 가공할 수 있도록 Ready 상태로 완전히 초기화합니다.
        /// </summary>
        /// <param name="id">NC 파일 ID</param>
        [HttpPost("{id}/reset")]
        public async Task<IActionResult> Reset(int id)
        {
            try
            {
                var result = await _ncFileService.ResetToReadyAsync(id);
                if (!result) return NotFound("대상 NC 파일을 찾을 수 없습니다.");
                
                return Ok(new { message = "파일이 가공 준비(Ready) 상태로 완전히 초기화되었습니다." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"파일 초기화 중 오류 발생 (FileId: {id})");
                return StatusCode(500, "초기화 처리 중 서버 오류가 발생했습니다.");
            }
        }
    }
}
