using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Models.OpenIddict;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.OpenIddict
{
    /// <summary>
    /// API资源管理Controller
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/openiddict/api-resource")]
    [Route("api/v{version:apiVersion}/openiddict/api-resource")]
    [Authorize(Roles = "admin,super_admin")]
    public class ApiResourceController : ControllerBase
    {
        private readonly IApiResourceService _service;

        public ApiResourceController(IApiResourceService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取操作者ID
        /// </summary>
        private Guid GetOperatorId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("user_id");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// 根据ID获取API资源
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            try
            {
                var result = await _service.GetAsync(id);
                if (result == null || result.Id == null)
                {
                    return Ok(ApiResponse<ApiResourceDto>.Error(40404, "API Resource not found"));
                }
                return Ok(ApiResponse<ApiResourceDto>.Success(result));
            }
            catch (Exception ex)
            {
                return Ok(
                    ApiResponse<ApiResourceDto>.Error(50000, $"Internal server error: {ex.Message}")
                );
            }
        }

        /// <summary>
        /// 根据名称获取API资源
        /// </summary>
        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Ok(ApiResponse<ApiResourceDto>.Error(40001, "Invalid resource name"));
                }

                var result = await _service.GetByNameAsync(name);
                if (result == null)
                {
                    return Ok(ApiResponse<ApiResourceDto>.Error(40404, "API Resource not found"));
                }
                return Ok(ApiResponse<ApiResourceDto>.Success(result));
            }
            catch (Exception ex)
            {
                return Ok(
                    ApiResponse<ApiResourceDto>.Error(50000, $"Internal server error: {ex.Message}")
                );
            }
        }

        /// <summary>
        /// 根据名称列表获取API资源
        /// </summary>
        [HttpPost("by-names")]
        public async Task<IActionResult> GetByNamesAsync([FromBody] IEnumerable<string> names)
        {
            try
            {
                if (names == null || !names.Any())
                {
                    return Ok(
                        ApiResponse<IEnumerable<ApiResourceDto>>.Error(
                            40001,
                            "Invalid resource names"
                        )
                    );
                }

                var result = await _service.GetByNamesAsync(names);
                return Ok(
                    ApiResponse<IEnumerable<ApiResourceDto>>.Success(
                        result ?? Array.Empty<ApiResourceDto>()
                    )
                );
            }
            catch (Exception ex)
            {
                return Ok(
                    ApiResponse<IEnumerable<ApiResourceDto>>.Error(
                        50000,
                        $"Internal server error: {ex.Message}"
                    )
                );
            }
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] ApiResourceModel model,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return Ok(ApiResponse.Error(40001, "Invalid page or pageSize parameters"));
                }

                var result = await _service.GetPagedListAsync(model, page, pageSize);
                return Ok(ApiResponse<object>.Success(result));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(50000, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取所有API资源
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(
                    ApiResponse<IEnumerable<ApiResourceDto>>.Success(
                        result ?? Array.Empty<ApiResourceDto>()
                    )
                );
            }
            catch (Exception ex)
            {
                return Ok(
                    ApiResponse<IEnumerable<ApiResourceDto>>.Error(
                        50000,
                        $"Internal server error: {ex.Message}"
                    )
                );
            }
        }

        /// <summary>
        /// 创建API资源
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InsertAsync([FromBody] ApiResourceDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                {
                    return Ok(
                        ApiResponse<int>.Error(40002, "Invalid resource data or missing name")
                    );
                }

                // 设置操作者ID
                dto.OperatorId = GetOperatorId();

                var result = await _service.InsertAsync(dto);
                return Ok(ApiResponse<int>.Success(result, "API Resource created successfully"));
            }
            catch (ArgumentException ex)
            {
                return Ok(ApiResponse<int>.Error(40003, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Ok(ApiResponse<int>.Error(40009, ex.Message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<int>.Error(50000, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新API资源
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] ApiResourceDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return Ok(ApiResponse<bool>.Error(40003, "Invalid resource data"));
                }

                // 确保ID匹配
                dto.Id = id;

                // 设置操作者ID
                dto.OperatorId = GetOperatorId();

                var result = await _service.UpdateAsync(dto);
                return Ok(ApiResponse<bool>.Success(result, "API Resource updated successfully"));
            }
            catch (ArgumentException ex)
            {
                return Ok(ApiResponse<bool>.Error(40003, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Ok(ApiResponse<bool>.Error(40009, ex.Message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<bool>.Error(50000, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// 删除API资源
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Ok(ApiResponse<bool>.Error(40005, "Invalid ID"));
                }

                var dto = new ApiResourceDto { Id = id, OperatorId = GetOperatorId() };

                var result = await _service.DeleteAsync(dto);
                return Ok(ApiResponse<bool>.Success(result, "API Resource deleted successfully"));
            }
            catch (ArgumentException ex)
            {
                return Ok(ApiResponse<bool>.Error(40003, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Ok(ApiResponse<bool>.Error(40009, ex.Message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<bool>.Error(50000, $"Internal server error: {ex.Message}"));
            }
        }
    }
}
