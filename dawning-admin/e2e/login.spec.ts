import { test, expect } from '@playwright/test';

test.describe('登录页面', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('应该显示登录表单', async ({ page }) => {
    // 检查页面标题或登录表单元素
    await expect(page.locator('form')).toBeVisible();
  });

  test('空表单提交应显示验证错误', async ({ page }) => {
    // 点击登录按钮
    await page.click('button[type="submit"]');

    // 应该显示验证错误信息
    await expect(page.locator('.arco-form-item-message')).toBeVisible();
  });

  test('错误凭证应显示错误提示', async ({ page }) => {
    // 填写错误的凭证
    await page.fill('input[type="text"]', 'wronguser');
    await page.fill('input[type="password"]', 'wrongpassword');

    // 提交表单
    await page.click('button[type="submit"]');

    // 应该显示错误消息
    await page.waitForTimeout(2000);
    // 检查是否仍在登录页面（登录失败）
    await expect(page).toHaveURL(/login/);
  });
});
