using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Models.OpenIddict;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.OpenIddict
{
    /// <summary>
    /// Controller for managing OpenIddict tokens, providing endpoints to perform CRUD operations and paging.
    /// </summary>
    [ApiController]
    [Route("api/openiddict/token")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenController"/> class.
        /// This constructor takes an ITokenService as a parameter, which is used to handle
        /// the business logic related to tokens. The service is injected via dependency injection,
        /// promoting loose coupling and easier testing.
        /// </summary>
        public TokenController(ITokenService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a token by its unique identifier.
        /// </summary>
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var result = await _service.GetAsync(id);
            if (result == null || result.Id == null)
            {
                return Ok(ApiResponse<TokenDto>.Error(40404, "Token not found"));
            }
            return Ok(ApiResponse<TokenDto>.Success(result));
        }

        /// <summary>
        /// Retrieves a token by its reference ID.
        /// </summary>
        [HttpGet("get-by-reference/{referenceId}")]
        public async Task<IActionResult> GetByReferenceIdAsync(string referenceId)
        {
            if (string.IsNullOrWhiteSpace(referenceId))
            {
                return Ok(ApiResponse<TokenDto>.Error(40001, "Invalid reference ID"));
            }

            var result = await _service.GetByReferenceIdAsync(referenceId);
            if (result == null)
            {
                return Ok(ApiResponse<TokenDto>.Error(40404, "Token not found"));
            }
            return Ok(ApiResponse<TokenDto>.Success(result));
        }

        /// <summary>
        /// Retrieves tokens by subject.
        /// </summary>
        [HttpGet("get-by-subject/{subject}")]
        public async Task<IActionResult> GetBySubjectAsync(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Ok(ApiResponse<IEnumerable<TokenDto>>.Error(40001, "Invalid subject"));
            }

            var result = await _service.GetBySubjectAsync(subject);
            return Ok(ApiResponse<IEnumerable<TokenDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Retrieves tokens by application ID.
        /// </summary>
        [HttpGet("get-by-application/{applicationId}")]
        public async Task<IActionResult> GetByApplicationIdAsync(Guid applicationId)
        {
            if (applicationId == Guid.Empty)
            {
                return Ok(ApiResponse<IEnumerable<TokenDto>>.Error(40001, "Invalid application ID"));
            }

            var result = await _service.GetByApplicationIdAsync(applicationId);
            return Ok(ApiResponse<IEnumerable<TokenDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Retrieves tokens by authorization ID.
        /// </summary>
        [HttpGet("get-by-authorization/{authorizationId}")]
        public async Task<IActionResult> GetByAuthorizationIdAsync(Guid authorizationId)
        {
            if (authorizationId == Guid.Empty)
            {
                return Ok(ApiResponse<IEnumerable<TokenDto>>.Error(40001, "Invalid authorization ID"));
            }

            var result = await _service.GetByAuthorizationIdAsync(authorizationId);
            return Ok(ApiResponse<IEnumerable<TokenDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Retrieves a paged list of tokens.
        /// </summary>
        [HttpPost("get-paged-list")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] TokenModel model,
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
        /// Retrieves all tokens.
        /// </summary>
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<TokenDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Inserts a new token.
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> InsertAsync([FromBody] TokenDto tokenDto)
        {
            if (tokenDto == null)
            {
                return Ok(ApiResponse<int>.Error(40002, "Invalid token data"));
            }

            var result = await _service.InsertAsync(tokenDto);
            return Ok(ApiResponse<int>.Success(result, "Token created successfully"));
        }

        /// <summary>
        /// Updates an existing token.
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] TokenDto tokenDto)
        {
            if (tokenDto?.Id == null)
            {
                return Ok(ApiResponse<bool>.Error(40003, "Invalid token data or missing ID"));
            }

            var result = await _service.UpdateAsync(tokenDto);
            return Ok(ApiResponse<bool>.Success(result, "Token updated successfully"));
        }

        /// <summary>
        /// Deletes a token by its unique identifier.
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Ok(ApiResponse<bool>.Error(40005, "Invalid ID"));
            }

            var result = await _service.DeleteAsync(new TokenDto { Id = id });
            return Ok(ApiResponse<bool>.Success(result, "Token deleted successfully"));
        }

        /// <summary>
        /// Prunes expired tokens from the database.
        /// </summary>
        [HttpPost("prune-expired")]
        public async Task<IActionResult> PruneExpiredTokensAsync()
        {
            var result = await _service.PruneExpiredTokensAsync();
            return Ok(ApiResponse<int>.Success(result, $"Successfully pruned {result} expired tokens"));
        }
    }
}
