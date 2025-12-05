using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;

namespace Dawning.Identity.Infra.Data.Mapping.Administration
{
    /// <summary>
    /// 用户实体与领域模型映射配置
    /// </summary>
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Entity -> Domain Model
            CreateMap<UserEntity, User>();

            // Domain Model -> Entity
            CreateMap<User, UserEntity>();
        }
    }
}
