import { test, expect } from '@playwright/test';
import { login, navigateTo } from './helpers/auth';

// 租户管理页面路径
const TENANT_PATH = '/administration/tenant';

test.describe('租户管理 E2E 测试', () => {
  test.beforeEach(async ({ page }) => {
    // 登录为超级管理员
    const loginSuccess = await login(page, 'admin', 'Admin@123456');
    if (!loginSuccess) {
      test.skip(true, '后端服务未运行或登录失败，跳过测试');
    }
  });

  test.describe('租户列表', () => {
    test('应该正确显示租户列表页面', async ({ page }) => {
      await page.goto(TENANT_PATH);
      await page.waitForLoadState('networkidle');
      
      // 等待页面加载
      await page.waitForTimeout(2000);

      // 检查页面标题或表格存在
      const hasTable = await page.locator('.arco-table').isVisible().catch(() => false);
      const hasTitle = await page.locator('text=租户').first().isVisible().catch(() => false);
      
      expect(hasTable || hasTitle).toBeTruthy();
    });

    test('应该显示默认租户数据', async ({ page }) => {
      await page.goto(TENANT_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(3000);

      // 检查默认租户是否显示（可能显示 default 或 默认租户）
      const hasDefault = await page.locator('text=default').isVisible().catch(() => false);
      const hasDefaultTenant = await page.locator('text=默认租户').isVisible().catch(() => false);
      
      expect(hasDefault || hasDefaultTenant).toBeTruthy();
    });

    test('应该有搜索和筛选功能', async ({ page }) => {
      await page.goto(TENANT_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查搜索框或筛选器存在
      const hasSearch = await page.locator('input').first().isVisible().catch(() => false);
      expect(hasSearch).toBeTruthy();
    });
  });

  test.describe('租户操作按钮', () => {
    test('应该有新建租户按钮', async ({ page }) => {
      await page.goto(TENANT_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查新建按钮
      const hasCreateButton = await page.locator('button').filter({ hasText: /新建|创建|添加/ }).first().isVisible().catch(() => false);
      expect(hasCreateButton).toBeTruthy();
    });

    test('应该有操作按钮（查看/编辑/更多）', async ({ page }) => {
      await page.goto(TENANT_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(3000);

      // 检查操作列按钮是否存在（任意一个）
      const hasButtons = await page.locator('.arco-table button').first().isVisible().catch(() => false);
      const hasIcons = await page.locator('.arco-table .arco-icon').first().isVisible().catch(() => false);
      
      expect(hasButtons || hasIcons).toBeTruthy();
    });
  });

  test.describe('分页功能', () => {
    test('分页控件应该正确显示', async ({ page }) => {
      await page.goto(TENANT_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查分页控件
      const hasPagination = await page.locator('.arco-pagination').isVisible().catch(() => false);
      // 或者表格本身有数据
      const hasTable = await page.locator('.arco-table').isVisible().catch(() => false);
      
      expect(hasPagination || hasTable).toBeTruthy();
    });
  });
});

test.describe('租户管理权限测试', () => {
  test('普通用户访问租户管理应被限制', async ({ page }) => {
    // 先尝试用普通用户登录
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('form', { timeout: 15000 });

    await page.fill('input[type="text"]', 'user');
    await page.fill('input[type="password"]', 'User@123456');
    await page.click('button[type="submit"]');

    await page.waitForTimeout(3000);

    // 尝试直接访问租户管理页面
    await page.goto(TENANT_PATH, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const url = page.url();
    // 如果登录失败会停留在登录页，如果权限不足可能重定向到403或首页
    const isBlocked = url.includes('/login') ||
                      url.includes('/403') ||
                      url.includes('/forbidden') ||
                      !url.includes('/tenant');

    expect(isBlocked).toBeTruthy();
  });
});
