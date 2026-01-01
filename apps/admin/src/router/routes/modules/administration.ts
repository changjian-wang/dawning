import { DEFAULT_LAYOUT } from '../base';
import { AppRouteRecordRaw } from '../types';

// ========== User Permission Management ==========
export const USER_PERMISSION: AppRouteRecordRaw = {
  path: '/user-permission',
  name: 'UserPermission',
  component: DEFAULT_LAYOUT,
  meta: {
    locale: 'menu.administration.userPermission',
    requiresAuth: true,
    icon: 'icon-user-group',
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
        icon: 'icon-user',
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
        icon: 'icon-idcard',
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
        icon: 'icon-safe',
        roles: ['*'],
      },
    },
  ],
};

// ========== OpenIddict Authentication ==========
export const OPENIDDICT: AppRouteRecordRaw = {
  path: '/openiddict',
  name: 'OpenIddict',
  component: DEFAULT_LAYOUT,
  meta: {
    locale: 'menu.administration.openiddict',
    requiresAuth: true,
    icon: 'icon-lock',
    order: 2,
  },
  children: [
    {
      path: 'application',
      name: 'Application',
      component: () =>
        import('@/views/administration/openiddict/application/index.vue'),
      meta: {
        locale: 'menu.administration.application',
        requiresAuth: true,
        icon: 'icon-apps',
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
        icon: 'icon-bookmark',
        roles: ['*'],
      },
    },
    {
      path: 'api-resource',
      name: 'ApiResource',
      component: () =>
        import('@/views/administration/openiddict/api-resource/index.vue'),
      meta: {
        locale: 'menu.administration.openiddict.api-resource',
        requiresAuth: true,
        icon: 'icon-code-block',
        roles: ['admin', 'super_admin'],
      },
    },
    {
      path: 'identity-resource',
      name: 'IdentityResource',
      component: () =>
        import('@/views/administration/openiddict/identity-resource/index.vue'),
      meta: {
        locale: 'menu.administration.openiddict.identity-resource',
        requiresAuth: true,
        icon: 'icon-relation',
        roles: ['admin', 'super_admin'],
      },
    },
    {
      path: 'authorization',
      name: 'Authorization',
      component: () =>
        import('@/views/administration/openiddict/authorization/index.vue'),
      meta: {
        locale: 'menu.administration.openiddict.authorization',
        requiresAuth: true,
        icon: 'icon-stamp',
        roles: ['admin', 'super_admin'],
      },
    },
  ],
};

// ========== Gateway Management ==========
export const GATEWAY: AppRouteRecordRaw = {
  path: '/gateway',
  name: 'Gateway',
  component: DEFAULT_LAYOUT,
  meta: {
    locale: 'menu.gateway',
    requiresAuth: true,
    icon: 'icon-cloud',
    order: 3,
  },
  children: [
    {
      path: 'cluster',
      name: 'GatewayCluster',
      component: () =>
        import('@/views/administration/gateway/cluster/index.vue'),
      meta: {
        locale: 'menu.gateway.cluster',
        requiresAuth: true,
        icon: 'icon-computer',
        roles: ['admin', 'super_admin'],
      },
    },
    {
      path: 'route',
      name: 'GatewayRoute',
      component: () => import('@/views/administration/gateway/route/index.vue'),
      meta: {
        locale: 'menu.gateway.route',
        requiresAuth: true,
        icon: 'icon-branch',
        roles: ['admin', 'super_admin'],
      },
    },
    {
      path: 'rate-limit',
      name: 'RateLimit',
      component: () =>
        import('@/views/administration/gateway/rate-limit/index.vue'),
      meta: {
        locale: 'menu.gateway.rateLimit',
        requiresAuth: true,
        icon: 'icon-stop',
        roles: ['admin', 'super_admin'],
      },
    },
    {
      path: 'monitor',
      name: 'GatewayMonitor',
      component: () =>
        import('@/views/administration/gateway/monitor/index.vue'),
      meta: {
        locale: 'menu.gateway.monitor',
        requiresAuth: true,
        icon: 'icon-dashboard',
        roles: ['admin', 'super_admin', 'auditor'],
      },
    },
    {
      path: 'request-log',
      name: 'GatewayRequestLog',
      component: () =>
        import('@/views/administration/gateway/request-log/index.vue'),
      meta: {
        locale: 'menu.gateway.requestLog',
        requiresAuth: true,
        icon: 'icon-file',
        roles: ['admin', 'super_admin', 'auditor'],
      },
    },
  ],
};

