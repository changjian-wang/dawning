using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// 系统元数据管理控制器
    /// </summary>
    [ApiController]
    [Route("api/system-metadata")]
    [Authorize(Roles = "admin,super_admin")]
    public class SystemMetadataController : ControllerBase
    {
        private readonly ISystemMetadataService _service;

        public SystemMetadataController(ISystemMetadataService service)
        {
            _service = service;
        }

        /// <summary>
        /// 根据ID获取系统元数据
        /// </summary>
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var result = await _service.GetAsync(id);
            if (result == null)
            {
                return Ok(ApiResponse<SystemMetadataDto>.Error(40404, "SystemMetadata not found"));
            }
            return Ok(ApiResponse<SystemMetadataDto>.Success(result));
        }

        /// <summary>
        /// 获取系统元数据分页列表
        /// </summary>
        [HttpPost("get-paged-list")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] SystemMetadataModel model,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return Ok(ApiResponse.Error(40001, "Invalid page or pageSize parameters"));
            }

            var result = await _service.GetPagedListAsync(model, page, pageSize);
            return Ok(ApiResponse<object>.Success(result));
        }

        /// <summary>
        /// 获取所有系统元数据
        /// </summary>
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<SystemMetadataDto>>.Success(result ?? []));
        }

        /// <summary>
        /// 创建系统元数据
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> InsertAsync([FromBody] SystemMetadataDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Key))
            {
                return Ok(ApiResponse<int>.Error(40002, "Invalid metadata or missing key"));
            }

            var result = await _service.InsertAsync(dto);
            return Ok(ApiResponse<int>.Success(result, "SystemMetadata created successfully"));
        }

        /// <summary>
        /// 更新系统元数据
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] SystemMetadataDto dto)
        {
            if (dto?.Id == null)
            {
                return Ok(ApiResponse<bool>.Error(40003, "Invalid metadata or missing ID"));
            }

            var result = await _service.UpdateAsync(dto);
            return Ok(ApiResponse<bool>.Success(result, "SystemMetadata updated successfully"));
        }

        /// <summary>
        /// 删除系统元数据
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Ok(ApiResponse<bool>.Error(40005, "Invalid ID"));
            }

            var result = await _service.DeleteAsync(new SystemMetadataDto { Id = id });
            return Ok(ApiResponse<bool>.Success(result, "SystemMetadata deleted successfully"));
        }
    }
}
