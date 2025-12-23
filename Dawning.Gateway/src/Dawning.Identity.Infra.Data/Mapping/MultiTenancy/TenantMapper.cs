using Dawning.Identity.Domain.Aggregates.MultiTenancy;
using Dawning.Identity.Infra.Data.PersistentObjects.MultiTenancy;

namespace Dawning.Identity.Infra.Data.Mapping.MultiTenancy
{
    /// <summary>
    /// 租户实体映射扩展
    /// </summary>
    public static class TenantMapper
    {
        /// <summary>
        /// Entity -> Domain Model
        /// </summary>
        public static Tenant ToModel(this TenantEntity entity)
        {
            return new Tenant
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                Domain = entity.Domain,
                Email = entity.Email,
                Phone = entity.Phone,
                LogoUrl = entity.LogoUrl,
                Settings = entity.Settings,
                ConnectionString = entity.ConnectionString,
                IsActive = entity.IsActive,
                Plan = entity.Plan,
                SubscriptionExpiresAt = entity.SubscriptionExpiresAt,
                MaxUsers = entity.MaxUsers,
                MaxStorageMB = entity.MaxStorageMB,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy
            };
        }

        /// <summary>
        /// Domain Model -> Entity
        /// </summary>
        public static TenantEntity ToEntity(this Tenant model)
        {
            return new TenantEntity
            {
                Id = model.Id,
                Code = model.Code,
                Name = model.Name,
                Description = model.Description,
                Domain = model.Domain,
                Email = model.Email,
                Phone = model.Phone,
                LogoUrl = model.LogoUrl,
                Settings = model.Settings,
                ConnectionString = model.ConnectionString,
                IsActive = model.IsActive,
                Plan = model.Plan,
                SubscriptionExpiresAt = model.SubscriptionExpiresAt,
                MaxUsers = model.MaxUsers,
                MaxStorageMB = model.MaxStorageMB,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                CreatedBy = model.CreatedBy,
                UpdatedBy = model.UpdatedBy
            };
        }
    }
}
