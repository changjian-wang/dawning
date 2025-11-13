using System;
namespace Dawning.Auth.Domain.Core.Interfaces
{
	public interface IAggregateRoot
	{
        /// <summary>
        /// 唯一码
        /// </summary>
        Guid Id { get; }
    }
}

