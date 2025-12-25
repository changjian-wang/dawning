/**
 * CDN 外部化配置
 * 将大型依赖包从 CDN 加载，减少打包体积
 *
 * 使用说明：
 * 1. 生产环境自动启用 CDN
 * 2. 开发环境不使用 CDN（便于调试）
 * 3. 可通过环境变量 VITE_USE_CDN=false 禁用
 */
import type { Plugin } from 'vite';
import type { ExternalOption } from 'rollup';

// CDN 基础 URL
const CDN_BASE = {
  jsdelivr: 'https://cdn.jsdelivr.net/npm',
  unpkg: 'https://unpkg.com',
  cdnjs: 'https://cdnjs.cloudflare.com/ajax/libs',
};

// CDN 配置列表
interface CdnConfig {
  name: string; // 包名
  var: string; // 全局变量名
  path: string; // CDN 路径
  css?: string; // CSS 路径（可选）
}

// 默认使用 jsdelivr
const CDN_PROVIDER = CDN_BASE.jsdelivr;

// CDN 依赖配置
export const cdnModules: CdnConfig[] = [
  {
    name: 'vue',
    var: 'Vue',
    path: `${CDN_PROVIDER}/vue@3.4.21/dist/vue.global.prod.js`,
  },
  {
    name: 'vue-router',
    var: 'VueRouter',
    path: `${CDN_PROVIDER}/vue-router@4.3.0/dist/vue-router.global.prod.js`,
  },
  {
    name: 'pinia',
    var: 'Pinia',
    path: `${CDN_PROVIDER}/pinia@2.1.7/dist/pinia.iife.prod.js`,
  },
  {
    name: 'axios',
    var: 'axios',
    path: `${CDN_PROVIDER}/axios@1.6.8/dist/axios.min.js`,
  },
  {
    name: 'dayjs',
    var: 'dayjs',
    path: `${CDN_PROVIDER}/dayjs@1.11.10/dayjs.min.js`,
  },
];

/**
 * 生成 CDN 脚本标签
 */
export function generateCdnScripts(modules: CdnConfig[]): string {
  return modules
    .map(
      (m) =>
        `<script src="${m.path}" crossorigin="anonymous"></script>`
    )
    .join('\n    ');
}

/**
 * 生成 CDN 预加载标签
 */
export function generateCdnPreloads(modules: CdnConfig[]): string {
  return modules
    .map(
      (m) =>
        `<link rel="preload" href="${m.path}" as="script" crossorigin="anonymous">`
    )
    .join('\n    ');
}

/**
 * 生成 Rollup external 配置
 */
export function generateExternals(modules: CdnConfig[]): ExternalOption {
  return modules.map((m) => m.name);
}

/**
 * 生成 Rollup globals 配置
 */
export function generateGlobals(
  modules: CdnConfig[]
): Record<string, string> {
  return modules.reduce(
    (acc, m) => {
      acc[m.name] = m.var;
      return acc;
    },
    {} as Record<string, string>
  );
}

/**
 * Vite CDN 插件
 * 在 HTML 中注入 CDN 脚本
 */
export default function configCdnPlugin(
  useCdn: boolean = true
): Plugin | null {
  if (!useCdn) {
    return null;
  }

  return {
    name: 'vite-plugin-cdn',
    enforce: 'post',
    transformIndexHtml(html) {
      const preloads = generateCdnPreloads(cdnModules);
      const scripts = generateCdnScripts(cdnModules);

      // 在 </head> 前插入预加载
      html = html.replace('</head>', `    ${preloads}\n  </head>`);

      // 在 <script type="module"> 前插入 CDN 脚本
      html = html.replace(
        '<script type="module"',
        `${scripts}\n    <script type="module"`
      );

      return html;
    },
  };
}

/**
 * 获取 Rollup 配置（用于生产构建）
 */
export function getCdnRollupOptions(useCdn: boolean) {
  if (!useCdn) {
    return {};
  }

  return {
    external: generateExternals(cdnModules),
    output: {
      globals: generateGlobals(cdnModules),
    },
  };
}
