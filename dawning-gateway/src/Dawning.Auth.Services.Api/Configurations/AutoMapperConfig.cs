using AutoMapper;
using Dawning.Auth.Application.Mapping;
using Dawning.Auth.Infra.CrossCutting.Mapping;

namespace Dawning.Auth.Services.Api.Configurations
{
    public static class AutoMapperConfig
    {
        public static void AddAutoMapperConfiguration(this IServiceCollection services)
        {
            AutoMapperProfileBootStrapper.RegisterServices(services);
        }
    }
}
