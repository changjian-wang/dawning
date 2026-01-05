using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Application.Mapping.Administration;
using Dawning.Identity.Application.Mapping.OpenIddict;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Core.Security;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;

namespace Dawning.Identity.Application.Services.OpenIddict
{
    public class ApplicationService : IApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// Service for managing OpenIddict applications
        /// </summary>
        public ApplicationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Validate URI format
        /// </summary>
        private bool IsValidUri(string uri)
        {
            return Uri.TryCreate(uri, UriKind.Absolute, out var uriResult)
                && (
                    uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps
                );
        }

        /// <summary>
        /// Validate redirect URI list
        /// </summary>
        private void ValidateRedirectUris(List<string> uris, string fieldName)
        {
            if (uris == null || uris.Count == 0)
            {
                return; // Allow empty
            }

            foreach (var uri in uris)
            {
                if (!IsValidUri(uri))
                {
                    throw new ArgumentException($"Invalid {fieldName}: {uri}");
                }
            }
        }

        /// <summary>
        /// Record audit log
        /// </summary>
        private async Task LogAuditAsync(
            Guid userId,
            string action,
            string entityType,
            Guid entityId,
            string description
        )
        {
            var auditLog = AuditLogMappers.CustomAudit(
                action,
                entityType,
                entityId,
                description,
                userId
            );

            await _unitOfWork.AuditLog.InsertAsync(auditLog);
        }

        public async Task<ApplicationDto> GetAsync(Guid id)
        {
            Domain.Aggregates.OpenIddict.Application model = await _unitOfWork.Application.GetAsync(
                id
            );
            return model?.ToDto() ?? new ApplicationDto();
        }

        public async Task<ApplicationDto?> GetByClientIdAsync(string clientId)
        {
            Domain.Aggregates.OpenIddict.Application? model =
                await _unitOfWork.Application.GetByClientIdAsync(clientId);
            return model?.ToDto();
        }

        public async Task<PagedData<ApplicationDto>> GetPagedListAsync(
            ApplicationModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedData<Domain.Aggregates.OpenIddict.Application> data =
                await _unitOfWork.Application.GetPagedListAsync(model, page, itemsPerPage);

            PagedData<ApplicationDto> pageResult = new PagedData<ApplicationDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<ApplicationDto>(),
            };

            return pageResult;
        }

        public async Task<IEnumerable<ApplicationDto>?> GetAllAsync()
        {
            var list = await _unitOfWork.Application.GetAllAsync();
            return list?.ToDtos() ?? new List<ApplicationDto>();
        }

        public async ValueTask<int> InsertAsync(ApplicationDto dto)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.ClientId))
            {
                throw new ArgumentException("ClientId is required");
            }

            if (string.IsNullOrWhiteSpace(dto.DisplayName))
            {
                throw new ArgumentException("DisplayName is required");
            }

            // Check ClientId uniqueness
            var existing = await _unitOfWork.Application.GetByClientIdAsync(dto.ClientId);
            if (existing != null)
            {
                throw new InvalidOperationException($"ClientId '{dto.ClientId}' already exists");
            }

            // Validate redirect URIs
            ValidateRedirectUris(dto.RedirectUris, "RedirectUri");
            ValidateRedirectUris(dto.PostLogoutRedirectUris, "PostLogoutRedirectUri");

            // Validate client type
            if (dto.Type != null && dto.Type != "confidential" && dto.Type != "public")
            {
                throw new ArgumentException("Type must be 'confidential' or 'public'");
            }

            Domain.Aggregates.OpenIddict.Application model =
                _mapper.Map<Domain.Aggregates.OpenIddict.Application>(dto);

            // If confidential type and has ClientSecret, use PBKDF2 hash
            if (!string.IsNullOrEmpty(dto.ClientSecret) && dto.Type == "confidential")
            {
                model.ClientSecret = PasswordHasher.Hash(dto.ClientSecret);
            }

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            var result = await _unitOfWork.Application.InsertAsync(model);

            // Record audit log
            await LogAuditAsync(
                _currentUserService.UserId ?? Guid.Empty,
                "Create",
                "Application",
                model.Id,
                $"Created OpenIddict application: {model.ClientId} ({model.DisplayName})"
            );

            return result;
        }

        public async ValueTask<bool> UpdateAsync(ApplicationDto dto)
        {
            if (dto.Id == null || dto.Id == Guid.Empty)
            {
                throw new ArgumentException("Id is required");
            }

            // Validate application exists
            var existing = await _unitOfWork.Application.GetAsync(dto.Id.Value);
            if (existing == null)
            {
                throw new InvalidOperationException($"Application with ID '{dto.Id}' not found");
            }

            // If ClientId was modified, check uniqueness of new value
            if (!string.IsNullOrWhiteSpace(dto.ClientId) && dto.ClientId != existing.ClientId)
            {
                var duplicateCheck = await _unitOfWork.Application.GetByClientIdAsync(dto.ClientId);
                if (duplicateCheck != null)
                {
                    throw new InvalidOperationException(
                        $"ClientId '{dto.ClientId}' already exists"
                    );
                }
            }

            // Validate redirect URIs
            ValidateRedirectUris(dto.RedirectUris, "RedirectUri");
            ValidateRedirectUris(dto.PostLogoutRedirectUris, "PostLogoutRedirectUri");

            Domain.Aggregates.OpenIddict.Application model =
                _mapper.Map<Domain.Aggregates.OpenIddict.Application>(dto);

            // If new secret is provided, use PBKDF2 hash
            if (!string.IsNullOrEmpty(dto.ClientSecret) && dto.Type == "confidential")
            {
                // Only re-hash if the secret is not already in hashed format
                if (!dto.ClientSecret.StartsWith("$PBKDF2$"))
                {
                    model.ClientSecret = PasswordHasher.Hash(dto.ClientSecret);
                }
            }

            model.UpdatedAt = DateTime.UtcNow;

            var result = await _unitOfWork.Application.UpdateAsync(model);

            // Record audit log
            await LogAuditAsync(
                _currentUserService.UserId ?? Guid.Empty,
                "Update",
                "Application",
                model.Id,
                $"Updated OpenIddict application: {model.ClientId} ({model.DisplayName})"
            );

            return result;
        }

        public async ValueTask<bool> DeleteAsync(ApplicationDto dto)
        {
            if (dto?.Id == null || dto.Id == Guid.Empty)
            {
                throw new ArgumentException("Id is required");
            }

            // Validate application exists
            var existing = await _unitOfWork.Application.GetAsync(dto.Id.Value);
            if (existing == null)
            {
                throw new InvalidOperationException($"Application with ID '{dto.Id}' not found");
            }

            Domain.Aggregates.OpenIddict.Application model =
                dto?.ToModel() ?? new Domain.Aggregates.OpenIddict.Application();

            var result = await _unitOfWork.Application.DeleteAsync(model);

            // Record audit log
            await LogAuditAsync(
                _currentUserService.UserId ?? Guid.Empty,
                "Delete",
                "Application",
                existing.Id,
                $"Deleted OpenIddict application: {existing.ClientId} ({existing.DisplayName})"
            );

            return result;
        }
    }
}
