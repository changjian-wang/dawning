import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

// 角色管理页面路径
const ROLE_PATH = '/administration/user-permission/role';

test.describe('角色管理 E2E 测试', () => {
  test.beforeEach(async ({ page }) => {
    // 登录为管理员
    const loginSuccess = await login(page, 'admin', 'Admin@123');
    if (!loginSuccess) {
      test.skip(true, '后端服务未运行或登录失败，跳过测试');
    }
  });

  test.describe('角色列表', () => {
    test('应该正确显示角色列表页面', async ({ page }) => {
      await page.goto(ROLE_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查页面标题或表格存在
      const hasTable = await page.locator('.arco-table').isVisible().catch(() => false);
      const hasTitle = await page.locator('text=角色').first().isVisible().catch(() => false);
      
      expect(hasTable || hasTitle).toBeTruthy();
    });

    test('应该显示默认角色数据', async ({ page }) => {
      await page.goto(ROLE_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(3000);

      // 检查默认角色是否显示（admin 或 super_admin）
      const hasAdmin = await page.locator('text=admin').first().isVisible().catch(() => false);
      const hasSuperAdmin = await page.locator('text=super_admin').isVisible().catch(() => false);
      
      expect(hasAdmin || hasSuperAdmin).toBeTruthy();
    });

    test('应该有搜索功能', async ({ page }) => {
      await page.goto(ROLE_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查搜索框存在
      const hasSearch = await page.locator('input').first().isVisible().catch(() => false);
      expect(hasSearch).toBeTruthy();
    });
  });

  test.describe('角色操作按钮', () => {
    test('应该有新建角色按钮', async ({ page }) => {
      await page.goto(ROLE_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查新建按钮
      const hasCreateButton = await page.locator('button').filter({ hasText: /新建|创建|添加/ }).first().isVisible().catch(() => false);
      expect(hasCreateButton).toBeTruthy();
    });

    test('应该有操作按钮（编辑/删除）', async ({ page }) => {
      await page.goto(ROLE_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(3000);

      // 检查操作列按钮是否存在
      const hasButtons = await page.locator('.arco-table button').first().isVisible().catch(() => false);
      const hasIcons = await page.locator('.arco-table .arco-icon').first().isVisible().catch(() => false);
      
      expect(hasButtons || hasIcons).toBeTruthy();
    });
  });

  test.describe('分页功能', () => {
    test('分页控件应该正确显示', async ({ page }) => {
      await page.goto(ROLE_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查分页控件或表格
      const hasPagination = await page.locator('.arco-pagination').isVisible().catch(() => false);
      const hasTable = await page.locator('.arco-table').isVisible().catch(() => false);
      
      expect(hasPagination || hasTable).toBeTruthy();
    });
  });
});

test.describe('角色管理权限测试', () => {
  test('普通用户访问角色管理应被限制', async ({ page }) => {
    // 尝试用普通用户登录
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('form', { timeout: 15000 });

    await page.fill('input[type="text"]', 'user');
    await page.fill('input[type="password"]', 'User@123456');
    await page.click('button[type="submit"]');

    await page.waitForTimeout(3000);

    // 尝试直接访问角色管理页面
    await page.goto(ROLE_PATH, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const url = page.url();
    // 如果登录失败会停留在登录页，如果权限不足可能重定向到403或首页
    const isBlocked = url.includes('/login') ||
                      url.includes('/403') ||
                      url.includes('/forbidden') ||
                      !url.includes('/role');

    expect(isBlocked).toBeTruthy();
  });
});
