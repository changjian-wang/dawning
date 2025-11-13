using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;

namespace Dawning.Auth.Infra.CrossCutting.Mapping;
public class AutoMapperProfileBootStrapper
{
    const string APPLICATION_ASSEMBLY_NAME = "Dawning.Auth.Application";
    const string INFRA_DATA_ASSEMBLY_NAME = "Dawning.Auth.Infra.Data";

    public static void RegisterServices(IServiceCollection services)
    {
        // 注册AutoMapper
        services.AddAutoMapper(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(APPLICATION_ASSEMBLY_NAME)));
        services.AddAutoMapper(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(INFRA_DATA_ASSEMBLY_NAME)));
    }
}

