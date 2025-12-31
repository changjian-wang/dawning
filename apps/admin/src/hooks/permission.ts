import { RouteLocationNormalized, RouteRecordRaw } from 'vue-router';
import { useUserStore } from '@/store';

export default function usePermission() {
  const userStore = useUserStore();

  return {
    /**
     * Check if route can be accessed
     * Supports both roles and permissions validation methods
     */
    accessRouter(route: RouteLocationNormalized | RouteRecordRaw) {
      // Routes that don't require auth are allowed directly
      if (!route.meta?.requiresAuth) {
        return true;
      }

      // Super admin has all permissions
      if (
        userStore.role === 'super_admin' ||
        userStore.roles.includes('super_admin')
      ) {
        return true;
      }

      // Check role permissions
      const hasRoleAccess =
        !route.meta?.roles ||
        route.meta?.roles?.includes('*') ||
        route.meta?.roles?.includes(userStore.role);

      // Check permission code (if route defines permissions)
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
     * Find first route with permission to access
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
     * Check if has specified permission
     */
    hasPermission(permission: string): boolean {
      return userStore.hasPermission(permission);
    },

    /**
     * Check if has any of the specified permissions
     */
    hasAnyPermission(permissions: string[]): boolean {
      return userStore.hasAnyPermission(permissions);
    },

    /**
     * Check if has all specified permissions
     */
    hasAllPermissions(permissions: string[]): boolean {
      return userStore.hasAllPermissions(permissions);
    },
  };
}
