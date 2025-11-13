using System;
using AutoMapper;
using Dawning.Auth.Application.Dtos.Administration;
using Dawning.Auth.Domain.Aggregates.Administration;

namespace Dawning.Auth.Application.Mapping.Administration
{
	public class ClaimTypeProfile : Profile
	{
        public ClaimTypeProfile()
        {
            CreateMap<ClaimType, ClaimTypeDto>(MemberList.Destination).ReverseMap();
        }
    }
}

