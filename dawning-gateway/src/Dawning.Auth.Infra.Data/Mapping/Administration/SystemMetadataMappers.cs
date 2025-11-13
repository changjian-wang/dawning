using System;
using AutoMapper;
using Dawning.Auth.Domain.Aggregates.Administration;
using Dawning.Auth.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Auth.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// A static class that provides mapping functionality between SystemMetadata and SystemMetadataEntity using AutoMapper.
    /// This class is part of the data mapping layer, facilitating the conversion of entities to domain models and vice versa.
    /// </summary>
    public static class SystemMetadataMappers
    {
        /// <summary>
        /// Represents the AutoMapper IMapper instance used for mapping between SystemMetadata and SystemMetadataEntity.
        /// This property is initialized in a static constructor, ensuring that the mapping configuration is only set up once.
        /// It provides the core functionality to convert between the domain model (SystemMetadata) and the data entity (SystemMetadataEntity),
        /// facilitating operations such as saving, updating, and retrieving data from the database in a format that aligns with the domain logic.
        /// </summary>
        private static IMapper Mapper { get; }

        /// <summary>
        /// A static class that provides mapping functionality between SystemMetadata and SystemMetadataEntity using AutoMapper.
        /// This class is part of the data mapping layer, facilitating the conversion of entities to domain models and vice versa.
        /// </summary>
        static SystemMetadataMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<SystemMetadataProfile>())
                .CreateMapper();
        }

        /// <summary>
        /// Converts a SystemMetadataEntity to a SystemMetadata model using AutoMapper.
        /// This method is an extension method and can be called on any instance of SystemMetadataEntity.
        /// </summary>
        /// <param name="entity">The SystemMetadataEntity to convert to a SystemMetadata model.</param>
        /// <returns>A SystemMetadata model that represents the data in the provided entity.</returns>
        public static SystemMetadata ToModel(this SystemMetadataEntity entity)
        {
            return Mapper.Map<SystemMetadata>(entity);
        }

        /// <summary>
        /// Converts a collection of SystemMetadataEntity objects to a collection of SystemMetadata domain models.
        /// </summary>
        /// <param name="entities">The collection of SystemMetadataEntity objects to convert.</param>
        /// <returns>An IEnumerable of SystemMetadata representing the converted entities.</returns>
        public static IEnumerable<SystemMetadata> ToModels(this IEnumerable<SystemMetadataEntity> entities)
        {
            return Mapper.Map<IEnumerable<SystemMetadata>>(entities);
        }

        /// <summary>
        /// Converts a SystemMetadata domain model to a SystemMetadataEntity using AutoMapper.
        /// This method is used for mapping domain models to their corresponding entity representations, typically before saving to the database.
        /// </summary>
        /// <param name="model">The SystemMetadata domain model to be converted into an entity.</param>
        /// <returns>A SystemMetadataEntity that represents the provided SystemMetadata model, ready for database operations.</returns>
        public static SystemMetadataEntity ToEntity(this SystemMetadata model)
        {
            return Mapper.Map<SystemMetadataEntity>(model);
        }
    }
}

