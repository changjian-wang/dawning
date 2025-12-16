<template>
  <div class="container">
    <Breadcrumb :items="['认证授权', '授权管理']" />
    <a-card class="general-card" title="授权管理">
      <!-- 搜索区 -->
      <a-row style="margin-bottom: 16px">
        <a-col :span="24">
          <a-form :model="searchForm" layout="inline">
            <a-form-item field="subject" label="用户标识">
              <a-input
                v-model="searchForm.subject"
                placeholder="请输入用户ID"
                allow-clear
                style="width: 200px"
              />
            </a-form-item>
            <a-form-item field="status" label="状态">
              <a-select
                v-model="searchForm.status"
                placeholder="请选择"
                allow-clear
                style="width: 120px"
              >
                <a-option value="valid">有效</a-option>
                <a-option value="revoked">已撤销</a-option>
              </a-select>
            </a-form-item>
            <a-form-item field="type" label="类型">
              <a-select
                v-model="searchForm.type"
                placeholder="请选择"
                allow-clear
                style="width: 140px"
              >
                <a-option value="permanent">永久授权</a-option>
                <a-option value="ad-hoc">临时授权</a-option>
              </a-select>
            </a-form-item>
            <a-form-item>
              <a-space>
                <a-button type="primary" @click="handleSearch">
                  <template #icon><icon-search /></template>
                  查询
                </a-button>
                <a-button @click="handleReset">
                  <template #icon><icon-refresh /></template>
                  重置
                </a-button>
              </a-space>
            </a-form-item>
          </a-form>
        </a-col>
      </a-row>

      <!-- 数据表格 -->
      <a-table
        :columns="columns"
        :data="data"
        :loading="loading"
        :pagination="{
          total,
          current: pageIndex,
          pageSize,
          showTotal: true,
          showPageSize: true,
        }"
        :bordered="false"
        @page-change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      >
        <template #status="{ record }">
          <a-tag :color="record.status === 'valid' ? 'green' : 'red'">
            {{ record.status === 'valid' ? '有效' : '已撤销' }}
          </a-tag>
        </template>
        <template #type="{ record }">
          <a-tag :color="record.type === 'permanent' ? 'blue' : 'orange'">
            {{ record.type === 'permanent' ? '永久授权' : '临时授权' }}
          </a-tag>
        </template>
        <template #scopes="{ record }">
          <a-space wrap>
            <a-tag
              v-for="scope in (record.scopes || []).slice(0, 3)"
              :key="scope"
              color="arcoblue"
            >
              {{ scope }}
            </a-tag>
            <a-tag v-if="(record.scopes || []).length > 3" color="gray">
              +{{ record.scopes.length - 3 }}
            </a-tag>
          </a-space>
        </template>
        <template #createdAt="{ record }">
          {{ formatDate(record.createdAt) }}
        </template>
        <template #optional="{ record }">
          <a-space>
            <a-button type="text" size="small" @click="handleView(record)">
              详情
            </a-button>
            <a-popconfirm
              v-if="record.status === 'valid'"
              content="确定撤销该授权吗？撤销后用户需要重新授权。"
              @ok="handleRevoke(record)"
            >
              <a-button type="text" size="small" status="danger">
                撤销
              </a-button>
            </a-popconfirm>
          </a-space>
        </template>
      </a-table>
    </a-card>

    <!-- 详情对话框 -->
    <a-modal
      v-model:visible="detailVisible"
      title="授权详情"
      :width="600"
      :footer="false"
    >
      <a-descriptions :data="detailData" :column="1" bordered />
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, onMounted } from 'vue';
  import { Message } from '@arco-design/web-vue';
  import {
    authorizationApi,
    IAuthorization,
    IAuthorizationModel,
  } from '@/api/openiddict/authorization';

  // 搜索表单
  const searchForm = reactive<IAuthorizationModel>({});
  const loading = ref(false);
  const data = ref<IAuthorization[]>([]);
  const total = ref(0);
  const pageIndex = ref(1);
  const pageSize = ref(10);

  // 详情
  const detailVisible = ref(false);
  const detailData = ref<any[]>([]);

  const columns = [
    { title: 'ID', dataIndex: 'id', width: 180, ellipsis: true },
    { title: '用户标识', dataIndex: 'subject', width: 200, ellipsis: true },
    { title: '状态', slotName: 'status', width: 100 },
    { title: '类型', slotName: 'type', width: 100 },
    { title: '作用域', slotName: 'scopes', width: 200 },
    { title: '创建时间', slotName: 'createdAt', width: 160 },
    { title: '操作', slotName: 'optional', width: 120 },
  ];

  function formatDate(dateStr?: string) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return d.toLocaleString('zh-CN');
  }

  // 加载数据
  async function loadData() {
    loading.value = true;
    try {
      const res = await authorizationApi.getPagedList(
        searchForm,
        pageIndex.value,
        pageSize.value
      );
      data.value = res.items;
      total.value = res.totalCount;
    } finally {
      loading.value = false;
    }
  }

  function handleSearch() {
    pageIndex.value = 1;
    loadData();
  }

  function handleReset() {
    Object.keys(searchForm).forEach(
      (k) => ((searchForm as any)[k] = undefined)
    );
    pageIndex.value = 1;
    loadData();
  }

  function handlePageChange(page: number) {
    pageIndex.value = page;
    loadData();
  }

  function handlePageSizeChange(size: number) {
    pageSize.value = size;
    pageIndex.value = 1;
    loadData();
  }

  function handleView(record: IAuthorization) {
    detailData.value = [
      { label: 'ID', value: record.id },
      { label: '用户标识', value: record.subject || '-' },
      { label: '应用程序ID', value: record.applicationId || '-' },
      { label: '状态', value: record.status === 'valid' ? '有效' : '已撤销' },
      {
        label: '类型',
        value: record.type === 'permanent' ? '永久授权' : '临时授权',
      },
      { label: '作用域', value: (record.scopes || []).join(', ') || '-' },
      { label: '创建时间', value: formatDate(record.createdAt) },
    ];
    detailVisible.value = true;
  }

  async function handleRevoke(record: IAuthorization) {
    try {
      await authorizationApi.revoke(record.id);
      Message.success('授权已撤销');
      loadData();
    } catch (e: any) {
      Message.error(e?.message || '撤销失败');
    }
  }

  onMounted(loadData);
</script>

<style scoped lang="less"></style>
