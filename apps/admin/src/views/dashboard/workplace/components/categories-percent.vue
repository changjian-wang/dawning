<template>
  <a-spin :loading="loading" style="width: 100%">
    <a-card
      class="general-card"
      :header-style="{ paddingBottom: '0' }"
      :body-style="{ padding: '20px' }"
    >
      <template #title>
        {{ $t('workplace.categoriesPercent') }}
      </template>
      <Chart height="310px" :option="chartOption" />
    </a-card>
  </a-spin>
</template>

<script lang="ts" setup>
  import { ref, onMounted } from 'vue';
  import { queryCategories, CategoriesData } from '@/api/dashboard';
  import useLoading from '@/hooks/loading';
  import useChartOption from '@/hooks/chart-option';

  const { loading, setLoading } = useLoading(true);
  const categoriesData = ref<CategoriesData>({
    categories: [],
    total: 0,
  });

  const colors = ['#249EFF', '#313CA9', '#21CCFF', '#F7BA1E', '#722ED1'];
  const darkColors = ['#3D72F6', '#A079DC', '#6CAAF5', '#F5B544', '#9F5EE5'];

  const { chartOption } = useChartOption((isDark) => {
    const { categories, total } = categoriesData.value;

    return {
      legend: {
        left: 'center',
        data: categories.map((c) => c.name),
        bottom: 0,
        icon: 'circle',
        itemWidth: 8,
        textStyle: {
          color: isDark ? 'rgba(255, 255, 255, 0.7)' : '#4E5969',
        },
        itemStyle: {
          borderWidth: 0,
        },
      },
      tooltip: {
        show: true,
        trigger: 'item',
      },
      graphic: {
        elements: [
          {
            type: 'text',
            left: 'center',
            top: '40%',
            style: {
              text: 'Total',
              textAlign: 'center',
              fill: isDark ? '#ffffffb3' : '#4E5969',
              fontSize: 14,
            },
          },
          {
            type: 'text',
            left: 'center',
            top: '50%',
            style: {
              text: total.toLocaleString(),
              textAlign: 'center',
              fill: isDark ? '#ffffffb3' : '#1D2129',
              fontSize: 16,
              fontWeight: 500,
            },
          },
        ],
      },
      series: [
        {
          type: 'pie',
          radius: ['50%', '70%'],
          center: ['50%', '50%'],
          label: {
            formatter: '{d}%',
            fontSize: 14,
            color: isDark ? 'rgba(255, 255, 255, 0.7)' : '#4E5969',
          },
          itemStyle: {
            borderColor: isDark ? '#232324' : '#fff',
            borderWidth: 1,
          },
          data: categories.map((c, idx) => ({
            value: c.value,
            name: c.name,
            itemStyle: {
              color: isDark
                ? darkColors[idx % darkColors.length]
                : colors[idx % colors.length],
            },
          })),
        },
      ],
    };
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const { data } = await queryCategories();
      categoriesData.value = data;
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

<style scoped lang="less"></style>
