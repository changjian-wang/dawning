using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Models.OpenIddict;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dawning.Identity.Api.Controllers.OpenIddict
{
    /// <summary>
    /// Controller for managing OpenIddict applications, providing endpoints to perform CRUD operations and paging.
    /// </summary>
    [ApiController]
    [Route("api/openiddict/application")]
    [Authorize]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationController"/> class.
        /// This constructor takes an IApplicationService as a parameter, which is used to handle
        /// the business logic related to applications. The service is injected via dependency injection,
        /// promoting loose coupling and easier testing.
        /// </summary>
        public ApplicationController(IApplicationService service)
        {
            _service = service;
        }

        /// <summary>
        /// 从JWT Claims中获取操作员ID
        /// </summary>
        private Guid GetOperatorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value
                           ?? User.FindFirst("user_id")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Retrieves an application by its unique identifier.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            try
            {
                var result = await _service.GetAsync(id);
                if (result == null || result.Id == null)
                {
                    return Ok(ApiResponse<ApplicationDto>.Error(40404, "Application not found"));
                }
                return Ok(ApiResponse<ApplicationDto>.Success(result));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<ApplicationDto>.Error(50000, $"Error retrieving application: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves an application by its client ID.
        /// </summary>
        [HttpGet("by-client-id/{clientId}")]
        public async Task<IActionResult> GetByClientIdAsync(string clientId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    return Ok(ApiResponse<ApplicationDto>.Error(40001, "Invalid client ID"));
                }

                var result = await _service.GetByClientIdAsync(clientId);
                if (result == null)
                {
                    return Ok(ApiResponse<ApplicationDto>.Error(40404, "Application not found"));
                }
                return Ok(ApiResponse<ApplicationDto>.Success(result));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<ApplicationDto>.Error(50000, $"Error retrieving application: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves a paged list of applications.
        /// </summary>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] ApplicationModel model,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
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
                return Ok(ApiResponse.Error(50000, $"Error retrieving applications: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves all applications.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<ApplicationDto>>.Success(result ?? []));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.Error(50000, $"Error retrieving applications: {ex.Message}"));
            }
        }

        /// <summary>
        /// Creates a new application.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ApplicationDto applicationDto)
        {
            try
            {
                if (applicationDto == null || string.IsNullOrWhiteSpace(applicationDto.ClientId))
                {
                    return Ok(ApiResponse<int>.Error(40002, "Invalid application data or missing client ID"));
                }

                var result = await _service.InsertAsync(applicationDto);
                return Ok(ApiResponse<int>.Success(result, "Application created successfully"));
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
                return Ok(ApiResponse<int>.Error(50000, $"Error creating application: {ex.Message}"));
            }
        }

        /// <summary>
        /// Updates an existing application.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] ApplicationDto applicationDto)
        {
            try
            {
                if (applicationDto?.Id == null || applicationDto.Id != id)
                {
                    return Ok(ApiResponse<bool>.Error(40003, "Invalid application data or ID mismatch"));
                }

                var result = await _service.UpdateAsync(applicationDto);
                return Ok(ApiResponse<bool>.Success(result, "Application updated successfully"));
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
                return Ok(ApiResponse<bool>.Error(50000, $"Error updating application: {ex.Message}"));
            }
        }

        /// <summary>
        /// Deletes an application by its unique identifier.
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

                var result = await _service.DeleteAsync(new ApplicationDto { Id = id });
                return Ok(ApiResponse<bool>.Success(result, "Application deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return Ok(ApiResponse<bool>.Error(40009, ex.Message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<bool>.Error(50000, $"Error deleting application: {ex.Message}"));
            }
        }
    }
}
