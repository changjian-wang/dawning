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
    /// Identity resource management controller
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/openiddict/identity-resource")]
    [Route("api/v{version:apiVersion}/openiddict/identity-resource")]
    [Authorize(Roles = "admin,super_admin")]
    public class IdentityResourceController : ControllerBase
    {
        private readonly IIdentityResourceService _service;

        public IdentityResourceController(IIdentityResourceService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get operator ID
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
        /// Get identity resource by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            try
            {
                var result = await _service.GetAsync(id);
                if (result == null || result.Id == null)
                {
                    return Ok(
                        ApiResponse<IdentityResourceDto>.Error(40404, "Identity Resource not found")
                    );
                }
                return Ok(ApiResponse<IdentityResourceDto>.Success(result));
            }
            catch (Exception ex)
            {
                return Ok(
                    ApiResponse<IdentityResourceDto>.Error(
                        50000,
                        $"Internal server error: {ex.Message}"
                    )
                );
            }
        }

        /// <summary>
        /// Get identity resource by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Ok(
                        ApiResponse<IdentityResourceDto>.Error(40001, "Invalid resource name")
                    );
                }

                var result = await _service.GetByNameAsync(name);
                if (result == null)
                {
                    return Ok(
                        ApiResponse<IdentityResourceDto>.Error(40404, "Identity Resource not found")
                    );
                }
                return Ok(ApiResponse<IdentityResourceDto>.Success(result));
            }
            catch (Exception ex)
            {
                return Ok(
                    ApiResponse<IdentityResourceDto>.Error(
                        50000,
                        $"Internal server error: {ex.Message}"
                    )
                );
            }
        }

        /// <summary>
        /// Get identity resources by name list
        /// </summary>
        [HttpPost("by-names")]
        public async Task<IActionResult> GetByNamesAsync([FromBody] IEnumerable<string> names)
        {
            try
            {
                if (names == null || !names.Any())
                {
                    return Ok(
                        ApiResponse<IEnumerable<IdentityResourceDto>>.Error(
                            40001,
                            "Invalid resource names"
                        )
                    );
                }

                var result = await _service.GetByNamesAsync(names);
                return Ok(
                    ApiResponse<IEnumerable<IdentityResourceDto>>.Success(
                        result ?? Array.Empty<IdentityResourceDto>()
                    )
                );
            }
            catch (Exception ex)
            {
                return Ok(
                    ApiResponse<IEnumerable<IdentityResourceDto>>.Error(
                        50000,
                        $"Internal server error: {ex.Message}"
                    )
                );
            }
        }

        /// <summary>
        /// Get paginated list
        /// </summary>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] IdentityResourceModel model,
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
        /// Get all identity resources
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(
                    ApiResponse<IEnumerable<IdentityResourceDto>>.Success(
                        result ?? Array.Empty<IdentityResourceDto>()
                    )
                );
            }
            catch (Exception ex)
            {
                return Ok(
                    ApiResponse<IEnumerable<IdentityResourceDto>>.Error(
                        50000,
                        $"Internal server error: {ex.Message}"
                    )
                );
            }
        }

        /// <summary>
        /// Create identity resource
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InsertAsync([FromBody] IdentityResourceDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                {
                    return Ok(
                        ApiResponse<int>.Error(40002, "Invalid resource data or missing name")
                    );
                }

                // Set operator ID
                dto.OperatorId = GetOperatorId();

                var result = await _service.InsertAsync(dto);
                return Ok(
                    ApiResponse<int>.Success(result, "Identity Resource created successfully")
                );
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
        /// Update identity resource
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] IdentityResourceDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return Ok(ApiResponse<bool>.Error(40003, "Invalid resource data"));
                }

                // Ensure ID matches
                dto.Id = id;

                // Set operator ID
                dto.OperatorId = GetOperatorId();

                var result = await _service.UpdateAsync(dto);
                return Ok(
                    ApiResponse<bool>.Success(result, "Identity Resource updated successfully")
                );
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
        /// Delete identity resource
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

                var dto = new IdentityResourceDto { Id = id, OperatorId = GetOperatorId() };

                var result = await _service.DeleteAsync(dto);
                return Ok(
                    ApiResponse<bool>.Success(result, "Identity Resource deleted successfully")
                );
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
