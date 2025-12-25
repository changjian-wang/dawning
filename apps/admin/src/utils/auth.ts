const TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';
const ID_TOKEN_KEY = 'id_token';
const TOKEN_EXPIRES_AT_KEY = 'token_expires_at';

const isLogin = () => {
  const token = localStorage.getItem(TOKEN_KEY);
  const expiresAt = localStorage.getItem(TOKEN_EXPIRES_AT_KEY);

  if (!token || !expiresAt) {
    return false;
  }

  // 检查 token 是否过期
  const now = Date.now();
  return now < parseInt(expiresAt, 10);
};

const getToken = () => {
  return localStorage.getItem(TOKEN_KEY);
};

const getRefreshToken = () => {
  return localStorage.getItem(REFRESH_TOKEN_KEY);
};

const getIdToken = () => {
  return localStorage.getItem(ID_TOKEN_KEY);
};

const setToken = (token: string) => {
  localStorage.setItem(TOKEN_KEY, token);
};

const setRefreshToken = (token: string) => {
  localStorage.setItem(REFRESH_TOKEN_KEY, token);
};

const setIdToken = (token: string) => {
  localStorage.setItem(ID_TOKEN_KEY, token);
};

const setTokenExpiresAt = (expiresIn: number) => {
  const expiresAt = Date.now() + expiresIn * 1000;
  localStorage.setItem(TOKEN_EXPIRES_AT_KEY, expiresAt.toString());
};

const clearToken = () => {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(ID_TOKEN_KEY);
  localStorage.removeItem(TOKEN_EXPIRES_AT_KEY);
};

export {
  isLogin,
  getToken,
  getRefreshToken,
  getIdToken,
  setToken,
  setRefreshToken,
  setIdToken,
  setTokenExpiresAt,
  clearToken,
};
