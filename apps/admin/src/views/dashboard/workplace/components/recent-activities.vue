<template>
  <a-spin :loading="loading" style="width: 100%">
    <a-card
      class="general-card"
      :header-style="{ paddingBottom: '0' }"
      :body-style="{ padding: '12px 16px' }"
    >
      <template #title>
        <span class="title">{{ $t('workplace.recentActivities') }}</span>
      </template>
      <template #extra>
        <a-link @click="fetchData()">{{ $t('workplace.refresh') }}</a-link>
      </template>
      <div class="activity-list">
        <div
          v-for="(item, index) in renderList"
          :key="index"
          class="activity-item"
        >
          <div class="activity-header">
            <a-tag size="small" color="arcoblue">{{ item.title }}</a-tag>
            <span class="activity-time">{{ item.time }}</span>
          </div>
          <div class="activity-desc">{{ item.description || '-' }}</div>
        </div>
        <a-empty v-if="renderList.length === 0" />
      </div>
    </a-card>
  </a-spin>
</template>

<script lang="ts" setup>
  import { ref, onMounted } from 'vue';
  import useLoading from '@/hooks/loading';
  import { queryRecentActivities, ActivityItem } from '@/api/dashboard';

  const { loading, setLoading } = useLoading();
  const renderList = ref<ActivityItem[]>([]);

  const fetchData = async () => {
    try {
      setLoading(true);
      const { data } = await queryRecentActivities({ limit: 5 });
      renderList.value = data || [];
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

<style scoped lang="less">
  .general-card {
    border-radius: 4px;

    .title {
      font-size: 14px;
      font-weight: 500;
    }
  }

  .activity-list {
    max-height: 300px;
    overflow-y: auto;
  }

  .activity-item {
    padding: 10px 0;
    border-bottom: 1px solid var(--color-neutral-2);

    &:last-child {
      border-bottom: none;
    }
  }

  .activity-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 6px;
  }

  .activity-time {
    font-size: 12px;
    color: var(--color-text-3);
  }

  .activity-desc {
    font-size: 12px;
    color: var(--color-text-2);
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
</style>
