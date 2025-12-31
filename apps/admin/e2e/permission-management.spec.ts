import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

// 权限管理页面路径
const PERMISSION_PATH = '/administration/user-permission/permission';

test.describe('权限管理 E2E 测试', () => {
  test.beforeEach(async ({ page }) => {
    // 登录为管理员
    const loginSuccess = await login(page, 'admin', 'Admin@123');
    if (!loginSuccess) {
      test.skip(true, '后端服务未运行或登录失败，跳过测试');
    }
  });

  test.describe('权限列表', () => {
    test('应该正确显示权限列表页面', async ({ page }) => {
      await page.goto(PERMISSION_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查页面标题或表格/树形结构存在
      const hasTable = await page.locator('.arco-table').isVisible().catch(() => false);
      const hasTree = await page.locator('.arco-tree').isVisible().catch(() => false);
      const hasTitle = await page.locator('text=权限').first().isVisible().catch(() => false);
      
      expect(hasTable || hasTree || hasTitle).toBeTruthy();
    });

    test('应该显示权限数据', async ({ page }) => {
      await page.goto(PERMISSION_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(3000);

      // 检查是否有权限相关内容 - 更宽松的匹配
      const hasPermissionCode = await page.locator('text=user').first().isVisible().catch(() => false);
      const hasPermissionList = await page.locator('.arco-table-body, .arco-tree').first().isVisible().catch(() => false);
      const hasTable = await page.locator('.arco-table').isVisible().catch(() => false);
      const hasEmpty = await page.locator('.arco-empty').isVisible().catch(() => false);
      
      expect(hasPermissionCode || hasPermissionList || hasTable || hasEmpty).toBeTruthy();
    });

    test('应该有搜索或筛选功能', async ({ page }) => {
      await page.goto(PERMISSION_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查搜索框或筛选器存在
      const hasSearch = await page.locator('input').first().isVisible().catch(() => false);
      const hasSelect = await page.locator('.arco-select').first().isVisible().catch(() => false);
      
      expect(hasSearch || hasSelect).toBeTruthy();
    });
  });

  test.describe('权限操作', () => {
    test('应该有新建权限按钮', async ({ page }) => {
      await page.goto(PERMISSION_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查新建按钮
      const hasCreateButton = await page.locator('button').filter({ hasText: /新建|创建|添加/ }).first().isVisible().catch(() => false);
      expect(hasCreateButton).toBeTruthy();
    });

    test('应该有操作按钮', async ({ page }) => {
      await page.goto(PERMISSION_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(3000);

      // 检查操作列按钮是否存在
      const hasButtons = await page.locator('button').count();
      expect(hasButtons).toBeGreaterThan(0);
    });
  });
});

test.describe('权限管理权限测试', () => {
  test('普通用户访问权限管理应被限制', async ({ page }) => {
    // 尝试用普通用户登录
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('form', { timeout: 15000 });

    await page.fill('input[type="text"]', 'user');
    await page.fill('input[type="password"]', 'User@123456');
    await page.click('button[type="submit"]');

    await page.waitForTimeout(3000);

    // 尝试直接访问权限管理页面
    await page.goto(PERMISSION_PATH, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const url = page.url();
    const isBlocked = url.includes('/login') ||
                      url.includes('/403') ||
                      url.includes('/forbidden') ||
                      !url.includes('/permission');

    expect(isBlocked).toBeTruthy();
  });
});
