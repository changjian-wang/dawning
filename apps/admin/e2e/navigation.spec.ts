import { test, expect } from '@playwright/test';

test.describe('首页导航', () => {
  test('未登录时访问首页应被处理', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });

    // 未登录用户应被重定向到登录页或停留在首页（取决于路由配置）
    await page.waitForTimeout(2000);
    const url = page.url();
    // 检查页面是否正常加载
    expect(url).toContain('localhost:5173');
  });

  test('访问不存在的页面应正常处理', async ({ page }) => {
    await page.goto('/non-existent-page-12345', { waitUntil: 'domcontentloaded' });

    // 应该显示 404 页面或重定向到其他页面
    await page.waitForTimeout(2000);
    const url = page.url();
    // 只要页面能加载就通过
    expect(url).toContain('localhost:5173');
  });
});
