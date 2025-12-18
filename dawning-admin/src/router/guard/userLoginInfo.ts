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
          // 注意：角色信息只存在于 access_token 中，id_token 不包含角色
          const accessToken = localStorage.getItem('access_token');
          const idToken = localStorage.getItem('id_token');
          
          if (accessToken) {
            // 从 JWT 解析用户信息
            const { parseJwtToken } = await import('@/api/auth');
            
            // 从 access_token 获取角色信息
            const accessTokenInfo = parseJwtToken(accessToken);
            // 从 id_token 获取用户基本信息（如果有的话）
            const idTokenInfo = idToken ? parseJwtToken(idToken) : null;
            
            if (accessTokenInfo) {
              // 处理角色（可能是单个字符串或数组）- 从 access_token 获取
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

              // 优先使用 id_token 中的用户信息，角色信息从 access_token 获取
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
