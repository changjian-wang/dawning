<template>
  <a-spin :loading="loading" style="width: 100%">
    <a-grid :cols="24" :row-gap="16" class="panel">
      <a-grid-item
        class="panel-col"
        :span="{ xs: 12, sm: 12, md: 12, lg: 12, xl: 12, xxl: 6 }"
      >
        <a-space>
          <a-avatar :size="54" class="col-avatar">
            <icon-user :style="{ fontSize: '28px', color: '#165DFF' }" />
          </a-avatar>
          <a-statistic
            :title="$t('workplace.totalUsers')"
            :value="stats.totalUsers"
            :value-from="0"
            animation
            show-group-separator
          >
            <template #suffix>
              <span class="unit">{{ $t('workplace.count') }}</span>
            </template>
          </a-statistic>
        </a-space>
      </a-grid-item>
      <a-grid-item
        class="panel-col"
        :span="{ xs: 12, sm: 12, md: 12, lg: 12, xl: 12, xxl: 6 }"
      >
        <a-space>
          <a-avatar :size="54" class="col-avatar">
            <icon-user-group :style="{ fontSize: '28px', color: '#722ED1' }" />
          </a-avatar>
          <a-statistic
            :title="$t('workplace.totalRoles')"
            :value="stats.totalRoles"
            :value-from="0"
            animation
            show-group-separator
          >
            <template #suffix>
              <span class="unit">{{ $t('workplace.count') }}</span>
            </template>
          </a-statistic>
        </a-space>
      </a-grid-item>
      <a-grid-item
        class="panel-col"
        :span="{ xs: 12, sm: 12, md: 12, lg: 12, xl: 12, xxl: 6 }"
      >
        <a-space>
          <a-avatar :size="54" class="col-avatar">
            <icon-file :style="{ fontSize: '28px', color: '#F77234' }" />
          </a-avatar>
          <a-statistic
            :title="$t('workplace.todayLogs')"
            :value="stats.todayAuditLogs"
            :value-from="0"
            animation
            show-group-separator
          >
            <template #suffix>
              <span class="unit">{{ $t('workplace.count') }}</span>
            </template>
          </a-statistic>
        </a-space>
      </a-grid-item>
      <a-grid-item
        class="panel-col"
        :span="{ xs: 12, sm: 12, md: 12, lg: 12, xl: 12, xxl: 6 }"
        style="border-right: none"
      >
        <a-space>
          <a-avatar :size="54" class="col-avatar">
            <icon-apps :style="{ fontSize: '28px', color: '#0FC6C2' }" />
          </a-avatar>
          <a-statistic
            :title="$t('workplace.totalApps')"
            :value="stats.totalApplications"
            :value-from="0"
            animation
          >
            <template #suffix>
              <span class="unit">{{ $t('workplace.count') }}</span>
            </template>
          </a-statistic>
        </a-space>
      </a-grid-item>
      <a-grid-item :span="24">
        <a-divider class="panel-border" />
      </a-grid-item>
    </a-grid>
  </a-spin>
</template>

<script lang="ts" setup>
  import { ref, onMounted } from 'vue';
  import { queryDashboardStats, DashboardStats } from '@/api/dashboard';
  import useLoading from '@/hooks/loading';

  const { loading, setLoading } = useLoading(true);
  const stats = ref<DashboardStats>({
    totalUsers: 0,
    totalRoles: 0,
    todayAuditLogs: 0,
    totalApplications: 0,
    growthRate: 0,
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const { data } = await queryDashboardStats();
      stats.value = data;
    } catch (err) {
      // Handle error
    } finally {
      setLoading(false);
    }
  };

  onMounted(() => {
    fetchData();
  });
</script>

<style lang="less" scoped>
  .arco-grid.panel {
    margin-bottom: 0;
    padding: 16px 20px 0;
  }

  .panel-col {
    padding-left: 43px;
    border-right: 1px solid rgb(var(--gray-2));
  }

  .col-avatar {
    margin-right: 12px;
    background-color: var(--color-fill-2);
  }

  .up-icon {
    color: rgb(var(--red-6));
  }

  .unit {
    margin-left: 8px;
    color: rgb(var(--gray-8));
    font-size: 12px;
  }

  :deep(.panel-border) {
    margin: 4px 0 0;
  }
</style>
