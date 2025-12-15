using System.Reflection;
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
        /// 注册应用服务
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 注册 UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 扫描并注册程序集中的服务和仓储
            foreach (var assembly in LoadAssemblies())
            {
                RegisterServices(services, assembly);
            }

            return services;
        }

        /// <summary>
        /// 注册服务
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
        /// 判断是否为 Service 或 Repository
        /// </summary>
        private static bool IsServiceOrRepository(Type type) =>
            type.Name.EndsWith(SERVICE_SUFFIX_NAME) || type.Name.EndsWith(REPOSITORY_SUFFIX_NAME);

        /// <summary>
        /// 获取相关接口（只返回项目内部的接口）
        /// </summary>
        private static IEnumerable<Type> GetRelevantInterfaces(this Type type) =>
            type.GetInterfaces().Where(i => i.Namespace?.StartsWith(NAME_SPACE) ?? false);

        /// <summary>
        /// 加载程序集
        /// </summary>
        private static IEnumerable<Assembly> LoadAssemblies()
        {
            yield return Assembly.Load(APPLICATION_ASSEMBLY_NAME);
            yield return Assembly.Load(INFRA_DATA_ASSEMBLY_NAME);
        }
    }
}
