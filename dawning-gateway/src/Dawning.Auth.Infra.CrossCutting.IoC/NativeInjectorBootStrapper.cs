using System.Reflection;
using System.Runtime.Loader;
using Dawning.Auth.Domain.Interfaces;
using Dawning.Auth.Infra.Data.UoW;
using Microsoft.Extensions.DependencyInjection;

namespace Dawning.Auth.Infra.CrossCutting.IoC
{
    public static class NativeInjectorBootStrapper
    {
        const string NAME_SPACE = "Dawning.Auth";
        const string APPLICATION_ASSEMBLY_NAME = $"{NAME_SPACE}.Application";
        const string SERVICE_SUFFIX_NAME = "Service";
        const string INFRA_DATA_ASSEMBLY_NAME = $"{NAME_SPACE}.Infra.Data";
        const string REPOSITORY_SUFFIX_NAME = "Repository";

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            foreach (var assembly in LoadAssemblies())
            {
                RegisterServices(services, assembly);
            }
            return services;
        }

        private static void RegisterServices(IServiceCollection services, Assembly assembly)
        {
            var serviceTypes = assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && IsServiceOrRepository(t));

            foreach (var type in serviceTypes)
            {
                foreach (var interfaceType in type.GetRelevantInterfaces())
                {
                    services.AddScoped(interfaceType, type);
                }
            }
        }

        private static bool IsServiceOrRepository(Type type) =>
            type.Name.EndsWith(SERVICE_SUFFIX_NAME);

        private static IEnumerable<Type> GetRelevantInterfaces(this Type type) =>
            type.GetInterfaces().Where(i => i.Namespace?.StartsWith(NAME_SPACE) ?? false);

        private static IEnumerable<Assembly> LoadAssemblies()
        {
            yield return Assembly.Load(APPLICATION_ASSEMBLY_NAME);
            // yield return Assembly.Load(INFRA_DATA_ASSEMBLY_NAME);
        }
    }
}