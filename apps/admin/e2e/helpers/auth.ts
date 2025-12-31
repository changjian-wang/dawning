import { Page } from '@playwright/test';

/**
 * 登录辅助函数
 * @param page Playwright 页面对象
 * @param username 用户名
 * @param password 密码
 */
export async function login(
  page: Page,
  username: string = 'admin',
  password: string = 'Admin@123'
): Promise<boolean> {
  try {
    // 导航到登录页面
    await page.goto('/login', { waitUntil: 'domcontentloaded' });

    // 等待表单加载
    await page.waitForSelector('form', { timeout: 15000 });

    // 填写凭证
    await page.fill('input[type="text"]', username);
    await page.fill('input[type="password"]', password);

    // 提交表单
    await page.click('button[type="submit"]');

    // 等待登录完成或失败
    await page.waitForTimeout(3000);

    // 检查是否仍在登录页面
    const currentUrl = page.url();
    if (currentUrl.includes('/login')) {
      // 登录可能失败，检查是否有错误消息
      console.log('登录失败或仍在登录页面');
      return false;
    }

    // 登录成功，等待页面完全加载
    await page.waitForLoadState('networkidle');
    return true;
  } catch (error) {
    console.log('登录过程出错:', error);
    return false;
  }
}

/**
 * 检查是否已登录
 * @param page Playwright 页面对象
 */
export async function isLoggedIn(page: Page): Promise<boolean> {
  const url = page.url();
  return !url.includes('/login');
}

/**
 * 登出
 * @param page Playwright 页面对象
 */
export async function logout(page: Page): Promise<void> {
  // 点击用户菜单
  const userMenu = page.locator('.arco-dropdown-trigger').first();
  if (await userMenu.isVisible()) {
    await userMenu.click();
    // 点击登出选项
    await page.click('text=登出');
    await page.waitForURL(/\/login/);
  }
}

/**
 * 导航到指定页面（确保已登录）
 * @param page Playwright 页面对象
 * @param path 目标路径
 */
export async function navigateTo(page: Page, path: string): Promise<void> {
  await page.goto(path, { waitUntil: 'domcontentloaded' });

  // 如果被重定向到登录页，先登录
  if (page.url().includes('/login')) {
    await login(page);
    await page.goto(path, { waitUntil: 'domcontentloaded' });
  }

  await page.waitForLoadState('networkidle');
}
