import { DEFAULT_LAYOUT } from '../base';
import { AppRouteRecordRaw } from '../types';

const IDS: AppRouteRecordRaw = {
  path: '/ids',
  name: 'ids',
  component: DEFAULT_LAYOUT,
  meta: {
    locale: 'menu.ids',
    requiresAuth: true,
    icon: 'icon-safe',
    order: 2,
  },
  children: [
    {
      path: 'scope',
      name: 'Scope',
      component: () => import('@/views/ids/scope/index.vue'),
      meta: {
        locale: 'menu.ids.scope',
        requiresAuth: true,
        roles: ['admin', 'super_admin'],
      },
    },
    {
      path: 'identity-resource',
      name: 'IdentityResource',
      component: () => import('@/views/ids/identity-resource/index.vue'),
      meta: {
        locale: 'menu.ids.identityResource',
        requiresAuth: true,
        roles: ['admin', 'super_admin'],
      },
    },
    {
      path: 'api-resource',
      name: 'ApiResource',
      component: () => import('@/views/ids/api-resource/index.vue'),
      meta: {
        locale: 'menu.ids.apiResource',
        requiresAuth: true,
        roles: ['admin', 'super_admin'],
      },
    },
  ],
};

export default IDS;
