using System;
namespace Dawning.Identity.Domain.Core.Interfaces
{
	public interface IAggregateRoot
	{
        /// <summary>
        /// 唯一码
        /// </summary>
        Guid Id { get; }
    }
}

