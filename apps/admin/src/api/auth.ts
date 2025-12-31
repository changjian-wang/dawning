import axios from 'axios';

export interface OAuthTokenRequest {
  grant_type: 'password' | 'refresh_token' | 'client_credentials';
  username?: string;
  password?: string;
  refresh_token?: string;
  client_id: string;
  client_secret?: string;
  scope?: string;
}

export interface OAuthTokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  refresh_token?: string;
  id_token?: string;
  scope?: string;
}

/**
 * OAuth 2.0 Password Grant login
 */
export function loginWithPassword(username: string, password: string) {
  const params = new URLSearchParams();
  params.append('grant_type', 'password');
  params.append('username', username);
  params.append('password', password);
  params.append('client_id', 'dawning-admin');
  params.append('scope', 'openid profile email roles api');

  return axios.post<OAuthTokenResponse>('/connect/token', params, {
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
  });
}

/**
 * OAuth 2.0 Refresh Token Grant - Refresh token
 */
export function refreshAccessToken(refreshToken: string) {
  const params = new URLSearchParams();
  params.append('grant_type', 'refresh_token');
  params.append('refresh_token', refreshToken);
  params.append('client_id', 'dawning-admin');

  return axios.post<OAuthTokenResponse>('/connect/token', params, {
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
  });
}

/**
 * Parse JWT Token to get user information
 */
export function parseJwtToken(token: string): any {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => `%${`00${c.charCodeAt(0).toString(16)}`.slice(-2)}`)
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}
