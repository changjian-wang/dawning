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
        locale: '元数据',
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
    {
      path: 'ids',
      name: 'identityServer',
      component: undefined,
      meta: {
        locale: 'menu.administration.ids',
        requiresAuth: true,
        icon: 'icon-safe',
        order: 1,
      },
      children: [
        {
          path: 'client',
          name: 'Client',
          component: () =>
            import('@/views/administration/ids/client/index.vue'),
          meta: {
            locale: 'menu.administration.ids.client',
            requiresAuth: true,
            roles: ['*'],
          },
        },
        {
          path: 'client/add',
          name: 'ClientAdd',
          component: () => import('@/views/administration/ids/client/add.vue'),
          meta: {
            locale: 'menu.add',
            requiresAuth: true,
            roles: ['*'],
            hideInMenu: true,
          },
        },
        {
          path: 'client/info',
          name: 'ClientInfo',
          component: () => import('@/views/administration/ids/client/info.vue'),
          meta: {
            locale: 'menu.info',
            requiresAuth: true,
            roles: ['*'],
            hideInMenu: true,
          },
        },
        {
          path: 'api-resource',
          name: 'ApiResource',
          component: () =>
            import('@/views/administration/ids/api-resource/index.vue'),
          meta: {
            locale: 'menu.administration.ids.api.resource',
            requiresAuth: true,
            roles: ['*'],
          },
        },
        {
          path: 'api-resource/add',
          name: 'ApiResourceAdd',
          component: () =>
            import('@/views/administration/ids/api-resource/add.vue'),
          meta: {
            locale: 'menu.add',
            requiresAuth: true,
            roles: ['*'],
            hideInMenu: true,
          },
        },
        {
          path: 'api-resource/info',
          name: 'ApiResourceInfo',
          component: () =>
            import('@/views/administration/ids/api-resource/info.vue'),
          meta: {
            locale: 'menu.info',
            requiresAuth: true,
            roles: ['*'],
            hideInMenu: true,
          },
        },
        {
          path: 'identity-resource',
          name: 'IdentityResource',
          component: () =>
            import('@/views/administration/ids/identity-resource/index.vue'),
          meta: {
            locale: 'menu.administration.ids.identity.resource',
            requiresAuth: true,
            roles: ['*'],
          },
        },
        {
          path: 'identity-resource/add',
          name: 'IdentityResourceAdd',
          component: () =>
            import('@/views/administration/ids/identity-resource/add.vue'),
          meta: {
            locale: 'menu.info',
            requiresAuth: true,
            roles: ['*'],
            hideInMenu: true,
          },
        },
      ],
    },
  ],
};

export default ADMINISTRATION;
