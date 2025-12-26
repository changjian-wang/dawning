<script lang="tsx">
  import {
    defineComponent,
    ref,
    h,
    compile,
    computed,
    resolveComponent,
  } from 'vue';
  import { useI18n } from 'vue-i18n';
  import { useRoute, useRouter, RouteRecordRaw } from 'vue-router';
  import type { RouteMeta } from 'vue-router';
  import { useAppStore } from '@/store';
  import { listenerRouteChange } from '@/utils/route-listener';
  import { openWindow, regexUrl } from '@/utils';
  import useMenuTree from './use-menu-tree';

  export default defineComponent({
    emit: ['collapse'],
    setup() {
      const { t } = useI18n();
      const appStore = useAppStore();
      const router = useRouter();
      const route = useRoute();
      const { menuTree } = useMenuTree();
      const collapsed = computed({
        get() {
          if (appStore.device === 'desktop') return appStore.menuCollapse;
          return false;
        },
        set(value: boolean) {
          appStore.updateSettings({ menuCollapse: value });
        },
      });

      const topMenu = computed(() => appStore.topMenu);
      const openKeys = ref<string[]>([]);
      const selectedKey = ref<string[]>([]);

      const goto = (item: RouteRecordRaw) => {
        // Open external link
        if (regexUrl.test(item.path)) {
          openWindow(item.path);
          selectedKey.value = [item.name as string];
          return;
        }
        // Eliminate external link side effects
        const { hideInMenu, activeMenu } = item.meta as RouteMeta;
        if (route.name === item.name && !hideInMenu && !activeMenu) {
          selectedKey.value = [item.name as string];
          return;
        }
        // Trigger router change
        router.push({
          name: item.name,
        });
      };
      const findMenuOpenKeys = (target: string) => {
        const result: string[] = [];
        let isFind = false;
        const backtrack = (item: RouteRecordRaw, keys: string[]) => {
          if (item.name === target) {
            isFind = true;
            result.push(...keys);
            return;
          }
          if (item.children?.length) {
            item.children.forEach((el) => {
              backtrack(el, [...keys, el.name as string]);
            });
          }
        };
        menuTree.value.forEach((el: RouteRecordRaw) => {
          if (isFind) return; // Performance optimization
          backtrack(el, [el.name as string]);
        });
        return result;
      };
      listenerRouteChange((newRoute) => {
        const { requiresAuth, activeMenu, hideInMenu } = newRoute.meta;
        if (requiresAuth && (!hideInMenu || activeMenu)) {
          const menuOpenKeys = findMenuOpenKeys(
            (activeMenu || newRoute.name) as string
          );

          const keySet = new Set([...menuOpenKeys, ...openKeys.value]);
          openKeys.value = [...keySet];

          selectedKey.value = [
            activeMenu || menuOpenKeys[menuOpenKeys.length - 1],
          ];
        }
      }, true);
      const setCollapse = (val: boolean) => {
        if (appStore.device === 'desktop')
          appStore.updateSettings({ menuCollapse: val });
      };

      const renderSubMenu = () => {
        const ASubMenu = resolveComponent('ASubMenu');
        const AMenuItem = resolveComponent('AMenuItem');

        function travel(_route: RouteRecordRaw[]) {
          const nodes: any[] = [];
          if (_route) {
            _route.forEach((element) => {
              // This is demo, modify nodes as needed
              const icon = element?.meta?.icon
                ? () => h(compile(`<${element?.meta?.icon}/>`)) as any
                : null;
              const menuTitle = t(element?.meta?.locale || '');
              const node =
                element?.children && element?.children.length !== 0
                  ? h(
                      ASubMenu,
                      {
                        key: element?.name,
                      },
                      {
                        default: () => travel(element?.children || []),
                        title: () => h('span', null, menuTitle),
                        icon,
                      }
                    )
                  : h(
                      AMenuItem,
                      {
                        key: element?.name,
                        onClick: () => goto(element),
                      },
                      {
                        default: () => h('span', null, menuTitle),
                        icon,
                      }
                    );
              nodes.push(node);
            });
          }
          return nodes;
        }
        return travel(menuTree.value);
      };

      return () => {
        const AMenu = resolveComponent('AMenu');
        const menu = h(
          AMenu,
          {
            'mode': topMenu.value ? 'horizontal' : 'vertical',
            'collapsed': collapsed.value,
            'onUpdate:collapsed': (val: boolean) => {
              collapsed.value = val;
            },
            'openKeys': openKeys.value,
            'onUpdate:openKeys': (val: string[]) => {
              openKeys.value = val;
            },
            'showCollapseButton': appStore.device !== 'mobile',
            'autoOpen': false,
            'selectedKeys': selectedKey.value,
            'autoOpenSelected': true,
            'levelIndent': 34,
            'style': 'height: 100%;width:100%;',
            'onCollapse': setCollapse,
          },
          renderSubMenu()
        );
        return menu;
      };
    },
  });
