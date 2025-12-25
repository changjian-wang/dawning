import Mock from 'mockjs';
import setupMock, {
  successResponseWrap,
} from '@/utils/setup-mock';

setupMock({
  setup() {
    // 注意：/api/user/info, /api/user/login, /api/user/logout 使用真实后端 API
    // 只 mock /api/user/menu 接口，因为后端没有实现这个接口

    // 用户的服务端菜单
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
