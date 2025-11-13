using Dawning.Auth.Application.Dtos.Administration;
using Dawning.Auth.Application.Interfaces.Administration;
using Dawning.Auth.Domain.Models.Administration;
using Dawning.Auth.Services.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Auth.Services.Api.Controllers.Administration
{
    /// <summary>
    /// Controller for managing claim types, providing endpoints to perform CRUD operations and paging.
    /// </summary>
    [ApiController]
    [Route("api/claim-type")]
    public class ClaimTypeController : ControllerBase
    {
        private readonly IClaimTypeService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimTypeController"/> class.
        /// This constructor takes an IClaimTypeService as a parameter, which is used to handle
        /// the business logic related to claim types. The service is injected via dependency injection,
        /// promoting loose coupling and easier testing.
        /// </summary>
        /// <param name="service"></param>
        public ClaimTypeController(IClaimTypeService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a claim type by its unique identifier.
        /// </summary>
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var result = await _service.GetAsync(id);
            if (result == null)
            {
                return Ok(ApiResponse<ClaimTypeDto>.Error(40004, "Claim type not found"));
            }
            return Ok(ApiResponse<ClaimTypeDto>.Success(result));
        }

        /// <summary>
        /// Retrieves a paged list of claim types.
        /// </summary>
        [HttpPost("get-paged-list")]
        public async Task<IActionResult> GetPagedListAsync(
            [FromBody] ClaimTypeModel model,
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
        /// Retrieves all claim types.
        /// </summary>
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ClaimTypeDto>>.Success(result ?? []));
        }

        /// <summary>
        /// Inserts a new claim type.
        /// </summary>
        [HttpPost("insert")]
        public async Task<IActionResult> InsertAsync([FromBody] ClaimTypeDto claimTypeDto)
        {
            if (claimTypeDto == null)
            {
                return Ok(ApiResponse<int>.Error(40002, "Invalid claim type data"));
            }

            var result = await _service.InsertAsync(claimTypeDto);
            return Ok(ApiResponse<int>.Success(result, "Claim type created successfully"));
        }

        /// <summary>
        /// Updates an existing claim type.
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] ClaimTypeDto claimTypeDto)
        {
            if (claimTypeDto?.Id == null)
            {
                return Ok(ApiResponse<bool>.Error(40003, "Invalid claim type data or missing ID"));
            }

            var result = await _service.UpdateAsync(claimTypeDto);
            return Ok(ApiResponse<bool>.Success(result, "Claim type updated successfully"));
        }

        /// <summary>
        /// Deletes a claim type by its unique identifier.
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Ok(ApiResponse<bool>.Error(40005, "Invalid ID"));
            }

            var result = await _service.DeleteAsync(new ClaimTypeDto { Id = id });
            return Ok(ApiResponse<bool>.Success(result, "Claim type deleted successfully"));
        }
    }
}