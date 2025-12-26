<template>
  <a-spin :loading="loading" style="width: 100%">
    <a-card
      class="general-card"
      :header-style="{ paddingBottom: '0' }"
      :body-style="{ padding: '16px' }"
    >
      <template #title>
        {{ $t('workplace.topPaths') }}
      </template>
      <template #extra>
        <a-link @click="fetchData()">{{ $t('workplace.refresh') }}</a-link>
      </template>
      <div class="path-list">
        <div v-for="(item, index) in topPaths" :key="index" class="path-item">
          <div class="path-rank" :class="getRankClass(index)">
            {{ index + 1 }}
          </div>
          <div class="path-info">
            <div class="path-name">{{ item.path }}</div>
            <div class="path-stats">
              <span class="stat">
                <icon-thunderbolt />
                {{ formatNumber(item.count) }} {{ $t('workplace.requests') }}
              </span>
              <span v-if="item.avgTime" class="stat">
                <icon-clock-circle />
                {{ item.avgTime.toFixed(0) }}ms
              </span>
            </div>
          </div>
          <div class="path-bar">
            <div
              class="bar-fill"
              :style="{ width: getBarWidth(item.count) }"
            ></div>
          </div>
        </div>
        <a-empty v-if="topPaths.length === 0" />
      </div>
    </a-card>
  </a-spin>
</template>

<script lang="ts" setup>
  import { ref, computed, onMounted } from 'vue';
  import useLoading from '@/hooks/loading';
  import { queryRequestStatistics } from '@/api/dashboard';

  interface PathItem {
    path: string;
    count: number;
    avgTime?: number;
  }

  const { loading, setLoading } = useLoading();
  const topPaths = ref<PathItem[]>([]);

  const maxCount = computed(() => {
    if (topPaths.value.length === 0) return 1;
    return Math.max(...topPaths.value.map((p) => p.count));
  });

  const getRankClass = (index: number): string => {
    if (index === 0) return 'gold';
    if (index === 1) return 'silver';
    if (index === 2) return 'bronze';
    return '';
  };

  const getBarWidth = (count: number): string => {
    if (!count || maxCount.value === 0) return '0%';
    return `${(count / maxCount.value) * 100}%`;
  };

  const formatNumber = (num: number): string => {
    if (num === undefined || num === null) return '0';
    if (num >= 1000000) return `${(num / 1000000).toFixed(1)}M`;
    if (num >= 1000) return `${(num / 1000).toFixed(1)}K`;
    return num.toString();
  };

  const fetchData = async () => {
    try {
      setLoading(true);
      const { data } = await queryRequestStatistics();
      if (data?.topPaths) {
        topPaths.value = data.topPaths.slice(0, 5).map((p) => ({
          path: p.path || '',
          count: p.count || p.requestCount || 0,
          avgTime: p.avgResponseTimeMs || p.averageResponseTimeMs,
        }));
      }
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
  }

  .path-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .path-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px;
    background: var(--color-fill-1);
    border-radius: 6px;
    transition: all 0.2s;

    &:hover {
      background: var(--color-fill-2);
    }
  }

  .path-rank {
    width: 24px;
    height: 24px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 12px;
    font-weight: 600;
    color: var(--color-text-3);
    background: var(--color-fill-3);
    border-radius: 50%;

    &.gold {
      color: #fff;
      background: linear-gradient(135deg, #ffd700, #ffb800);
    }

    &.silver {
      color: #fff;
      background: linear-gradient(135deg, #c0c0c0, #a0a0a0);
    }

    &.bronze {
      color: #fff;
      background: linear-gradient(135deg, #cd7f32, #b87333);
    }
  }

  .path-info {
    flex: 1;
    min-width: 0;
  }

  .path-name {
    font-size: 13px;
    color: var(--color-text-1);
    font-weight: 500;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .path-stats {
    display: flex;
    gap: 16px;
    margin-top: 4px;
  }

  .stat {
    display: flex;
    align-items: center;
    gap: 4px;
    font-size: 12px;
    color: var(--color-text-3);

    svg {
      font-size: 12px;
    }
  }

  .path-bar {
    width: 80px;
    height: 6px;
    background: var(--color-fill-3);
    border-radius: 3px;
    overflow: hidden;
  }

  .bar-fill {
    height: 100%;
    background: linear-gradient(90deg, #165dff, #722ed1);
    border-radius: 3px;
    transition: width 0.3s ease;
  }
</style>
