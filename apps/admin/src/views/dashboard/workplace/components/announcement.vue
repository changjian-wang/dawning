<template>
  <a-spin :loading="loading" style="width: 100%">
    <a-card
      class="general-card"
      :title="$t('workplace.announcement')"
      :header-style="{ paddingBottom: '0' }"
      :body-style="{ padding: '15px 20px 13px 20px' }"
    >
      <template #extra>
        <a-link @click="fetchData">{{ $t('workplace.refresh') }}</a-link>
      </template>
      <div>
        <a-empty v-if="!list.length" />
        <div v-for="(item, idx) in list" :key="idx" class="item">
          <a-tag :color="item.type" size="small">{{ item.label }}</a-tag>
          <span class="item-content">
            {{ item.content }}
          </span>
        </div>
      </div>
    </a-card>
  </a-spin>
</template>

<script lang="ts" setup>
  import { ref, onMounted } from 'vue';
  import { queryAnnouncements, AnnouncementItem } from '@/api/dashboard';
  import useLoading from '@/hooks/loading';

  const { loading, setLoading } = useLoading(true);
  const list = ref<AnnouncementItem[]>([]);

  const fetchData = async () => {
    try {
      setLoading(true);
      const { data } = await queryAnnouncements();
      list.value = data || [];
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
  .item {
    display: flex;
    align-items: center;
    width: 100%;
    height: 24px;
    margin-bottom: 4px;

    .item-content {
      flex: 1;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      margin-left: 4px;
      color: var(--color-text-2);
      text-decoration: none;
      font-size: 13px;
      cursor: pointer;
    }
  }
</style>
