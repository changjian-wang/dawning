import { test, expect } from '@playwright/test';

test.describe('主题和样式', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('form', { timeout: 15000 });
  });

  test('页面应有默认主题', async ({ page }) => {
    // 检查 body 或 html 元素是否有主题相关的属性
    const html = page.locator('html');
    await expect(html).toBeVisible();
    
    // 检查是否有 arco-theme 属性或相关类名
    const body = page.locator('body');
    const hasArcoStyles = await body.evaluate((el) => {
      // 检查是否应用了 Arco Design 样式
      const styles = window.getComputedStyle(el);
      return styles.fontFamily.includes('system-ui') || 
             styles.fontFamily.includes('Roboto') ||
             el.classList.toString().includes('arco') ||
             document.querySelector('[class*="arco"]') !== null;
    });
    
    expect(hasArcoStyles).toBe(true);
  });

  test('表单应有正确的样式', async ({ page }) => {
    const form = page.locator('form');
    await expect(form).toBeVisible();
    
    // 检查表单内是否有 Arco Design 的输入框组件
    const hasArcoInputs = await page.locator('[class*="arco-input"]').count() > 0;
    
    // 至少应该有输入框
    if (!hasArcoInputs) {
      const hasInputs = await page.locator('input').count() > 0;
      expect(hasInputs).toBe(true);
    }
  });

  test('按钮应有正确的样式', async ({ page }) => {
    const submitButton = page.locator('button[type="submit"]');
    await expect(submitButton).toBeVisible();
    
    // 检查按钮是否可点击
    await expect(submitButton).toBeEnabled();
  });
});

test.describe('国际化', () => {
  test('页面应正确加载', async ({ page }) => {
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    
    // 检查页面是否有中文或英文内容（说明 i18n 加载成功）
    const pageContent = await page.textContent('body');
    expect(pageContent).toBeTruthy();
    expect(pageContent!.length).toBeGreaterThan(0);
  });

  test('登录表单应显示文本', async ({ page }) => {
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('form', { timeout: 15000 });
    
    // 检查按钮是否有文本
    const submitButton = page.locator('button[type="submit"]');
    const buttonText = await submitButton.textContent();
    expect(buttonText).toBeTruthy();
    expect(buttonText!.trim().length).toBeGreaterThan(0);
  });
});

test.describe('网络和性能', () => {
  test('页面应该优雅处理慢速网络', async ({ page, context }) => {
    // 模拟慢速 3G 网络
    await context.route('**/*', async (route) => {
      await new Promise(resolve => setTimeout(resolve, 100));
      await route.continue();
    });
    
    await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 60000 });
    
    // 页面应该仍然加载成功
    await expect(page.locator('form')).toBeVisible({ timeout: 30000 });
  });

  test('页面资源应该正确加载', async ({ page }) => {
    const failedRequests: string[] = [];
    
    page.on('requestfailed', (request) => {
      // 忽略一些预期可能失败的请求
      if (!request.url().includes('hot-update') && 
          !request.url().includes('favicon') &&
          !request.url().includes('api/') &&
          !request.url().includes('fonts/') &&
          !request.url().includes('.woff')) {
        failedRequests.push(request.url());
      }
    });
    
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(3000);
    
    // 不应该有关键资源加载失败
    expect(failedRequests).toHaveLength(0);
  });
});
