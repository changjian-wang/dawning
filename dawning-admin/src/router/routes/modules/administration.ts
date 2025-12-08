import { DEFAULT_LAYOUT } from '../base';
import { AppRouteRecordRaw } from '../types';

const ADMINISTRATION: AppRouteRecordRaw = {
  path: '/administration',
  name: 'administration',
  component: DEFAULT_LAYOUT,
  meta: {
    locale: 'menu.administration',
    requiresAuth: true,
    icon: 'icon-settings',
    order: 1,
  },
  children: [
    {
      path: 'user',
      name: 'User',
      component: () => import('@/views/administration/user/index.vue'),
      meta: {
        locale: 'menu.administration.user',
        requiresAuth: true,
        roles: ['*'],
      },
    },
    {
      path: 'claim-type',
      name: 'ClaimType',
      component: () => import('@/views/administration/claim-type/index.vue'),
      meta: {
        locale: 'menu.administration.claim.type',
        requiresAuth: true,
        roles: ['*'],
      },
    },
    {
      path: 'claim-type/:id/info',
      name: 'ClaimTypeInfo',
      component: () => import('@/views/administration/claim-type/info.vue'),
      meta: {
        locale: 'menu.info',
        requiresAuth: true,
        roles: ['*'],
        hideInMenu: true,
      },
    },
    {
      path: 'system-metadata',
      name: 'SystemMetadata',
      component: () =>
        import('@/views/administration/system-metadata/index.vue'),
      meta: {
        locale: 'menu.administration.system.metadata',
        requiresAuth: true,
        roles: ['*'],
      },
    },
    {
      path: 'system-metadata/:id/info',
      name: 'SystemMetadataInfo',
      component: () => import('@/views/administration/system-metadata/add.vue'),
      meta: {
        locale: 'menu.add',
        requiresAuth: true,
        roles: ['*'],
        hideInMenu: true,
      },
    },
  ],
};

export default ADMINISTRATION;
