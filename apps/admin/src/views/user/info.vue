<template>
  <div class="container">
    <a-card class="general-card" :title="$t('userCenter.title')">
      <div class="user-info-wrapper">
        <!-- 头像区域 -->
        <div class="avatar-section">
          <a-avatar :size="100" :style="{ backgroundColor: '#165dff' }">
            <template v-if="!userStore.avatar">
              <span style="font-size: 40px; font-weight: 500; color: #fff">
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

        <!-- 信息区域（卡片分组式） -->
        <div class="info-section">
          <div class="info-group">
            <div class="info-item">
              <span class="info-label">{{ $t('userCenter.accountId') }}</span>
              <span class="info-value">{{ userStore.accountId || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">{{ $t('userCenter.username') }}</span>
              <span class="info-value">{{ userStore.name || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">{{ $t('userCenter.email') }}</span>
              <span class="info-value">{{ userStore.email || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">{{ $t('userCenter.role') }}</span>
              <span class="info-value">
                <a-space>
                  <a-tag
                    v-for="role in userRoles"
                    :key="role"
                    :color="getRoleColor(role)"
                  >
                    {{ getRoleLabel(role) }}
                  </a-tag>
                </a-space>
              </span>
            </div>
            <div class="info-item">
              <span class="info-label">{{ $t('userCenter.phone') }}</span>
              <span class="info-value">{{ userStore.phone || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">{{
                $t('userCenter.registrationDate')
              }}</span>
              <span class="info-value">{{
                userStore.registrationDate || '-'
              }}</span>
            </div>
          </div>
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
  import { computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useUserStore } from '@/store';
  import { user as userApi } from '@/api/administration/user';

  const userStore = useUserStore();
  const router = useRouter();

  // 页面加载时从后端获取最新用户信息
  onMounted(async () => {
    if (userStore.accountId) {
      try {
        const userInfo = await userApi.api.get(userStore.accountId);
        userStore.setInfo({
          name: userInfo.username,
          email: userInfo.email,
          phone: userInfo.phoneNumber,
        });
      } catch (error) {
        console.error('Failed to fetch user info:', error);
      }
    }
  });

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
    display: flex;
    align-items: center;
  }

  .info-group {
    width: 100%;
    background: var(--color-fill-2);
    border-radius: 12px;
    box-shadow: 0 2px 8px 0 rgba(0, 0, 0, 0.03);
    padding: 32px 32px 24px 32px;
    display: flex;
    flex-direction: column;
    gap: 18px;
  }

  .info-item {
    display: flex;
    align-items: center;
    gap: 16px;
    font-size: 16px;
  }
  .info-label {
    color: var(--color-text-3);
    min-width: 90px;
    font-weight: 500;
    letter-spacing: 1px;
  }
  .info-value {
    color: var(--color-text-1);
    font-weight: 400;
    flex: 1;
    word-break: break-all;
  }

  .action-section {
    margin-top: 20px;
    padding-top: 20px;
    border-top: 1px solid var(--color-border);
  }
</style>
