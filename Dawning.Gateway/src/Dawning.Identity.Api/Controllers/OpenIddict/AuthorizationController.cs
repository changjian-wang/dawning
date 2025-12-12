using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Models.OpenIddict;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.OpenIddict
{
    /// <summary>
    /// Controller for managing OpenIddict authorizations, providing endpoints to perform CRUD operations and paging.
    /// </summary>
    [ApiController]
    [Route("api/openiddict/authorization")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationController"/> class.
        /// This constructor takes an IAuthorizationService as a parameter, which is used to handle
        /// the business logic related to authorizations. The service is injected via dependency injection,
        /// promoting loose coupling and easier testing.
        /// </summary>
        public AuthorizationController(IAuthorizationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves an authorization by its unique identifier.
        /// </summary>
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var result = await _service.GetAsync(id);
            if (result == null || result.Id == null)
            {
                return Ok(ApiResponse<AuthorizationDto>.Error(40404, "Authorization not found"));
            }
            return Ok(ApiResponse<AuthorizationDto>.Success(result));
        }

        /// <summary>
        /// Retrieves authorizations by subject.
        /// </summary>
        [HttpGet("get-by-subject/{subject}")]
        public async Task<IActionResult> GetBySubjectAsync(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Ok(ApiResponse<IEnumerable<AuthorizationDto>>.Error(40001, "Invalid subject"));
            }

            var result = await _service.GetBySubjectAsync(subject);
            return Ok(ApiResponse<IEnumerable<AuthorizationDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Retrieves authorizations by application ID.
        /// </summary>
        [HttpGet("get-by-application/{applicationId}")]
        public async Task<IActionResult> GetByApplicationIdAsync(Guid applicationId)
        {
            if (applicationId == Guid.Empty)
            {
                return Ok(ApiResponse<IEnumerable<AuthorizationDto>>.Error(40001, "Invalid application ID"));
            }

            var result = await _service.GetByApplicationIdAsync(applicationId);
            return Ok(ApiResponse<IEnumerable<AuthorizationDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Retrieves a paged list of authorizations.
        /// </summary>
        [HttpPost("get-paged-list")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] AuthorizationModel model,
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
        /// Retrieves all authorizations.
        /// </summary>
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<AuthorizationDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Inserts a new authorization.
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> InsertAsync([FromBody] AuthorizationDto authorizationDto)
        {
            if (authorizationDto == null || string.IsNullOrWhiteSpace(authorizationDto.Subject))
            {
                return Ok(ApiResponse<int>.Error(40002, "Invalid authorization data or missing subject"));
            }

            var result = await _service.InsertAsync(authorizationDto);
            return Ok(ApiResponse<int>.Success(result, "Authorization created successfully"));
        }

        /// <summary>
        /// Updates an existing authorization.
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] AuthorizationDto authorizationDto)
        {
            if (authorizationDto?.Id == null)
            {
                return Ok(ApiResponse<bool>.Error(40003, "Invalid authorization data or missing ID"));
            }

            var result = await _service.UpdateAsync(authorizationDto);
            return Ok(ApiResponse<bool>.Success(result, "Authorization updated successfully"));
        }

        /// <summary>
        /// Deletes an authorization by its unique identifier.
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Ok(ApiResponse<bool>.Error(40005, "Invalid ID"));
            }

            var result = await _service.DeleteAsync(new AuthorizationDto { Id = id });
            return Ok(ApiResponse<bool>.Success(result, "Authorization deleted successfully"));
        }
    }
}
