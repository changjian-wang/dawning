// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


#pragma warning disable 1591

using IdentityServer4.Models;
using System;
using System.Collections.Generic;

namespace Dawning.Auth.Application.Dtos
{
    public class ClientDto
    {
        public Guid Id { get; set; }

        public bool Enabled { get; set; } = true;

        public string ClientId { get; set; }

        public string ProtocolType { get; set; } = "oidc";

        public ICollection<ClientSecretDto> ClientSecrets { get; set; }

        public bool RequireClientSecret { get; set; } = true;

        public string ClientName { get; set; }

        public string Description { get; set; }

        public string ClientUri { get; set; }

        public string LogoUri { get; set; }

        public bool RequireConsent { get; set; } = false;

        public bool AllowRememberConsent { get; set; } = true;

        public bool AlwaysIncludeUserClaimsInIdToken { get; set; }

        public ICollection<ClientGrantTypeDto> AllowedGrantTypes { get; set; }

        public bool RequirePkce { get; set; } = true;

        public bool AllowPlainTextPkce { get; set; }

        public bool RequireRequestObject { get; set; }

        public bool AllowAccessTokensViaBrowser { get; set; }

        public ICollection<ClientRedirectUriDto> RedirectUris { get; set; }

        public ICollection<ClientPostLogoutRedirectUriDto> PostLogoutRedirectUris { get; set; }

        public string FrontChannelLogoutUri { get; set; }

        public bool FrontChannelLogoutSessionRequired { get; set; } = true;

        public string BackChannelLogoutUri { get; set; }

        public bool BackChannelLogoutSessionRequired { get; set; } = true;

        public bool AllowOfflineAccess { get; set; }

        public ICollection<ClientScopeDto> AllowedScopes { get; set; }

        public int IdentityTokenLifetime { get; set; } = 300;

        public ICollection<string> AllowedIdentityTokenSigningAlgorithms { get; set; }

        public int AccessTokenLifetime { get; set; } = 3600;

        public int AuthorizationCodeLifetime { get; set; } = 300;

        public int? ConsentLifetime { get; set; } = null;

        public int AbsoluteRefreshTokenLifetime { get; set; } = 2592000;

        public int SlidingRefreshTokenLifetime { get; set; } = 1296000;

        public int RefreshTokenUsage { get; set; } = (int)TokenUsage.OneTimeOnly;

        public bool UpdateAccessTokenClaimsOnRefresh { get; set; }

        public int RefreshTokenExpiration { get; set; } = (int)TokenExpiration.Absolute;

        public int AccessTokenType { get; set; } = (int)0; // AccessTokenType.Jwt;

        public bool EnableLocalLogin { get; set; } = true;

        public ICollection<ClientIdPRestrictionDto> IdentityProviderRestrictions { get; set; }

        public bool IncludeJwtId { get; set; }

        public ICollection<ClientClaim> Claims { get; set; }

        public bool AlwaysSendClientClaims { get; set; }

        public string ClientClaimsPrefix { get; set; } = "client_";

        public string PairWiseSubjectSalt { get; set; }

        public ICollection<ClientCorsOriginDto> AllowedCorsOrigins { get; set; }

        public ICollection<ClientPropertyDto> Properties { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Updated { get; set; }

        public DateTime? LastAccessed { get; set; }

        public int? UserSsoLifetime { get; set; }

        public string UserCodeType { get; set; }

        public int DeviceCodeLifetime { get; set; } = 300;

        public bool NonEditable { get; set; }
    }
}