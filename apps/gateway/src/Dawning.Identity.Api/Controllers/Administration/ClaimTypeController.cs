using Dawning.Identity.Api.Helpers;
using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// ClaimType Management Controller
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/claim-type")]
    [Route("api/v{version:apiVersion}/claim-type")]
    [Authorize(Roles = "admin,super_admin")]
    public class ClaimTypeController : ControllerBase
    {
        private readonly IClaimTypeService _service;
        private readonly AuditLogHelper _auditLogHelper;

        public ClaimTypeController(IClaimTypeService service, AuditLogHelper auditLogHelper)
        {
            _service = service;
            _auditLogHelper = auditLogHelper;
        }

        /// <summary>
        /// Get claim type by ID
        /// </summary>
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var result = await _service.GetAsync(id);
            if (result == null)
            {
                return Ok(ApiResponse<ClaimTypeDto>.Error(40404, "ClaimType not found"));
            }
            return Ok(ApiResponse<ClaimTypeDto>.Success(result));
        }

        /// <summary>
        /// Get paged claim type list
        /// </summary>
        [HttpPost("get-paged-list")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] ClaimTypeModel model,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            if (page < 1 || pageSize < 1)
            {
                return Ok(ApiResponse.Error(40001, "Invalid page or pageSize parameters"));
            }

            var result = await _service.GetPagedListAsync(model, page, pageSize);
            return Ok(ApiResponse<object>.Success(result));
        }

        /// <summary>
        /// Get all claim types
        /// </summary>
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ClaimTypeDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Create claim type
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> InsertAsync([FromBody] ClaimTypeDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return Ok(ApiResponse<int>.Error(40002, "Invalid claim type data or missing name"));
            }

            var result = await _service.InsertAsync(dto);

            await _auditLogHelper.LogAsync(
                "CreateClaimType",
                "ClaimType",
                dto.Id ?? Guid.Empty,
                $"Created claim type: {dto.Name}",
                null,
                dto
            );

            return Ok(ApiResponse<int>.Success(result, "ClaimType created successfully"));
        }

        /// <summary>
        /// Update claim type
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] ClaimTypeDto dto)
        {
            if (dto?.Id == null)
            {
                return Ok(ApiResponse<bool>.Error(40003, "Invalid claim type data or missing ID"));
            }

            var result = await _service.UpdateAsync(dto);

            await _auditLogHelper.LogAsync(
                "UpdateClaimType",
                "ClaimType",
                dto.Id.Value,
                $"Updated claim type: {dto.Name}",
                null,
                dto
            );

            return Ok(ApiResponse<bool>.Success(result, "ClaimType updated successfully"));
        }

        /// <summary>
        /// Delete claim type
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Ok(ApiResponse<bool>.Error(40005, "Invalid ID"));
            }

            var result = await _service.DeleteAsync(new ClaimTypeDto { Id = id });

            await _auditLogHelper.LogAsync(
                "DeleteClaimType",
                "ClaimType",
                id,
                $"Deleted claim type: {id}"
            );

            return Ok(ApiResponse<bool>.Success(result, "ClaimType deleted successfully"));
        }
    }
}
