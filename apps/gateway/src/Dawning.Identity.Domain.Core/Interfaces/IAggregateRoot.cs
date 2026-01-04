using System;

namespace Dawning.Identity.Domain.Core.Interfaces
{
    public interface IAggregateRoot
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        Guid Id { get; }
    }
}
