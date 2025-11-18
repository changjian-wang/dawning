using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Models.OpenIddict;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.OpenIddict
{
    /// <summary>
    /// Controller for managing OpenIddict applications, providing endpoints to perform CRUD operations and paging.
    /// </summary>
    [ApiController]
    [Route("api/openiddict/application")]
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
        /// Retrieves an application by its unique identifier.
        /// </summary>
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var result = await _service.GetAsync(id);
            if (result == null || result.Id == null)
            {
                return Ok(ApiResponse<ApplicationDto>.Error(40404, "Application not found"));
            }
            return Ok(ApiResponse<ApplicationDto>.Success(result));
        }

        /// <summary>
        /// Retrieves an application by its client ID.
        /// </summary>
        [HttpGet("get-by-client-id/{clientId}")]
        public async Task<IActionResult> GetByClientIdAsync(string clientId)
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

        /// <summary>
        /// Retrieves a paged list of applications.
        /// </summary>
        [HttpPost("get-paged-list")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] ApplicationModel model,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            if (page < 1 || size < 1)
            {
                return Ok(ApiResponse.Error(40001, "Invalid page or size parameters"));
            }

            var result = await _service.GetPagedListAsync(model, page, size);
            return Ok(ApiResponse<object>.Success(result));
        }

        /// <summary>
        /// Retrieves all applications.
        /// </summary>
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ApplicationDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Inserts a new application.
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> InsertAsync([FromBody] ApplicationDto applicationDto)
        {
            if (applicationDto == null || string.IsNullOrWhiteSpace(applicationDto.ClientId))
            {
                return Ok(ApiResponse<int>.Error(40002, "Invalid application data or missing client ID"));
            }

            var result = await _service.InsertAsync(applicationDto);
            return Ok(ApiResponse<int>.Success(result, "Application created successfully"));
        }

        /// <summary>
        /// Updates an existing application.
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] ApplicationDto applicationDto)
        {
            if (applicationDto?.Id == null)
            {
                return Ok(ApiResponse<bool>.Error(40003, "Invalid application data or missing ID"));
            }

            var result = await _service.UpdateAsync(applicationDto);
            return Ok(ApiResponse<bool>.Success(result, "Application updated successfully"));
        }

        /// <summary>
        /// Deletes an application by its unique identifier.
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Ok(ApiResponse<bool>.Error(40005, "Invalid ID"));
            }

            var result = await _service.DeleteAsync(new ApplicationDto { Id = id });
            return Ok(ApiResponse<bool>.Success(result, "Application deleted successfully"));
        }
    }
}