// ========== Monitoring and Alerting ==========
export const MONITORING: AppRouteRecordRaw = {
  path: '/monitoring',
  name: 'Monitoring',
  component: DEFAULT_LAYOUT,
  meta: {
    locale: 'menu.administration.monitoring',
    requiresAuth: true,
    icon: 'icon-dashboard',
    order: 4,
  },
  children: [
    {
      path: 'system-monitor',
      name: 'SystemMonitor',
      component: () =>
        import('@/views/administration/system-monitor/index.vue'),
      meta: {
        locale: 'menu.administration.systemMonitor',
        requiresAuth: true,
        icon: 'icon-desktop',
        roles: ['admin', 'super_admin'],
      },
    },
    {
      path: 'alert',
      name: 'AlertManagement',
      component: () => import('@/views/administration/alert/index.vue'),
      meta: {
        locale: 'menu.administration.alertManagement',
        requiresAuth: true,
        icon: 'icon-notification',
        roles: ['admin', 'super_admin'],
      },
    },
    {
      path: 'audit-log',
      name: 'AuditLog',
      component: () => import('@/views/administration/audit-log/index.vue'),
      meta: {
        locale: 'menu.administration.auditLog',
        requiresAuth: true,
        icon: 'icon-book',
        roles: ['admin', 'super_admin', 'auditor'],
      },
    },
    {
      path: 'system-log',
      name: 'SystemLog',
      component: () => import('@/views/administration/system-log/index.vue'),
      meta: {
        locale: 'menu.administration.systemLog',
        requiresAuth: true,
        icon: 'icon-file',
        roles: ['admin', 'super_admin'],
      },
    },
  ],
};

// ========== Multi-tenant Management ==========
export const TENANT: AppRouteRecordRaw = {
  path: '/tenant',
  name: 'Tenant',
  component: DEFAULT_LAYOUT,
  redirect: '/tenant/index',
  meta: {
    locale: 'menu.administration.tenant',
    requiresAuth: true,
    icon: 'icon-apps',
    order: 5,
    hideChildrenInMenu: true,
  },
  children: [
    {
      path: 'index',
      name: 'TenantManagement',
      component: () => import('@/views/administration/tenant/index.vue'),
      meta: {
        locale: 'menu.administration.tenant',
        requiresAuth: true,
        icon: 'icon-apps',
        roles: ['super_admin'],
        activeMenu: 'Tenant',
      },
    },
  ],
};

// ========== System Settings ==========
export const SETTINGS: AppRouteRecordRaw = {
  path: '/settings',
  name: 'Settings',
  component: DEFAULT_LAYOUT,
  meta: {
    locale: 'menu.administration.settings',
    requiresAuth: true,
    icon: 'icon-tool',
    order: 6,
  },
  children: [
    {
      path: 'claim-type',
      name: 'ClaimType',
      component: () => import('@/views/administration/claim-type/index.vue'),
      meta: {
        locale: 'menu.administration.claim.type',
        requiresAuth: true,
        icon: 'icon-tags',
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
      path: 'system-config',
      name: 'SystemConfig',
      component: () => import('@/views/administration/system-config/index.vue'),
      meta: {
        locale: 'menu.systemConfig',
        requiresAuth: true,
        icon: 'icon-settings',
        roles: ['admin', 'super_admin'],
      },
    },
  ],
};

// Export all routes
export default [
  USER_PERMISSION,
  OPENIDDICT,
  GATEWAY,
  MONITORING,
  TENANT,
  SETTINGS,
];
