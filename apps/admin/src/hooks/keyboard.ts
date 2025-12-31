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
 * Keyboard shortcuts composable
 * Provides global and local shortcut support
 */
export function useKeyboard(shortcuts: KeyboardShortcut[] = []) {
  const activeShortcuts = ref<KeyboardShortcut[]>([]);
  const showHelpModal = ref(false);

  const handleKeyDown = (event: KeyboardEvent) => {
    // Ignore shortcuts in input elements (except Escape)
    const target = event.target as HTMLElement;
    const isInputElement =
      target.tagName === 'INPUT' ||
      target.tagName === 'TEXTAREA' ||
      target.isContentEditable;

    if (isInputElement && event.key !== 'Escape') {
      return;
    }

    // Find matching shortcut
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
 * Global keyboard shortcuts composable
 * Provides application-level shortcuts
 */
export function useGlobalKeyboard() {
  const router = useRouter();

  const globalShortcuts: KeyboardShortcut[] = [
    {
      key: '?',
      shift: true,
      description: 'Show keyboard shortcuts help',
      action: () => {
        Message.info('Press Shift + ? to view keyboard shortcuts help');
      },
    },
    {
      key: 'h',
      alt: true,
      description: 'Go to Home',
      action: () => {
        router.push('/dashboard/workplace');
      },
    },
    {
      key: 'u',
      alt: true,
      description: 'User Management',
      action: () => {
        router.push('/administration/user');
      },
    },
    {
      key: 'r',
      alt: true,
      description: 'Role Management',
      action: () => {
        router.push('/administration/role');
      },
    },
    {
      key: 'p',
      alt: true,
      description: 'Permission Management',
      action: () => {
        router.push('/administration/permission');
      },
    },
    {
      key: 'l',
      alt: true,
      description: 'Audit Log',
      action: () => {
        router.push('/administration/audit-log');
      },
    },
    {
      key: 's',
      alt: true,
      description: 'System Configuration',
      action: () => {
        router.push('/administration/system-config');
      },
    },
    {
      key: 'Escape',
      description: 'Close modal/Cancel operation',
      action: () => {
        // Handled by individual components
      },
    },
  ];

  return useKeyboard(globalShortcuts);
}

/**
 * Table page keyboard shortcuts composable
 * Provides shortcuts for CRUD operations
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
      description: 'Add new record',
      action: handlers.onAdd,
    });
  }

  if (handlers.onEdit) {
    tableShortcuts.push({
      key: 'e',
      ctrl: true,
      description: 'Edit selected record',
      action: handlers.onEdit,
    });
  }

  if (handlers.onDelete) {
    tableShortcuts.push({
      key: 'Delete',
      description: 'Delete selected record',
      action: handlers.onDelete,
    });
  }

  if (handlers.onSearch) {
    tableShortcuts.push({
      key: 'f',
      ctrl: true,
      description: 'Search/Filter',
      action: handlers.onSearch,
    });
  }

  if (handlers.onRefresh) {
    tableShortcuts.push({
      key: 'r',
      ctrl: true,
      description: 'Refresh data',
      action: handlers.onRefresh,
    });
  }

  if (handlers.onExport) {
    tableShortcuts.push({
      key: 'e',
      ctrl: true,
      shift: true,
      description: 'Export data',
      action: handlers.onExport,
    });
  }

  return useKeyboard(tableShortcuts);
}
