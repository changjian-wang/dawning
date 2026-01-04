using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Dawning.Identity.Infra.CrossCutting.Mapping
{
    public static class AutoMapperProfileBootStrapper
    {
        const string NAME_SPACE = "Dawning.Identity";
        const string APPLICATION_ASSEMBLY_NAME = $"{NAME_SPACE}.Application";
        const string INFRA_DATA_ASSEMBLY_NAME = $"{NAME_SPACE}.Infra.Data";

        /// <summary>
        /// Register AutoMapper services
        /// </summary>
        public static IServiceCollection RegisterServices(IServiceCollection services)
        {
            var assemblies = LoadAssemblies().ToList();
            services.AddAutoMapper(assemblies);
            return services;
        }

        /// <summary>
        /// Load assemblies containing AutoMapper Profiles
        /// </summary>
        private static IEnumerable<Assembly> LoadAssemblies()
        {
            yield return Assembly.Load(APPLICATION_ASSEMBLY_NAME);
            yield return Assembly.Load(INFRA_DATA_ASSEMBLY_NAME);
        }
    }
}
