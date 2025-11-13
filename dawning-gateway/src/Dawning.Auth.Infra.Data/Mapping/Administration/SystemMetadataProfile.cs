using System;
using AutoMapper;
using Dawning.Auth.Domain.Aggregates.Administration;
using Dawning.Auth.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Auth.Infra.Data.Mapping.Administration
{
	public class SystemMetadataProfile : Profile
	{
		public SystemMetadataProfile()
		{
            CreateMap<SystemMetadataEntity, SystemMetadata>(MemberList.Destination).ReverseMap();
        }
	}
}

