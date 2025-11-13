/* -------- BEGIN administration --------*/
// Access Token Type
const AccessTokenType = {
  Jwt: 0,
  Reference: 1
} as const;

export type AccessTokenType = typeof AccessTokenType[keyof typeof AccessTokenType];

// Grant Type
export type GrantType = "implicit" | "hybrid" | "authorization_code" | "client_credentials" | "password" | "urn:ietf:params:oauth:grant-type:device_code"

export const GrantTypes = {
  Implicit: "implicit",
  Hybrid: "hybrid",
  AuthorizationCode: "authorization_code",
  ClientCredentials: "client_credentials",
  ResourceOwnerPassword: "password",
  DeviceFlow: "urn:ietf:params:oauth:grant-type:device_code",
} as const;

// Token Expiration
const TokenExpiration = {
  Sliding: 0,
  Absolute: 1
} as const;

export type TokenExpiration = typeof TokenExpiration[keyof typeof TokenExpiration];

// Token Usage
const TokenUsage = {
  ReUse: 0,
  OneTimeOnly: 1
} as const;

export type TokenUsage = typeof TokenUsage[keyof typeof TokenUsage];

// Redirect Type
export type RedirectType = "callback" | "signout";
export const RedirectTypes = {
  Callback: "callback",
  Signout: "signout",
} as const;
/* -------- END administration --------*/
