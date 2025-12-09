import type { Router, LocationQueryRaw } from 'vue-router';
import NProgress from 'nprogress'; // progress bar

import { useUserStore } from '@/store';
import { isLogin } from '@/utils/auth';

export default function setupUserLoginInfoGuard(router: Router) {
  router.beforeEach(async (to, from, next) => {
    NProgress.start();
    const userStore = useUserStore();
    if (isLogin()) {
      if (userStore.role) {
        next();
      } else {
        try {
          // 尝试从 token 中恢复用户信息
          const token =
            localStorage.getItem('id_token') ||
            localStorage.getItem('access_token');
          if (token) {
            // 从 JWT 解析用户信息
            const { parseJwtToken } = await import('@/api/auth');
            const userInfo = parseJwtToken(token);
            if (userInfo) {
              userStore.setInfo({
                name: userInfo.name || userInfo.sub,
                email: userInfo.email,
                role: userInfo.role || 'admin',
                accountId: userInfo.sub,
              });
              next();
              return;
            }
          }

          // 如果无法从 token 恢复，尝试获取用户信息
          await userStore.info();
          next();
        } catch (error) {
          await userStore.logout();
          next({
            name: 'login',
            query: {
              redirect: to.name,
              ...to.query,
            } as LocationQueryRaw,
          });
        }
      }
    } else {
      if (to.name === 'login') {
        next();
        return;
      }
      next({
        name: 'login',
        query: {
          redirect: to.name,
          ...to.query,
        } as LocationQueryRaw,
      });
    }
  });
}
