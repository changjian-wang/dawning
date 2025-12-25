import { mergeConfig } from 'vite';
import eslint from 'vite-plugin-eslint';
import baseConfig from './vite.config.base';

export default mergeConfig(
  {
    mode: 'development',
    server: {
      host: '0.0.0.0',
      port: 5173,
      open: true,
      fs: {
        strict: true,
      },
      proxy: {
        '/connect': {
          target: 'http://localhost:5202',
          changeOrigin: true,
          secure: false,
        },
        '/api': {
          target: 'http://localhost:5202',
          changeOrigin: true,
          secure: false,
        },
      },
    },
    plugins: [
      eslint({
        cache: false,
        include: ['src/**/*.ts', 'src/**/*.tsx', 'src/**/*.vue'],
        exclude: ['node_modules'],
      }),
    ],
  },
  baseConfig
);
