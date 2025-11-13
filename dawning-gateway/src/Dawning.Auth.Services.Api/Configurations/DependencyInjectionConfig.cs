using Dawning.Auth.Infra.CrossCutting.IoC;

namespace Dawning.Auth.Services.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void AddDependencyInjectionConfiguration(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            NativeInjectorBootStrapper.AddApplicationServices(services);
        }
    }
}
