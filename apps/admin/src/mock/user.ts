import Mock from 'mockjs';
import setupMock, {
  successResponseWrap,
} from '@/utils/setup-mock';

setupMock({
  setup() {
    // Note: /api/user/info, /api/user/login, /api/user/logout use real backend API
    // Only mock /api/user/menu endpoint because backend has not implemented it

    // User's server-side menu
    Mock.mock(new RegExp('/api/user/menu'), () => {
      const menuList = [
        {
          path: '/dashboard',
          name: 'dashboard',
          meta: {
            locale: 'menu.server.dashboard',
            requiresAuth: true,
            icon: 'icon-dashboard',
            order: 1,
          },
          children: [
            {
              path: 'workplace',
              name: 'Workplace',
              meta: {
                locale: 'menu.server.workplace',
                requiresAuth: true,
              },
            },
          ],
        },
      ];
      return successResponseWrap(menuList);
    });
  },
});
