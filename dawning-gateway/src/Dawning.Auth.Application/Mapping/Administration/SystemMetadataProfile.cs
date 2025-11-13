using System;
using AutoMapper;
using Dawning.Auth.Application.Dtos.Administration;
using Dawning.Auth.Domain.Aggregates.Administration;

namespace Dawning.Auth.Application.Mapping.Administration
{
	public class SystemMetadataProfile : Profile
	{
		public SystemMetadataProfile()
		{
            CreateMap<SystemMetadata, SystemMetadataDto>(MemberList.Destination).ReverseMap();
        }
	}
}

