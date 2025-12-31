import { DirectiveBinding } from 'vue';
import { useUserStore } from '@/store';

/**
 * v-has-permission directive
 * Usage:
 *   v-has-permission="'permission.code'" - Check single permission
 *   v-has-permission="['permission.code1', 'permission.code2']" - Check any one permission
 *   v-has-permission:all="['permission.code1', 'permission.code2']" - Check all permissions
 */
function checkPermission(el: HTMLElement, binding: DirectiveBinding) {
  const { value, arg } = binding;
  const userStore = useUserStore();

  // Super admin has all permissions
  if (
    userStore.role === 'super_admin' ||
    userStore.roles.includes('super_admin')
  ) {
    return;
  }

  let hasPermission = false;

  if (typeof value === 'string') {
    // Single permission check
    hasPermission = userStore.permissions.includes(value);
  } else if (Array.isArray(value)) {
    if (arg === 'all') {
      // Check all permissions
      hasPermission = value.every((p: string) =>
        userStore.permissions.includes(p)
      );
    } else {
      // Check any one permission
      hasPermission = value.some((p: string) =>
        userStore.permissions.includes(p)
      );
    }
  } else {
    throw new Error(
      `v-has-permission requires a string or array of permission codes`
    );
  }

  if (!hasPermission && el.parentNode) {
    el.parentNode.removeChild(el);
  }
}

export default {
  mounted(el: HTMLElement, binding: DirectiveBinding) {
    checkPermission(el, binding);
  },
  updated(el: HTMLElement, binding: DirectiveBinding) {
    checkPermission(el, binding);
  },
};
