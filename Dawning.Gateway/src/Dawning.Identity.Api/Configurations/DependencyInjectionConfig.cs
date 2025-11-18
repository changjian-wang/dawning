using Dawning.Identity.Infra.CrossCutting.IoC;

namespace Dawning.Identity.Api.Configurations
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
