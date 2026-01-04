using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// User role association aggregate root
    /// </summary>
    public class UserRole : IAggregateRoot
    {
        /// <summary>
        /// Association ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Role ID
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Assignment time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Assigner ID
        /// </summary>
        public Guid? CreatedBy { get; set; }
    }
}
