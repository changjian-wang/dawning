using System.Reflection;
using Dawning.Identity.Application.Services.Monitoring;
using Dawning.Identity.Application.Services.MultiTenancy;
using Dawning.Identity.Domain.Interfaces.MultiTenancy;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Infra.Data.Repository.UoW;
using Microsoft.Extensions.DependencyInjection;

namespace Dawning.Identity.Infra.CrossCutting.IoC
{
    public static class NativeInjectorBootStrapper
    {
        const string NAME_SPACE = "Dawning.Identity";
        const string APPLICATION_ASSEMBLY_NAME = $"{NAME_SPACE}.Application";
        const string INFRA_DATA_ASSEMBLY_NAME = $"{NAME_SPACE}.Infra.Data";
        const string SERVICE_SUFFIX_NAME = "Service";
        const string REPOSITORY_SUFFIX_NAME = "Repository";

        /// <summary>
        /// Register application services
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register HttpClientFactory (for Webhook notifications)
            services.AddHttpClient();

            // Register alert check background service
            services.AddHostedService<AlertCheckBackgroundService>();

            // Register multi-tenant context (Scoped, one instance per request)
            services.AddScoped<ITenantContext, TenantContext>();

            // Scan and register services and repositories from assemblies
            foreach (var assembly in LoadAssemblies())
            {
                RegisterServices(services, assembly);
            }

            return services;
        }

        /// <summary>
        /// Register services
        /// </summary>
        private static void RegisterServices(IServiceCollection services, Assembly assembly)
        {
            var serviceTypes = assembly
                .GetExportedTypes()
                .Where(t => !t.IsAbstract && IsServiceOrRepository(t));

            foreach (var type in serviceTypes)
            {
                foreach (var interfaceType in type.GetRelevantInterfaces())
                {
                    services.AddScoped(interfaceType, type);
                }
            }
        }

        /// <summary>
        /// Check if type is a Service or Repository
        /// </summary>
        private static bool IsServiceOrRepository(Type type) =>
            type.Name.EndsWith(SERVICE_SUFFIX_NAME) || type.Name.EndsWith(REPOSITORY_SUFFIX_NAME);

        /// <summary>
        /// Get relevant interfaces (only return project internal interfaces)
        /// </summary>
        private static IEnumerable<Type> GetRelevantInterfaces(this Type type) =>
            type.GetInterfaces().Where(i => i.Namespace?.StartsWith(NAME_SPACE) ?? false);

        /// <summary>
        /// Load assemblies
        /// </summary>
        private static IEnumerable<Assembly> LoadAssemblies()
        {
            yield return Assembly.Load(APPLICATION_ASSEMBLY_NAME);
            yield return Assembly.Load(INFRA_DATA_ASSEMBLY_NAME);
        }
    }
}
