using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Models.Administration
{
	public class SystemMetadataModel
	{
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 键
        /// </summary>
        public string? Key { get; set; }
    }
}

