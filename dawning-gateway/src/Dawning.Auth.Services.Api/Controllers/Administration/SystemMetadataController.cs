using Dawning.Auth.Application.Dtos.Administration;
using Dawning.Auth.Application.Interfaces.Administration;
using Dawning.Auth.Domain.Models.Administration;
using Dawning.Auth.Services.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Auth.Services.Api.Controllers.Administration
{
    /// <summary>
    /// Controller for managing system metadatas, providing endpoints to perform CRUD operations and paging.
    /// </summary>
    [ApiController]
    [Route("api/system-metadata")]
    public class SystemMetadataController : ControllerBase
    {
        private readonly ISystemMetadataService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemMetadataController"/> class.
        /// This constructor takes an ISystemMetadataService as a parameter, which is used to handle
        /// the business logic related to system metadatas. The service is injected via dependency injection,
        /// promoting loose coupling and easier testing.
        /// </summary>
        /// <param name="service"></param>
        public SystemMetadataController(ISystemMetadataService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a system metadata by its unique identifier.
        /// </summary>
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var result = await _service.GetAsync(id);
            if (result == null)
            {
                return Ok(ApiResponse<SystemMetadataDto>.Error(40004, "system metadata not found"));
            }
            return Ok(ApiResponse<SystemMetadataDto>.Success(result));
        }

        /// <summary>
        /// Retrieves a paged list of system metadatas.
        /// </summary>
        [HttpPost("get-paged-list")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] SystemMetadataModel model,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10
        )
        {
            if (page < 1 || size < 1)
            {
                return Ok(ApiResponse.Error(40001, "Invalid page or size parameters"));
            }

            var result = await _service.GetPagedListAsync(model, page, size);
            return Ok(ApiResponse<object>.Success(result));
        }

        /// <summary>
        /// Inserts a new system metadata.
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> InsertAsync([FromBody] SystemMetadataDto SystemMetadataDto)
        {
            if (SystemMetadataDto == null)
            {
                return Ok(ApiResponse<int>.Error(40002, "Invalid system metadata data"));
            }

            var result = await _service.InsertAsync(SystemMetadataDto);
            return Ok(ApiResponse<int>.Success(result, "system metadata created successfully"));
        }

        /// <summary>
        /// Updates an existing system metadata.
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] SystemMetadataDto SystemMetadataDto)
        {
            if (SystemMetadataDto?.Id == null)
            {
                return Ok(
                    ApiResponse<bool>.Error(40003, "Invalid system metadata data or missing ID")
                );
            }

            var result = await _service.UpdateAsync(SystemMetadataDto);
            return Ok(ApiResponse<bool>.Success(result, "system metadata updated successfully"));
        }

        /// <summary>
        /// Deletes a system metadata by its unique identifier.
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Ok(ApiResponse<bool>.Error(40005, "Invalid ID"));
            }

            var result = await _service.DeleteAsync(new SystemMetadataDto { Id = id });
            return Ok(ApiResponse<bool>.Success(result, "system metadata deleted successfully"));
        }
    }
}