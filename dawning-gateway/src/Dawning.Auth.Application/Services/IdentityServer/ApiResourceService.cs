using System;
using Dawning.Auth.Application.Interfaces.IdentityServer;
using Dawning.Auth.Domain.Interfaces;

namespace Dawning.Auth.Application.Services.IdentityServer
{
	public class ApiResourceService : IApiResourceService
    {
		private readonly IUnitOfWork _unitOfWork;

		public ApiResourceService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
	}
}