</script>

<style lang="less">
  .arco-menu-inner {
    .arco-menu-inline-header {
      display: flex;
      align-items: center;
    }
    .arco-icon {
      &:not(.arco-icon-down) {
        font-size: 18px;
      }
    }

    // 确保所有层级的菜单项都显示图标
    .arco-menu-item,
    .arco-menu-inline-header {
      .arco-menu-icon {
        display: inline-flex !important;
        align-items: center;
        justify-content: center;
        margin-right: 10px;
        color: var(--color-text-2);

        .arco-icon {
          font-size: 16px;
        }
      }
    }

    // 子菜单项的图标样式
    .arco-menu-inline-content {
      .arco-menu-item {
        .arco-menu-icon {
          margin-right: 8px;

          .arco-icon {
            font-size: 14px;
          }
        }
      }
    }

    // 选中状态的图标颜色
    .arco-menu-item.arco-menu-selected,
    .arco-menu-inline-header.arco-menu-selected {
      .arco-menu-icon {
        color: rgb(var(--primary-6));
      }
    }
  }

  // 菜单收起时的样式优化
  .arco-menu-collapsed {
    width: 48px;

    // 收起时的一级菜单项（包含子菜单的）
    .arco-menu-pop {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100%;
      padding: 0 !important;
      height: 40px;
      cursor: pointer;

      .arco-menu-icon {
        margin-right: 0 !important;

        .arco-icon {
          font-size: 18px;
          color: var(--color-text-2);
        }
      }

      // 隐藏文字和箭头
      .arco-menu-title,
      .arco-menu-icon-suffix {
        display: none !important;
      }

      &:hover {
        background-color: var(--color-fill-2);

        .arco-menu-icon .arco-icon {
          color: rgb(var(--primary-6));
        }
      }
    }

    // 收起时的单独菜单项（无子菜单）
    .arco-menu-item {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 0 !important;
      height: 40px;

      .arco-menu-icon {
        margin-right: 0 !important;
      }

      .arco-menu-title {
        display: none !important;
      }
    }
  }
</style>

<!-- 弹出菜单样式（需要全局样式，挂载在 body 上） -->
<style lang="less">
  // 收起时弹出的子菜单样式
  .arco-trigger-popup {
    .arco-menu {
      min-width: 180px;
      padding: 4px 0;
      border-radius: 4px;
      box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);

      .arco-menu-item {
        display: flex;
        align-items: center;
        padding: 0 16px;
        height: 36px;

        .arco-menu-icon {
          display: inline-flex !important;
          align-items: center;
          margin-right: 10px;

          .arco-icon {
            font-size: 14px;
            color: var(--color-text-2);
          }
        }

        &:hover {
          background-color: var(--color-fill-2);
        }

        &.arco-menu-selected {
          background-color: var(--color-primary-light-1);

          .arco-menu-icon .arco-icon {
            color: rgb(var(--primary-6));
          }
        }
      }

      // 弹出菜单内的子菜单标题
      .arco-menu-inline-header {
        display: flex;
        align-items: center;

        .arco-menu-icon {
          display: inline-flex !important;
          margin-right: 10px;

          .arco-icon {
            font-size: 14px;
          }
        }
      }
    }
  }
</style>
