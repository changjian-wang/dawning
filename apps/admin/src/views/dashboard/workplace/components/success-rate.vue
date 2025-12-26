<template>
  <a-spin :loading="loading" style="width: 100%">
    <a-card class="general-card" :body-style="{ padding: '20px' }">
      <template #title>
        {{ $t('workplace.successRate') }}
      </template>
      <div class="rate-container">
        <div class="chart-wrapper">
          <Chart :option="chartOption" style="width: 160px; height: 160px" />
        </div>
        <div class="stats-wrapper">
          <div class="stat-item">
            <span class="stat-label">{{ $t('workplace.totalRequests') }}</span>
            <span class="stat-value">{{
              formatNumber(statistics.totalRequests)
            }}</span>
          </div>
          <div class="stat-item success">
            <span class="stat-label">{{
              $t('workplace.successRequests')
            }}</span>
            <span class="stat-value">{{
              formatNumber(statistics.successRequests)
            }}</span>
          </div>
          <div class="stat-item warning">
            <span class="stat-label">{{ $t('workplace.clientErrors') }}</span>
            <span class="stat-value">{{
              formatNumber(statistics.clientErrors)
            }}</span>
          </div>
          <div class="stat-item error">
            <span class="stat-label">{{ $t('workplace.serverErrors') }}</span>
            <span class="stat-value">{{
              formatNumber(statistics.serverErrors)
            }}</span>
          </div>
        </div>
      </div>
    </a-card>
  </a-spin>
</template>

<script lang="ts" setup>
  import { ref, computed, onMounted } from 'vue';
  import { graphic } from 'echarts';
  import useLoading from '@/hooks/loading';
  import { queryRequestStatistics } from '@/api/dashboard';

  interface StatsData {
    totalRequests: number;
    successRequests: number;
    clientErrors: number;
    serverErrors: number;
  }

  const { loading, setLoading } = useLoading();

  const statistics = ref<StatsData>({
    totalRequests: 0,
    successRequests: 0,
    clientErrors: 0,
    serverErrors: 0,
  });

  const successRate = computed(() => {
    if (statistics.value.totalRequests === 0) return 100;
    return (
      (statistics.value.successRequests / statistics.value.totalRequests) * 100
    );
  });

  const rateColor = computed(() => {
    if (successRate.value >= 99) return '#00B42A';
    if (successRate.value >= 95) return '#7BC616';
    if (successRate.value >= 90) return '#FF7D00';
    return '#F53F3F';
  });

  const chartOption = computed(() => {
    const color = rateColor.value;
    const rate = successRate.value;
    return {
      series: [
        {
          type: 'gauge',
          startAngle: 90,
          endAngle: -270,
          radius: '90%',
          pointer: { show: false },
          progress: {
            show: true,
            overlap: false,
            roundCap: true,
            clip: false,
            itemStyle: {
              color: new graphic.LinearGradient(0, 0, 1, 0, [
                { offset: 0, color },
                { offset: 1, color },
              ]),
            },
          },
          axisLine: {
            lineStyle: {
              width: 12,
              color: [[1, '#E5E6EB']],
            },
          },
          splitLine: { show: false },
          axisTick: { show: false },
          axisLabel: { show: false },
          title: { show: false },
          detail: {
            fontSize: 24,
            fontWeight: 'bold',
            color,
            formatter: `${rate.toFixed(1)}%`,
            offsetCenter: [0, 0],
          },
          data: [{ value: rate }],
        },
      ],
    };
  });

  const formatNumber = (num: number): string => {
    if (num >= 1000000) return `${(num / 1000000).toFixed(1)}M`;
    if (num >= 1000) return `${(num / 1000).toFixed(1)}K`;
    return num.toString();
  };

  const fetchData = async () => {
    try {
      setLoading(true);
      const { data } = await queryRequestStatistics();
      if (data) {
        statistics.value = {
          totalRequests: data.totalRequests || 0,
          successRequests: data.successfulRequests || 0,
          clientErrors: data.clientErrors || 0,
          serverErrors: data.serverErrors || 0,
        };
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

  .rate-container {
    display: flex;
    align-items: center;
    gap: 24px;
  }

  .chart-wrapper {
    flex-shrink: 0;
  }

  .stats-wrapper {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .stat-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    background: var(--color-fill-1);
    border-radius: 4px;
    border-left: 3px solid var(--color-neutral-4);

    &.success {
      border-left-color: #00b42a;
    }

    &.warning {
      border-left-color: #ff7d00;
    }

    &.error {
      border-left-color: #f53f3f;
    }
  }

  .stat-label {
    font-size: 13px;
    color: var(--color-text-2);
  }

  .stat-value {
    font-size: 16px;
    font-weight: 600;
    color: var(--color-text-1);
  }
</style>
