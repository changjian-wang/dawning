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
          // Try to restore user info from token
          // Note: Role info only exists in access_token, id_token does not contain roles
          const accessToken = localStorage.getItem('access_token');
          const idToken = localStorage.getItem('id_token');

          if (accessToken) {
            // Parse user info from JWT
            const { parseJwtToken } = await import('@/api/auth');

            // Get role info from access_token
            const accessTokenInfo = parseJwtToken(accessToken);
            // Get user basic info from id_token (if available)
            const idTokenInfo = idToken ? parseJwtToken(idToken) : null;

            if (accessTokenInfo) {
              // Handle roles (may be single string or array) - from access_token
              let roles: string[] = [];
              if (Array.isArray(accessTokenInfo.role)) {
                roles = accessTokenInfo.role;
              } else if (accessTokenInfo.role) {
                roles = [accessTokenInfo.role];
              }
              const primaryRole = (roles[0] || 'user') as
                | ''
                | '*'
                | 'admin'
                | 'user'
                | 'super_admin';

              // Prefer user info from id_token, get role info from access_token
              const userInfo = idTokenInfo || accessTokenInfo;

              userStore.setInfo({
                name: userInfo.name || userInfo.sub,
                email: userInfo.email,
                role: primaryRole,
                roles,
                accountId: userInfo.sub,
              });
              next();
              return;
            }
          }

          // If unable to recover from token, try to get user info
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
