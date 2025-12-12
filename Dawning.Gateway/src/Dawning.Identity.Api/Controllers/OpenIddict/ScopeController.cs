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
        /// Gets the operator ID from JWT claims
        /// </summary>
        private Guid GetOperatorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub")
                           ?? User.FindFirst("user_id");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Retrieves a scope by its unique identifier.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            try
            {
                var result = await _service.GetAsync(id);
                if (result == null || result.Id == null)
                {
                    return Ok(ApiResponse<ScopeDto>.Error(40404, "Scope not found"));
                }
                return Ok(ApiResponse<ScopeDto>.Success(result));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<ScopeDto>.Error(50000, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves a scope by its name.
        /// </summary>
        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            try
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
            catch (Exception ex)
            {
                return Ok(ApiResponse<ScopeDto>.Error(50000, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves scopes by their names.
        /// </summary>
        [HttpPost("by-names")]
        public async Task<IActionResult> GetByNamesAsync([FromBody] IEnumerable<string> names)
        {
            try
            {
                if (names == null || !names.Any())
                {
                    return Ok(ApiResponse<IEnumerable<ScopeDto>>.Error(40001, "Invalid scope names"));
                }

                var result = await _service.GetByNamesAsync(names);
                return Ok(ApiResponse<IEnumerable<ScopeDto>>.Success(result ?? []));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<IEnumerable<ScopeDto>>.Error(50000, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves a paged list of scopes.
        /// </summary>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] ScopeModel model,
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
                return Ok(ApiResponse<object>.Error(50000, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves all scopes.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<ScopeDto>>.Success(result ?? []));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<IEnumerable<ScopeDto>>.Error(50000, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Inserts a new scope.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InsertAsync([FromBody] ScopeDto scopeDto)
        {
            try
            {
                if (scopeDto == null || string.IsNullOrWhiteSpace(scopeDto.Name))
                {
                    return Ok(ApiResponse<int>.Error(40002, "Invalid scope data or missing name"));
                }

                // Set operator ID for audit
                scopeDto.OperatorId = GetOperatorId();

                var result = await _service.InsertAsync(scopeDto);
                return Ok(ApiResponse<int>.Success(result, "Scope created successfully"));
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
        /// Updates an existing scope.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] ScopeDto scopeDto)
        {
            try
            {
                if (scopeDto == null)
                {
                    return Ok(ApiResponse<bool>.Error(40003, "Invalid scope data"));
                }

                // Ensure ID matches route parameter
                scopeDto.Id = id;

                // Set operator ID for audit
                scopeDto.OperatorId = GetOperatorId();

                var result = await _service.UpdateAsync(scopeDto);
                return Ok(ApiResponse<bool>.Success(result, "Scope updated successfully"));
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
        /// Deletes a scope by its unique identifier.
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

                var scopeDto = new ScopeDto
                {
                    Id = id,
                    OperatorId = GetOperatorId()
                };

                var result = await _service.DeleteAsync(scopeDto);
                return Ok(ApiResponse<bool>.Success(result, "Scope deleted successfully"));
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
