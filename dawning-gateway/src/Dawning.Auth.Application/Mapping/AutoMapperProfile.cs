using System;
using System.Runtime.Loader;
using AutoMapper;

namespace Dawning.Auth.Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        const string NAME_SPACE = "Dawning.Auth";
        const string DOMAIN_ASSEMBLY_NAME = $"{NAME_SPACE}.Domain";
        const string APPLICATION_ASSEMBLY_NAME = $"{NAME_SPACE}.Application";

        /// <summary>
        /// Constructor
        /// </summary>
        public AutoMapperProfile()
        {
            List<Type> entities = AssemblyLoadContext.Default
                .LoadFromAssemblyPath(AppContext.BaseDirectory + $"{DOMAIN_ASSEMBLY_NAME}.dll")
                .DefinedTypes
                .Select(s => s.AsType())
                .Where(s => s.Namespace != null && s.Namespace.StartsWith($"{DOMAIN_ASSEMBLY_NAME}.Entities"))
                .ToList();

            List<Type> dtos = AssemblyLoadContext.Default
                .LoadFromAssemblyPath(AppContext.BaseDirectory + $"{APPLICATION_ASSEMBLY_NAME}.dll")
                .DefinedTypes
                .Select(s => s.AsType())
                .Where(s => s.Namespace != null && s.Namespace.StartsWith($"{APPLICATION_ASSEMBLY_NAME}.Dtos"))
                .ToList();

            if (entities.Any() && dtos.Any())
            {
                entities.ForEach(entity =>
                {
                    dtos.ForEach(dto =>
                    {
                        if (dto.Name.EndsWith($"{entity.Name}Dto"))
                        {
                            CreateMap(entity, dto).ReverseMap();
                        }
                    });
                });
            }
        }
    }
}

