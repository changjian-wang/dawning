using System;
using AutoMapper;
using Dawning.Auth.Application.Dtos;
using Dawning.Auth.Domain.Entities.IdentityServer;

namespace Dawning.Auth.Application.Mapping.IdentityServer
{
    /// <summary>
    /// Defines entity/model mapping for API resources.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class ApiResourceMapperProfile : Profile
    {
        /// <summary>
        /// <see cref="ApiResourceMapperProfile"/>
        /// </summary>
		public ApiResourceMapperProfile()
		{
            CreateMap<ApiResource, ApiResourceDto>().ReverseMap();

            CreateMap<ApiResourceClaim, ApiResourceClaimDto>().ReverseMap();

            CreateMap<ApiResourceSecret, ApiResourceSecretDto>().ReverseMap();

            CreateMap<ApiResourceScope, ApiResourceScopeDto>().ReverseMap();
		}
	}
}

