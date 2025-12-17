import { test, expect } from '@playwright/test';

test.describe('首页导航', () => {
  test('未登录时应重定向到登录页', async ({ page }) => {
    await page.goto('/');

    // 未登录用户应被重定向到登录页
    await expect(page).toHaveURL(/login/);
  });

  test('访问不存在的页面应显示 404', async ({ page }) => {
    await page.goto('/non-existent-page-12345');

    // 应该显示 404 页面或重定向到登录
    const url = page.url();
    const is404OrLogin = url.includes('404') || url.includes('login');
    expect(is404OrLogin).toBeTruthy();
  });
});
