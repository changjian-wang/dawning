import { onMounted, onUnmounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { Message } from '@arco-design/web-vue';

export interface KeyboardShortcut {
  key: string;
  ctrl?: boolean;
  alt?: boolean;
  shift?: boolean;
  meta?: boolean;
  description: string;
  action: () => void;
}

/**
 * 键盘快捷键组合式函数
 * 提供全局和局部快捷键支持
 */
export function useKeyboard(shortcuts: KeyboardShortcut[] = []) {
  const activeShortcuts = ref<KeyboardShortcut[]>([]);
  const showHelpModal = ref(false);

  const handleKeyDown = (event: KeyboardEvent) => {
    // 忽略输入框中的快捷键（除了 Escape）
    const target = event.target as HTMLElement;
    const isInputElement =
      target.tagName === 'INPUT' ||
      target.tagName === 'TEXTAREA' ||
      target.isContentEditable;

    if (isInputElement && event.key !== 'Escape') {
      return;
    }

    // 查找匹配的快捷键
    const matchedShortcut = activeShortcuts.value.find((shortcut) => {
      const keyMatch = shortcut.key.toLowerCase() === event.key.toLowerCase();
      const ctrlMatch = !!shortcut.ctrl === (event.ctrlKey || event.metaKey);
      const altMatch = !!shortcut.alt === event.altKey;
      const shiftMatch = !!shortcut.shift === event.shiftKey;

      return keyMatch && ctrlMatch && altMatch && shiftMatch;
    });

    if (matchedShortcut) {
      event.preventDefault();
      event.stopPropagation();
      matchedShortcut.action();
    }
  };

  const registerShortcuts = (newShortcuts: KeyboardShortcut[]) => {
    activeShortcuts.value = [...activeShortcuts.value, ...newShortcuts];
  };

  const unregisterShortcuts = (shortcutsToRemove: KeyboardShortcut[]) => {
    activeShortcuts.value = activeShortcuts.value.filter(
      (s) =>
        !shortcutsToRemove.some(
          (r) =>
            r.key === s.key &&
            r.ctrl === s.ctrl &&
            r.alt === s.alt &&
            r.shift === s.shift
        )
    );
  };

  const getShortcutLabel = (shortcut: KeyboardShortcut): string => {
    const parts: string[] = [];
    if (shortcut.ctrl) parts.push('Ctrl');
    if (shortcut.alt) parts.push('Alt');
    if (shortcut.shift) parts.push('Shift');
    if (shortcut.meta) parts.push('⌘');
    parts.push(shortcut.key.toUpperCase());
    return parts.join(' + ');
  };

  onMounted(() => {
    activeShortcuts.value = shortcuts;
    window.addEventListener('keydown', handleKeyDown);
  });

  onUnmounted(() => {
    window.removeEventListener('keydown', handleKeyDown);
  });

  return {
    activeShortcuts,
    showHelpModal,
    registerShortcuts,
    unregisterShortcuts,
    getShortcutLabel,
  };
}

/**
 * 全局快捷键组合式函数
 * 提供应用级别的快捷键
 */
export function useGlobalKeyboard() {
  const router = useRouter();

  const globalShortcuts: KeyboardShortcut[] = [
    {
      key: '?',
      shift: true,
      description: '显示快捷键帮助',
      action: () => {
        Message.info('按 Shift + ? 查看快捷键帮助');
      },
    },
    {
      key: 'h',
      alt: true,
      description: '返回首页',
      action: () => {
        router.push('/dashboard/workplace');
      },
    },
    {
      key: 'u',
      alt: true,
      description: '用户管理',
      action: () => {
        router.push('/administration/user');
      },
    },
    {
      key: 'r',
      alt: true,
      description: '角色管理',
      action: () => {
        router.push('/administration/role');
      },
    },
    {
      key: 'p',
      alt: true,
      description: '权限管理',
      action: () => {
        router.push('/administration/permission');
      },
    },
    {
      key: 'l',
      alt: true,
      description: '审计日志',
      action: () => {
        router.push('/administration/audit-log');
      },
    },
    {
      key: 's',
      alt: true,
      description: '系统配置',
      action: () => {
        router.push('/administration/system-config');
      },
    },
    {
      key: 'Escape',
      description: '关闭弹窗/取消操作',
      action: () => {
        // 由各组件自行处理
      },
    },
  ];

  return useKeyboard(globalShortcuts);
}

/**
 * 表格页面快捷键组合式函数
 * 提供 CRUD 操作的快捷键
 */
export function useTableKeyboard(handlers: {
  onAdd?: () => void;
  onEdit?: () => void;
  onDelete?: () => void;
  onSearch?: () => void;
  onRefresh?: () => void;
  onExport?: () => void;
}) {
  const tableShortcuts: KeyboardShortcut[] = [];

  if (handlers.onAdd) {
    tableShortcuts.push({
      key: 'n',
      ctrl: true,
      description: '新增记录',
      action: handlers.onAdd,
    });
  }

  if (handlers.onEdit) {
    tableShortcuts.push({
      key: 'e',
      ctrl: true,
      description: '编辑选中记录',
      action: handlers.onEdit,
    });
  }

  if (handlers.onDelete) {
    tableShortcuts.push({
      key: 'Delete',
      description: '删除选中记录',
      action: handlers.onDelete,
    });
  }

  if (handlers.onSearch) {
    tableShortcuts.push({
      key: 'f',
      ctrl: true,
      description: '搜索/筛选',
      action: handlers.onSearch,
    });
  }

  if (handlers.onRefresh) {
    tableShortcuts.push({
      key: 'r',
      ctrl: true,
      description: '刷新数据',
      action: handlers.onRefresh,
    });
  }

  if (handlers.onExport) {
    tableShortcuts.push({
      key: 'e',
      ctrl: true,
      shift: true,
      description: '导出数据',
      action: handlers.onExport,
    });
  }

  return useKeyboard(tableShortcuts);
}
