import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright 配置
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
  testDir: './e2e',
  /* 完全并行运行测试 */
  fullyParallel: true,
  /* 在 CI 上禁止使用 test.only */
  forbidOnly: !!process.env.CI,
  /* 仅在 CI 上重试 */
  retries: process.env.CI ? 2 : 0,
  /* 在 CI 上禁用并行 */
  workers: process.env.CI ? 1 : undefined,
  /* 测试报告 */
  reporter: 'html',
  /* 共享设置 */
  use: {
    /* 基础 URL */
    baseURL: 'http://localhost:5173',
    /* 失败时收集跟踪 */
    trace: 'on-first-retry',
    /* 截图 */
    screenshot: 'only-on-failure',
  },

  /* 配置项目 */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  /* 在测试前启动本地开发服务器 */
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
