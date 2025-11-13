// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#pragma warning disable 1591

using System;
using System.Collections.Generic;

namespace Dawning.Auth.Application.Dtos
{
    public class ApiResourceDto
    {
        public Guid Id { get; set; }

        public bool Enabled { get; set; } = true;

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string AllowedAccessTokenSigningAlgorithms { get; set; }

        public bool ShowInDiscoveryDocument { get; set; } = true;

        public ICollection<ApiResourceSecretDto> Secrets { get; set; }

        public ICollection<ApiResourceScopeDto> Scopes { get; set; }

        public ICollection<ApiResourceClaimDto> UserClaims { get; set; }

        public ICollection<ApiResourcePropertyDto> Properties { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Updated { get; set; }

        public DateTime? LastAccessed { get; set; }

        public bool NonEditable { get; set; }
    }
}
