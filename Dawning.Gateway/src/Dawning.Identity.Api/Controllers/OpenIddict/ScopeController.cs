using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Models.OpenIddict;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.OpenIddict
{
    /// <summary>
    /// Controller for managing OpenIddict scopes, providing endpoints to perform CRUD operations and paging.
    /// </summary>
    [ApiController]
    [Route("api/openiddict/scope")]
    [Authorize(Roles = "admin,super_admin")]
    public class ScopeController : ControllerBase
    {
        private readonly IScopeService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeController"/> class.
        /// This constructor takes an IScopeService as a parameter, which is used to handle
        /// the business logic related to scopes. The service is injected via dependency injection,
        /// promoting loose coupling and easier testing.
        /// </summary>
        public ScopeController(IScopeService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a scope by its unique identifier.
        /// </summary>
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var result = await _service.GetAsync(id);
            if (result == null || result.Id == null)
            {
                return Ok(ApiResponse<ScopeDto>.Error(40404, "Scope not found"));
            }
            return Ok(ApiResponse<ScopeDto>.Success(result));
        }

        /// <summary>
        /// Retrieves a scope by its name.
        /// </summary>
        [HttpGet("get-by-name/{name}")]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Ok(ApiResponse<ScopeDto>.Error(40001, "Invalid scope name"));
            }

            var result = await _service.GetByNameAsync(name);
            if (result == null)
            {
                return Ok(ApiResponse<ScopeDto>.Error(40404, "Scope not found"));
            }
            return Ok(ApiResponse<ScopeDto>.Success(result));
        }

        /// <summary>
        /// Retrieves scopes by their names.
        /// </summary>
        [HttpPost("get-by-names")]
        public async Task<IActionResult> GetByNamesAsync([FromBody] IEnumerable<string> names)
        {
            if (names == null || !names.Any())
            {
                return Ok(ApiResponse<IEnumerable<ScopeDto>>.Error(40001, "Invalid scope names"));
            }

            var result = await _service.GetByNamesAsync(names);
            return Ok(ApiResponse<IEnumerable<ScopeDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Retrieves a paged list of scopes.
        /// </summary>
        [HttpPost("get-paged-list")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] ScopeModel model,
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
        /// Retrieves all scopes.
        /// </summary>
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ScopeDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Inserts a new scope.
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> InsertAsync([FromBody] ScopeDto scopeDto)
        {
            if (scopeDto == null || string.IsNullOrWhiteSpace(scopeDto.Name))
            {
                return Ok(ApiResponse<int>.Error(40002, "Invalid scope data or missing name"));
            }

            var result = await _service.InsertAsync(scopeDto);
            return Ok(ApiResponse<int>.Success(result, "Scope created successfully"));
        }

        /// <summary>
        /// Updates an existing scope.
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] ScopeDto scopeDto)
        {
            if (scopeDto?.Id == null)
            {
                return Ok(ApiResponse<bool>.Error(40003, "Invalid scope data or missing ID"));
            }

            var result = await _service.UpdateAsync(scopeDto);
            return Ok(ApiResponse<bool>.Success(result, "Scope updated successfully"));
        }

        /// <summary>
        /// Deletes a scope by its unique identifier.
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Ok(ApiResponse<bool>.Error(40005, "Invalid ID"));
            }

            var result = await _service.DeleteAsync(new ScopeDto { Id = id });
            return Ok(ApiResponse<bool>.Success(result, "Scope deleted successfully"));
        }
    }
}
