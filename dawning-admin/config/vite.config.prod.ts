import { mergeConfig } from 'vite';
import baseConfig from './vite.config.base';
import configCompressPlugin from './plugin/compress';
import configVisualizerPlugin from './plugin/visualizer';
import configArcoResolverPlugin from './plugin/arcoResolver';
import configImageminPlugin from './plugin/imagemin';

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
          manualChunks: {
            arco: ['@arco-design/web-vue'],
            chart: ['echarts', 'vue-echarts'],
            vue: ['vue', 'vue-router', 'pinia', '@vueuse/core', 'vue-i18n'],
            axios: ['axios'],
          },
        },
      },
      chunkSizeWarningLimit: 2000,
      // CSS 代码分割
      cssCodeSplit: true,
      // 使用 esbuild 压缩（Vite 默认，比 terser 更快）
      minify: 'esbuild',
      // esbuild 选项：移除 console 和 debugger
      esbuildOptions: {
        drop: ['console', 'debugger'],
      },
      // 静态资源内联阈值 (4KB)
      assetsInlineLimit: 4096,
      // 禁用 sourcemap 以减小体积
      sourcemap: false,
    },
    // esbuild 全局配置
    esbuild: {
      drop: ['console', 'debugger'],
    },
  },
  baseConfig
);
