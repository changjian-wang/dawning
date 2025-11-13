// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


#pragma warning disable 1591

namespace Dawning.Auth.Application.Dtos
{
    public class ClientPostLogoutRedirectUriDto
    {
        public Guid Id { get; set; }

        public string PostLogoutRedirectUri { get; set; }

        public Guid ClientId { get; set; }

        public ClientDto Client { get; set; }
    }
}