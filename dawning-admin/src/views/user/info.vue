<template>
  <div class="container">
    <a-card class="general-card" :title="$t('userCenter.title')">
      <div class="user-info-wrapper">
        <!-- 头像区域 -->
        <div class="avatar-section">
          <a-avatar :size="100" :style="{ backgroundColor: '#165dff' }">
            <template v-if="!userStore.avatar">
              <span style="font-size: 40px; font-weight: 500; color: #fff;">
                {{ userInitial }}
              </span>
            </template>
            <img v-else alt="avatar" :src="userStore.avatar" />
          </a-avatar>
          <div class="user-name">{{ userStore.name || '-' }}</div>
          <a-tag :color="roleColor" size="large">
            {{ roleLabel }}
          </a-tag>
        </div>

        <!-- 信息区域 -->
        <div class="info-section">
          <a-descriptions
            :column="1"
            :label-style="{ width: '120px' }"
            bordered
          >
            <a-descriptions-item :label="$t('userCenter.accountId')">
              {{ userStore.accountId || '-' }}
            </a-descriptions-item>
            <a-descriptions-item :label="$t('userCenter.username')">
              {{ userStore.name || '-' }}
            </a-descriptions-item>
            <a-descriptions-item :label="$t('userCenter.email')">
              {{ userStore.email || '-' }}
            </a-descriptions-item>
            <a-descriptions-item :label="$t('userCenter.role')">
              <a-space>
                <a-tag v-for="role in userRoles" :key="role" :color="getRoleColor(role)">
                  {{ getRoleLabel(role) }}
                </a-tag>
              </a-space>
            </a-descriptions-item>
            <a-descriptions-item :label="$t('userCenter.phone')">
              {{ userStore.phone || '-' }}
            </a-descriptions-item>
            <a-descriptions-item :label="$t('userCenter.registrationDate')">
              {{ userStore.registrationDate || '-' }}
            </a-descriptions-item>
          </a-descriptions>
        </div>
      </div>

      <!-- 操作按钮 -->
      <div class="action-section">
        <a-space>
          <a-button type="primary" @click="goToSettings">
            <template #icon>
              <icon-settings />
            </template>
            {{ $t('userCenter.editSettings') }}
          </a-button>
        </a-space>
      </div>
    </a-card>
  </div>
</template>

<script lang="ts" setup>
  import { computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { useUserStore } from '@/store';

  const userStore = useUserStore();
  const router = useRouter();

  const userInitial = computed(() => {
    const name = userStore.name || 'U';
    return name.charAt(0).toUpperCase();
  });

  const userRoles = computed(() => {
    if (userStore.roles && userStore.roles.length > 0) {
      return userStore.roles;
    }
    return userStore.role ? [userStore.role] : [];
  });

  const roleLabels: Record<string, string> = {
    super_admin: '超级管理员',
    admin: '系统管理员',
    user_manager: '用户管理员',
    auditor: '审计员',
    user: '普通用户',
  };

  const roleColors: Record<string, string> = {
    super_admin: 'red',
    admin: 'orangered',
    user_manager: 'blue',
    auditor: 'green',
    user: 'gray',
  };

  const getRoleLabel = (role: string) => {
    return roleLabels[role] || role;
  };

  const getRoleColor = (role: string) => {
    return roleColors[role] || 'gray';
  };

  const roleLabel = computed(() => {
    return getRoleLabel(userStore.role || 'user');
  });

  const roleColor = computed(() => {
    return getRoleColor(userStore.role || 'user');
  });

  const goToSettings = () => {
    router.push({ name: 'Setting' });
  };
</script>

<style scoped lang="less">
  .container {
    padding: 20px;
  }

  .user-info-wrapper {
    display: flex;
    gap: 60px;
    padding: 30px 0;
  }

  .avatar-section {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 16px;
    min-width: 150px;
  }

  .user-name {
    font-size: 20px;
    font-weight: 600;
    color: var(--color-text-1);
  }

  .info-section {
    flex: 1;
    max-width: 600px;
  }

  .action-section {
    margin-top: 20px;
    padding-top: 20px;
    border-top: 1px solid var(--color-border);
  }
</style>
