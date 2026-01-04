using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// Role permission association entity
    /// </summary>
    public class RolePermission : IAggregateRoot
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Role ID
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Permission ID
        /// </summary>
        public Guid PermissionId { get; set; }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Creator ID
        /// </summary>
        public Guid? CreatedBy { get; set; }
    }
}
