import { mergeConfig } from 'vite';
import baseConfig from './vite.config.base';
import configCompressPlugin from './plugin/compress';
import configVisualizerPlugin from './plugin/visualizer';
import configArcoResolverPlugin from './plugin/arcoResolver';
import configImageminPlugin from './plugin/imagemin';

// 是否使用 CDN（通过环境变量控制，默认不启用以保持兼容性）
const useCdn = process.env.VITE_USE_CDN === 'true';

export default mergeConfig(
  {
    mode: 'production',
    plugins: [
      configCompressPlugin('both'), // 同时启用 gzip 和 brotli 压缩
      configVisualizerPlugin(),
      configArcoResolverPlugin(),
      configImageminPlugin(),
    ],
    build: {
      rollupOptions: {
        output: {
          // 分包策略优化
          manualChunks: {
            // UI 框架
            'arco-design': ['@arco-design/web-vue'],
            // 图表库（较大，单独分包）
            'echarts': ['echarts', 'vue-echarts'],
            // Vue 核心
            'vue-core': ['vue', 'vue-router', 'pinia'],
            // Vue 工具库
            'vue-utils': ['@vueuse/core', 'vue-i18n'],
            // HTTP 客户端
            'http': ['axios'],
            // 工具库
            'utils': ['dayjs', 'nprogress'],
          },
          // 资源文件命名（带 hash）
          chunkFileNames: 'assets/js/[name]-[hash].js',
          entryFileNames: 'assets/js/[name]-[hash].js',
          assetFileNames: (assetInfo) => {
            const info = assetInfo.name?.split('.') || [];
            const ext = info[info.length - 1];
            // 按类型分目录
            if (/\.(png|jpe?g|gif|svg|webp|avif|ico)$/i.test(assetInfo.name || '')) {
              return 'assets/images/[name]-[hash].[ext]';
            }
            if (/\.(woff2?|eot|ttf|otf)$/i.test(assetInfo.name || '')) {
              return 'assets/fonts/[name]-[hash].[ext]';
            }
            if (/\.css$/i.test(assetInfo.name || '')) {
              return 'assets/css/[name]-[hash].[ext]';
            }
            return 'assets/[name]-[hash].[ext]';
          },
        },
      },
      // 分包大小警告阈值
      chunkSizeWarningLimit: 1000,
      // CSS 代码分割
      cssCodeSplit: true,
      // 使用 esbuild 压缩（Vite 默认，比 terser 更快）
      minify: 'esbuild',
      // esbuild 选项：移除 console 和 debugger
      esbuildOptions: {
        drop: ['console', 'debugger'],
        // 保持函数名（便于错误追踪）
        keepNames: true,
      },
      // 静态资源内联阈值 (4KB)
      assetsInlineLimit: 4096,
      // 禁用 sourcemap 以减小体积
      sourcemap: false,
      // 启用 CSS 压缩
      cssMinify: true,
      // 模块预加载
      modulePreload: {
        polyfill: true,
      },
      // 构建目标
      target: ['es2020', 'chrome87', 'safari14', 'firefox78', 'edge88'],
    },
    // esbuild 全局配置
    esbuild: {
      drop: ['console', 'debugger'],
    },
  },
  baseConfig
);
