using System;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    /// <summary>
    /// Application 映射器
    /// </summary>
    public static class ApplicationMappers
    {
        private static IMapper Mapper { get; }

        static ApplicationMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ApplicationProfile>())
                .CreateMapper();
        }

        public static Application ToModel(this ApplicationEntity entity)
        {
            return Mapper.Map<Application>(entity);
        }

        public static IEnumerable<Application> ToModels(this IEnumerable<ApplicationEntity> entities)
        {
            return Mapper.Map<IEnumerable<Application>>(entities);
        }

        public static ApplicationEntity ToEntity(this Application model)
        {
            return Mapper.Map<ApplicationEntity>(model);
        }
    }
}

