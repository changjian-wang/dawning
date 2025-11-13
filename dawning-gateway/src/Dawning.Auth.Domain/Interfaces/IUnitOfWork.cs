using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.Auth.Domain.Interfaces.Administration;

namespace Dawning.Auth.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IClaimTypeRepository ClaimType { get; }

        ISystemMetadataRepository SystemMetadata { get; }
    }
}
