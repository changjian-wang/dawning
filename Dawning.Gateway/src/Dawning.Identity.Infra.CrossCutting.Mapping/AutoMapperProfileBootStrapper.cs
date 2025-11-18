using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Dawning.Identity.Infra.CrossCutting.Mapping
{
    public static class AutoMapperProfileBootStrapper
    {
        const string NAME_SPACE = "Dawning.Identity";
        const string APPLICATION_ASSEMBLY_NAME = $"{NAME_SPACE}.Application";
        const string INFRA_DATA_ASSEMBLY_NAME = $"{NAME_SPACE}.Infra.Data";

        /// <summary>
        /// 注册 AutoMapper 服务
        /// </summary>
        public static IServiceCollection RegisterServices(IServiceCollection services)
        {
            var assemblies = LoadAssemblies().ToList();
            services.AddAutoMapper(assemblies);
            return services;
        }

        /// <summary>
        /// 加载包含 Profile 的程序集
        /// </summary>
        private static IEnumerable<Assembly> LoadAssemblies()
        {
            yield return Assembly.Load(APPLICATION_ASSEMBLY_NAME);
            yield return Assembly.Load(INFRA_DATA_ASSEMBLY_NAME);
        }
    }
}
