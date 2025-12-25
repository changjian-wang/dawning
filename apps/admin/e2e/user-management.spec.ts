import { test, expect } from '@playwright/test';

test.describe('用户管理页面', () => {
  test.beforeEach(async ({ page }) => {
    // 设置 mock token 以模拟登录状态
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
  });

  test('未授权访问用户列表应重定向到登录', async ({ page }) => {
    // 直接访问用户管理页面
    await page.goto('/administration/user-permission/user', { waitUntil: 'domcontentloaded' });
    
    // 应该被重定向到登录页面或显示未授权
    await page.waitForTimeout(2000);
    const url = page.url();
    
    // 验证被重定向到登录页面
    expect(url).toMatch(/login|unauthorized/);
  });

  test('登录页面应有正确的表单结构', async ({ page }) => {
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    
    // 等待表单加载
    await page.waitForSelector('form', { timeout: 10000 });
    
    // 检查必要的表单元素
    const usernameInput = page.locator('input[type="text"]');
    const passwordInput = page.locator('input[type="password"]');
    const submitButton = page.locator('button[type="submit"]');
    
    await expect(usernameInput.first()).toBeVisible();
    await expect(passwordInput.first()).toBeVisible();
    await expect(submitButton).toBeVisible();
  });
});

test.describe('页面布局', () => {
  test('登录页面应适配移动端', async ({ page }) => {
    // 设置移动端视口
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    
    // 等待表单加载
    await page.waitForSelector('form', { timeout: 10000 });
    
    // 检查表单是否可见
    await expect(page.locator('form')).toBeVisible();
    
    // 检查输入框是否可用
    const inputs = page.locator('input');
    const inputCount = await inputs.count();
    expect(inputCount).toBeGreaterThan(0);
  });

  test('登录页面应适配平板', async ({ page }) => {
    // 设置平板视口
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    
    // 等待表单加载
    await page.waitForSelector('form', { timeout: 10000 });
    
    // 检查表单是否可见
    await expect(page.locator('form')).toBeVisible();
  });

  test('登录页面应适配桌面', async ({ page }) => {
    // 设置桌面视口
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    
    // 等待表单加载
    await page.waitForSelector('form', { timeout: 10000 });
    
    // 检查表单是否可见
    await expect(page.locator('form')).toBeVisible();
  });
});
