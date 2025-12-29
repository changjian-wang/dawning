<template>
  <a-card
    class="general-card"
    :header-style="{ paddingBottom: '0' }"
    :body-style="{ padding: '16px' }"
  >
    <template #title>
      {{ $t('workplace.quickOperation') }}
    </template>
    <div class="operation-grid">
      <div
        v-for="item in operations"
        :key="item.key"
        class="operation-item"
        @click="handleClick(item)"
      >
        <div class="operation-icon" :style="{ background: item.bgColor }">
          <component :is="item.icon" :style="{ color: item.iconColor }" />
        </div>
        <span class="operation-label">{{ $t(item.label) }}</span>
      </div>
    </div>
  </a-card>
</template>

<script lang="ts" setup>
  import { useRouter } from 'vue-router';
  import {
    IconUser,
    IconUserGroup,
    IconApps,
    IconFile,
    IconSettings,
    IconSafe,
  } from '@arco-design/web-vue/es/icon';

  interface OperationItem {
    key: string;
    label: string;
    icon: typeof IconUser;
    bgColor: string;
    iconColor: string;
    route: string;
  }

  const router = useRouter();

  const operations: OperationItem[] = [
    {
      key: 'users',
      label: 'workplace.op.manageUsers',
      icon: IconUser,
      bgColor: 'rgba(22, 93, 255, 0.1)',
      iconColor: '#165DFF',
      route: '/administration/user-permission/user',
    },
    {
      key: 'roles',
      label: 'workplace.op.manageRoles',
      icon: IconUserGroup,
      bgColor: 'rgba(114, 46, 209, 0.1)',
      iconColor: '#722ED1',
      route: '/administration/user-permission/role',
    },
    {
      key: 'apps',
      label: 'workplace.op.manageApps',
      icon: IconApps,
      bgColor: 'rgba(15, 198, 194, 0.1)',
      iconColor: '#0FC6C2',
      route: '/administration/openiddict/application',
    },
    {
      key: 'logs',
      label: 'workplace.op.viewLogs',
      icon: IconFile,
      bgColor: 'rgba(247, 114, 52, 0.1)',
      iconColor: '#F77234',
      route: '/administration/monitoring/audit-log',
    },
    {
      key: 'routes',
      label: 'workplace.op.gatewayRoutes',
      icon: IconSettings,
      bgColor: 'rgba(0, 180, 42, 0.1)',
      iconColor: '#00B42A',
      route: '/administration/gateway/route',
    },
    {
      key: 'rateLimit',
      label: 'workplace.op.rateLimit',
      icon: IconSafe,
      bgColor: 'rgba(245, 63, 63, 0.1)',
      iconColor: '#F53F3F',
      route: '/administration/gateway/rate-limit',
    },
  ];

  const handleClick = (item: OperationItem) => {
    router.push(item.route);
  };
</script>

<style scoped lang="less">
  .general-card {
    border-radius: 4px;
  }

  .operation-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 8px;
  }

  .operation-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 12px 4px;
    background: var(--color-fill-1);
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s;
    min-width: 0;
    overflow: hidden;

    &:hover {
      background: var(--color-fill-2);
      transform: translateY(-2px);

      .operation-icon {
        transform: scale(1.1);
      }
    }
  }

  .operation-icon {
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 10px;
    font-size: 20px;
    margin-bottom: 6px;
    transition: transform 0.2s;
    flex-shrink: 0;
  }

  .operation-label {
    font-size: 11px;
    color: var(--color-text-2);
    text-align: center;
    word-break: break-word;
    line-height: 1.3;
    max-width: 100%;
  }
</style>
