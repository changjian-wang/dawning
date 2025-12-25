import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

// 告警管理页面路径 (在监控子菜单下)
const ALERT_PATH = '/administration/monitoring/alert';

test.describe('告警管理 E2E 测试', () => {
  test.beforeEach(async ({ page }) => {
    // 登录为管理员
    const loginSuccess = await login(page, 'admin', 'Admin@123456');
    if (!loginSuccess) {
      test.skip(true, '后端服务未运行或登录失败，跳过测试');
    }
  });

  test.describe('告警规则', () => {
    test('应该正确显示告警管理页面', async ({ page }) => {
      await page.goto(ALERT_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查页面标题或内容存在 (使用.stat-card或.general-card)
      const hasStatCard = await page.locator('.stat-card').first().isVisible().catch(() => false);
      const hasTitle = await page.locator('text=告警').first().isVisible().catch(() => false);
      const hasCard = await page.locator('.general-card').isVisible().catch(() => false);
      
      expect(hasStatCard || hasTitle || hasCard).toBeTruthy();
    });

    test('应该显示统计卡片', async ({ page }) => {
      await page.goto(ALERT_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(3000);

      // 检查是否有统计卡片(.stat-card类)
      const hasStatCard = await page.locator('.stat-card').first().isVisible().catch(() => false);
      const hasStatistic = await page.locator('.arco-statistic').first().isVisible().catch(() => false);
      expect(hasStatCard || hasStatistic).toBeTruthy();
    });

    test('应该有规则管理Tab', async ({ page }) => {
      await page.goto(ALERT_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查是否有规则相关的Tab或内容
      const hasRuleTab = await page.locator('text=规则').first().isVisible().catch(() => false);
      const hasRuleTable = await page.locator('.arco-table').isVisible().catch(() => false);
      
      expect(hasRuleTab || hasRuleTable).toBeTruthy();
    });

    test('应该有新建规则按钮', async ({ page }) => {
      await page.goto(ALERT_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查新建按钮
      const hasCreateButton = await page.locator('button').filter({ hasText: /新建|创建|添加/ }).first().isVisible().catch(() => false);
      expect(hasCreateButton).toBeTruthy();
    });
  });

  test.describe('告警历史', () => {
    test('应该有历史记录Tab', async ({ page }) => {
      await page.goto(ALERT_PATH);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);

      // 检查是否有历史相关的Tab
      const hasHistoryTab = await page.locator('text=历史').first().isVisible().catch(() => false);
      const hasTabs = await page.locator('.arco-tabs-tab').count();
      
      expect(hasHistoryTab || hasTabs > 1).toBeTruthy();
    });
  });
});

test.describe('告警管理权限测试', () => {
  test('普通用户访问告警管理应被限制', async ({ page }) => {
    // 尝试用普通用户登录
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('form', { timeout: 15000 });

    await page.fill('input[type="text"]', 'user');
    await page.fill('input[type="password"]', 'User@123456');
    await page.click('button[type="submit"]');

    await page.waitForTimeout(3000);

    // 尝试直接访问告警管理页面
    await page.goto(ALERT_PATH, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    const url = page.url();
    const isBlocked = url.includes('/login') ||
                      url.includes('/403') ||
                      url.includes('/forbidden') ||
                      !url.includes('/alert');

    expect(isBlocked).toBeTruthy();
  });
});
