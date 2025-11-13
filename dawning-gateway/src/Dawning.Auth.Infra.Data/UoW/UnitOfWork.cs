using Dawning.Auth.Domain.Interfaces;
using Dawning.Auth.Domain.Interfaces.Administration;

namespace Dawning.Auth.Infra.Data.UoW
{
    /// <summary>
    /// Unit of Work implementation
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Claim type repository
        /// </summary>
        public IClaimTypeRepository ClaimType { get; }

        /// <summary>
        /// System metadata repository
        /// </summary>
        public ISystemMetadataRepository SystemMetadata { get; }

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class
        /// </summary>
        /// <param name="claimType">The claim type repository</param>
        /// <param name="systemMetadata">The system metadata repository</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
        public UnitOfWork(
            IClaimTypeRepository claimType,
            ISystemMetadataRepository systemMetadata)
        {
            ClaimType = claimType ?? throw new ArgumentNullException(nameof(claimType));
            SystemMetadata = systemMetadata ?? throw new ArgumentNullException(nameof(systemMetadata));
        }
    }
}
