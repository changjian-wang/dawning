using Dawning.Identity.Infra.CrossCutting.Mapping;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// AutoMapper configuration
    /// </summary>
    public static class AutoMapperConfig
    {
        public static void AddAutoMapperConfiguration(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            AutoMapperProfileBootStrapper.RegisterServices(services);
        }
    }
}
