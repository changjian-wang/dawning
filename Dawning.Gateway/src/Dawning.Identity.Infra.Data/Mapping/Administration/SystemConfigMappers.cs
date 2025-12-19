using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// A static class that provides mapping functionality between SystemConfigAggregate and SystemConfigEntity using AutoMapper.
    /// This class is part of the data mapping layer, facilitating the conversion of entities to domain models and vice versa.
    /// </summary>
    public static class SystemConfigMappers
    {
        /// <summary>
        /// Represents the AutoMapper IMapper instance used for mapping between SystemConfigAggregate and SystemConfigEntity.
        /// This property is initialized in a static constructor, ensuring that the mapping configuration is only set up once.
        /// It provides the core functionality to convert between the domain model (SystemConfigAggregate) and the data entity (SystemConfigEntity),
        /// facilitating operations such as saving, updating, and retrieving data from the database in a format that aligns with the domain logic.
        /// </summary>
        private static IMapper Mapper { get; }

        /// <summary>
        /// A static class that provides mapping functionality between SystemConfigAggregate and SystemConfigEntity using AutoMapper.
        /// This class is part of the data mapping layer, facilitating the conversion of entities to domain models and vice versa.
        /// </summary>
        static SystemConfigMappers()
        {
            Mapper = new MapperConfiguration(cfg =>
                cfg.AddProfile<SystemConfigProfile>()
            ).CreateMapper();
        }

        /// <summary>
        /// Converts a SystemConfigEntity to a SystemConfigAggregate model using AutoMapper.
        /// This method is an extension method and can be called on any instance of SystemConfigEntity.
        /// </summary>
        /// <param name="entity">The SystemConfigEntity to convert to a SystemConfigAggregate model.</param>
        /// <returns>A SystemConfigAggregate model that represents the data in the provided entity.</returns>
        public static SystemConfigAggregate ToModel(this SystemConfigEntity entity)
        {
            return Mapper.Map<SystemConfigAggregate>(entity);
        }

        /// <summary>
        /// Converts a collection of SystemConfigEntity objects to a collection of SystemConfigAggregate domain models.
        /// </summary>
        /// <param name="entities">The collection of SystemConfigEntity objects to convert.</param>
        /// <returns>An IEnumerable of SystemConfigAggregate representing the converted entities.</returns>
        public static IEnumerable<SystemConfigAggregate> ToModels(
            this IEnumerable<SystemConfigEntity> entities
        )
        {
            return Mapper.Map<IEnumerable<SystemConfigAggregate>>(entities);
        }

        /// <summary>
        /// Converts a SystemConfigAggregate domain model to a SystemConfigEntity using AutoMapper.
        /// This method is used for mapping domain models to their corresponding entity representations, typically before saving to the database.
        /// </summary>
        /// <param name="model">The SystemConfigAggregate domain model to be converted into an entity.</param>
        /// <returns>A SystemConfigEntity that represents the provided SystemConfigAggregate model, ready for database operations.</returns>
        public static SystemConfigEntity ToEntity(this SystemConfigAggregate model)
        {
            return Mapper.Map<SystemConfigEntity>(model);
        }
    }
}
