using System;
using Dawning.Auth.Dapper.Contrib;
using Dawning.Auth.Infra.CrossCutting.Utils;

namespace Dawning.Auth.Application.Dtos.Administration
{
	public class ClaimTypeDto
	{
        public Guid? Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 类型。String, Int, DateTime, Boolean, Enum
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 描述说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否必须项
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// 用户是否可编辑
        /// </summary>
        public bool NonEditable { get; set; } = true;

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; } = TimestampUtil.GetCurrentTimestamp();

        /// <summary>
        /// 创建时间
        /// </summary>
        [IgnoreUpdate]
        public DateTime? Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? Updated { get; set; }
    }
}

