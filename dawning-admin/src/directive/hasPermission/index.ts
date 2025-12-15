import { DirectiveBinding } from 'vue';
import { useUserStore } from '@/store';

/**
 * v-has-permission 指令
 * 用法：
 *   v-has-permission="'permission.code'" - 检查单个权限
 *   v-has-permission="['permission.code1', 'permission.code2']" - 检查任意一个权限
 *   v-has-permission:all="['permission.code1', 'permission.code2']" - 检查所有权限
 */
function checkPermission(el: HTMLElement, binding: DirectiveBinding) {
  const { value, arg } = binding;
  const userStore = useUserStore();

  // 超级管理员拥有所有权限
  if (
    userStore.role === 'super_admin' ||
    userStore.roles.includes('super_admin')
  ) {
    return;
  }

  let hasPermission = false;

  if (typeof value === 'string') {
    // 单个权限检查
    hasPermission = userStore.permissions.includes(value);
  } else if (Array.isArray(value)) {
    if (arg === 'all') {
      // 检查所有权限
      hasPermission = value.every((p: string) =>
        userStore.permissions.includes(p)
      );
    } else {
      // 检查任意一个权限
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
