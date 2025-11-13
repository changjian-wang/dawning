// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


#pragma warning disable 1591

namespace Dawning.Auth.Application.Dtos
{
    public class IdentityResourceClaimDto : UserClaimDto
    {
        public Guid IdentityResourceId { get; set; }

        public IdentityResourceDto IdentityResource { get; set; }
    }
}