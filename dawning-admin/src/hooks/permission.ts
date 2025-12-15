import { RouteLocationNormalized, RouteRecordRaw } from 'vue-router';
import { useUserStore } from '@/store';

export default function usePermission() {
  const userStore = useUserStore();

  return {
    /**
     * 检查是否可以访问路由
     * 支持 roles 和 permissions 两种验证方式
     */
    accessRouter(route: RouteLocationNormalized | RouteRecordRaw) {
      // 不需要认证的路由直接放行
      if (!route.meta?.requiresAuth) {
        return true;
      }

      // 超级管理员拥有所有权限
      if (
        userStore.role === 'super_admin' ||
        userStore.roles.includes('super_admin')
      ) {
        return true;
      }

      // 检查角色权限
      const hasRoleAccess =
        !route.meta?.roles ||
        route.meta?.roles?.includes('*') ||
        route.meta?.roles?.includes(userStore.role);

      // 检查权限码（如果路由定义了 permissions）
      const requiredPermissions = route.meta?.permissions as
        | string[]
        | undefined;
      const hasPermissionAccess =
        !requiredPermissions ||
        requiredPermissions.length === 0 ||
        requiredPermissions.some((p: string) =>
          userStore.permissions.includes(p)
        );

      return hasRoleAccess && hasPermissionAccess;
    },

    /**
     * 查找第一个有权限访问的路由
     */
    findFirstPermissionRoute(_routers: any, role = 'admin') {
      const cloneRouters = [..._routers];
      while (cloneRouters.length) {
        const firstElement = cloneRouters.shift();
        if (
          firstElement?.meta?.roles?.find((el: string[]) => {
            return el.includes('*') || el.includes(role);
          })
        )
          return { name: firstElement.name };
        if (firstElement?.children) {
          cloneRouters.push(...firstElement.children);
        }
      }
      return null;
    },

    /**
     * 检查是否拥有指定权限
     */
    hasPermission(permission: string): boolean {
      return userStore.hasPermission(permission);
    },

    /**
     * 检查是否拥有任意一个权限
     */
    hasAnyPermission(permissions: string[]): boolean {
      return userStore.hasAnyPermission(permissions);
    },

    /**
     * 检查是否拥有所有权限
     */
    hasAllPermissions(permissions: string[]): boolean {
      return userStore.hasAllPermissions(permissions);
    },
  };
}
