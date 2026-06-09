using Microsoft.AspNetCore.Mvc;
using PncNext.Domain.Entities;
using PncNext.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PncNext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiskController : ControllerBase
    {
        private readonly IDiskManagementService _diskService;
        private readonly ILogger<DiskController> _logger;

        public DiskController(IDiskManagementService diskService, ILogger<DiskController> logger)
        {
            _diskService = diskService;
            _logger = logger;
        }

        /// <summary>
        /// 전체 디스크 목록을 조회합니다.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiskInventory>>> GetAll()
        {
            var disks = await _diskService.GetAllDisksAsync();
            return Ok(disks);
        }

        /// <summary>
        /// 바코드로 특정 디스크를 조회합니다.
        /// </summary>
        [HttpGet("{barcode}")]
        public async Task<ActionResult<DiskInventory>> GetByBarcode(string barcode)
        {
            var disk = await _diskService.GetDiskByBarcodeAsync(barcode);
            if (disk == null) return NotFound("해당 바코드의 자재 정보를 찾을 수 없습니다.");
            return Ok(disk);
        }

        /// <summary>
        /// 새 디스크 자재를 등록합니다.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DiskInventory>> Register([FromBody] DiskInventory disk)
        {
            try
            {
                var registered = await _diskService.RegisterDiskAsync(disk);
                return CreatedAtAction(nameof(GetByBarcode), new { barcode = registered.DiskBarcode }, registered);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 디스크의 가공 영역 레이아웃을 업데이트합니다.
        /// </summary>
        [HttpPut("{id}/layout")]
        public async Task<IActionResult> UpdateLayout(int id, [FromBody] string newLayoutJson)
        {
            try
            {
                await _diskService.UpdateUsedAreaAsync(id, newLayoutJson);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// 디스크 정보를 삭제합니다.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _diskService.DeleteDiskAsync(id);
            return NoContent();
        }
    }
}
