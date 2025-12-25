import { test, expect } from '@playwright/test';

test.describe('页面可访问性', () => {
  test('登录页面应该快速加载', async ({ page }) => {
    const startTime = Date.now();
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    const loadTime = Date.now() - startTime;

    // 页面应在 30 秒内加载完成（考虑首次冷启动）
    expect(loadTime).toBeLessThan(30000);
  });

  test('页面应有正确的视口', async ({ page }) => {
    await page.goto('/login', { waitUntil: 'domcontentloaded' });

    // 检查视口大小
    const viewport = page.viewportSize();
    expect(viewport?.width).toBeGreaterThan(0);
    expect(viewport?.height).toBeGreaterThan(0);
  });

  test('页面不应有 JavaScript 错误', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', (error) => {
      errors.push(error.message);
    });

    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    // 页面不应有严重的 JavaScript 错误
    const criticalErrors = errors.filter(
      (e) => !e.includes('ResizeObserver') && !e.includes('network')
    );
    expect(criticalErrors).toHaveLength(0);
  });
});
