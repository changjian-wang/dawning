using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// 用户实体与领域模型映射器
    /// </summary>
    public static class UserMappers
    {
        private static IMapper Mapper { get; }

        static UserMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>()).CreateMapper();
        }

        /// <summary>
        /// 将用户实体转换为领域模型
        /// </summary>
        public static User ToModel(this UserEntity entity)
        {
            return Mapper.Map<User>(entity);
        }

        /// <summary>
        /// 将用户实体集合转换为领域模型集合
        /// </summary>
        public static IEnumerable<User> ToModels(this IEnumerable<UserEntity> entities)
        {
            return entities.Select(e => e.ToModel());
        }

        /// <summary>
        /// 将领域模型转换为用户实体
        /// </summary>
        public static UserEntity ToEntity(this User model)
        {
            return Mapper.Map<UserEntity>(model);
        }
    }
}
