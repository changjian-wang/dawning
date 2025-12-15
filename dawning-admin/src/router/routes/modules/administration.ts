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
      path: 'role',
      name: 'Role',
      component: () => import('@/views/administration/role/index.vue'),
      meta: {
        locale: 'menu.administration.role',
        requiresAuth: true,
        roles: ['*'],
      },
    },
    {
      path: 'permission',
      name: 'Permission',
      component: () => import('@/views/administration/permission/index.vue'),
      meta: {
        locale: 'menu.administration.permission',
        requiresAuth: true,
        roles: ['*'],
      },
    },
    {
      path: 'application',
      name: 'Application',
      component: () =>
        import('@/views/administration/openiddict/application/index.vue'),
      meta: {
        locale: 'menu.administration.application',
        requiresAuth: true,
        roles: ['*'],
      },
    },
    {
      path: 'scope',
      name: 'Scope',
      component: () =>
        import('@/views/administration/openiddict/scope/index.vue'),
      meta: {
        locale: 'menu.administration.scope',
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
    {
      path: 'audit-log',
      name: 'AuditLog',
      component: () => import('@/views/administration/audit-log/index.vue'),
      meta: {
        locale: 'menu.administration.auditLog',
        requiresAuth: true,
        roles: ['admin', 'super_admin', 'auditor'],
        icon: 'icon-history',
      },
    },
    {
      path: 'system-log',
      name: 'SystemLog',
      component: () => import('@/views/administration/system-log/index.vue'),
      meta: {
        locale: 'menu.administration.systemLog',
        requiresAuth: true,
        roles: ['admin', 'super_admin'],
        icon: 'icon-file',
      },
    },
    {
      path: 'api-resource',
      name: 'ApiResource',
      component: () =>
        import('@/views/administration/openiddict/api-resource/index.vue'),
      meta: {
        locale: 'menu.administration.apiResource',
        requiresAuth: true,
        roles: ['admin', 'super_admin'],
        icon: 'icon-apps',
      },
    },
    {
      path: 'identity-resource',
      name: 'IdentityResource',
      component: () =>
        import('@/views/administration/openiddict/identity-resource/index.vue'),
      meta: {
        locale: 'menu.administration.identityResource',
        requiresAuth: true,
        roles: ['admin', 'super_admin'],
        icon: 'icon-idcard',
      },
    },
    {
      path: 'authorization',
      name: 'Authorization',
      component: () =>
        import('@/views/administration/openiddict/authorization/index.vue'),
      meta: {
        locale: 'menu.administration.authorization',
        requiresAuth: true,
        roles: ['admin', 'super_admin'],
        icon: 'icon-safe',
      },
    },
  ],
};

export default ADMINISTRATION;
