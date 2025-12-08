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
      path: 'openiddict',
      name: 'openIddict',
      component: undefined,
      meta: {
        locale: 'menu.administration.openiddict',
        requiresAuth: true,
        icon: 'icon-safe',
        order: 1,
      },
      children: [
        {
          path: 'application',
          name: 'Application',
          component: () =>
            import('@/views/administration/openiddict/application/index.vue'),
          meta: {
            locale: 'menu.administration.openiddict.application',
            requiresAuth: true,
            roles: ['*'],
          },
        },
        {
          path: 'client',
          name: 'Client',
          component: () =>
            import('@/views/administration/openiddict/client/index.vue'),
          meta: {
            locale: 'menu.administration.openiddict.client',
            requiresAuth: true,
            roles: ['*'],
          },
        },
        {
          path: 'client/add',
          name: 'ClientAdd',
          component: () => import('@/views/administration/openiddict/client/add.vue'),
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
          component: () => import('@/views/administration/openiddict/client/info.vue'),
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
            import('@/views/administration/openiddict/api-resource/index.vue'),
          meta: {
            locale: 'menu.administration.openiddict.api.resource',
            requiresAuth: true,
            roles: ['*'],
          },
        },
        {
          path: 'api-resource/add',
          name: 'ApiResourceAdd',
          component: () =>
            import('@/views/administration/openiddict/api-resource/add.vue'),
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
            import('@/views/administration/openiddict/api-resource/info.vue'),
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
            import('@/views/administration/openiddict/identity-resource/index.vue'),
          meta: {
            locale: 'menu.administration.openiddict.identity.resource',
            requiresAuth: true,
            roles: ['*'],
          },
        },
        {
          path: 'identity-resource/add',
          name: 'IdentityResourceAdd',
          component: () =>
            import('@/views/administration/openiddict/identity-resource/add.vue'),
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
