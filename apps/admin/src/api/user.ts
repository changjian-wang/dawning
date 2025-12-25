import axios from '@/api/interceptor';
import type { RouteRecordNormalized } from 'vue-router';
import type { UserState } from '@/store/modules/user/types';

export interface LoginData {
  username: string;
  password: string;
}

export interface LoginRes {
  token: string;
}
export function login(data: LoginData) {
  return axios.post<LoginRes>('/api/user/login', data);
}

export function logout() {
  return axios.post<LoginRes>('/connect/logout', {}, {
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
  });
}

export function getUserInfo() {
  return axios.get<UserState>('/api/user/info');
}

export function getMenuList() {
  return axios.get<RouteRecordNormalized[]>('/api/user/menu');
}
