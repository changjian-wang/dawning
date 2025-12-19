<template>
  <div class="container">
    <Breadcrumb
      :items="[
        'menu.administration',
        'menu.administration.openiddict',
        'menu.administration.openiddict.authorization',
      ]"
    />
    <a-card class="general-card search-card">
      <a-form :model="searchForm" layout="inline" class="search-form">
        <a-row :gutter="[16, 16]" style="width: 100%">
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item
              field="subject"
              label="用户标识"
              class="form-item-block"
            >
              <a-input
                v-model="searchForm.subject"
                placeholder="请输入用户ID"
                allow-clear
              >
                <template #prefix>
                  <icon-user />
                </template>
              </a-input>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item field="status" label="状态" class="form-item-block">
              <a-select
                v-model="searchForm.status"
                placeholder="请选择"
                allow-clear
              >
                <template #prefix>
                  <icon-check-circle />
                </template>
                <a-option value="valid">有效</a-option>
                <a-option value="revoked">已撤销</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item field="type" label="类型" class="form-item-block">
              <a-select
                v-model="searchForm.type"
                placeholder="请选择"
                allow-clear
              >
                <template #prefix>
                  <icon-safe />
                </template>
                <a-option value="permanent">永久授权</a-option>
                <a-option value="ad-hoc">临时授权</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="24" :lg="6" :xl="6" class="action-col">
            <a-space :size="12">
              <a-button type="primary" @click="handleSearch">
                <template #icon><icon-search /></template>
                查询
              </a-button>
              <a-button @click="handleReset">
                <template #icon><icon-refresh /></template>
                重置
              </a-button>
            </a-space>
          </a-col>
        </a-row>
      </a-form>
    </a-card>
    <a-card class="general-card table-card">
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
            <a-button
              type="text"
              size="small"
              @click="handleView(record)"
            >
              <template #icon><icon-eye /></template>
              详情
            </a-button>
            <a-popconfirm
              v-if="record.status === 'valid'"
              content="确定撤销该授权吗？撤销后用户需要重新授权。"
              @ok="handleRevoke(record)"
            >
              <a-button type="text" size="small" status="danger">
                <template #icon><icon-stop /></template>
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
      width="650px"
      :footer="false"
    >
      <div class="detail-content">
        <div class="detail-row">
          <span class="label">ID</span>
          <span class="value">
            <a-typography-text copyable>
              {{ currentRecord?.id }}
            </a-typography-text>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">用户标识</span>
          <span class="value">
            <a-typography-text v-if="currentRecord?.subject" copyable>
              {{ currentRecord.subject }}
            </a-typography-text>
            <template v-else>-</template>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">应用程序ID</span>
          <span class="value">
            <a-typography-text v-if="currentRecord?.applicationId" copyable>
              {{ currentRecord.applicationId }}
            </a-typography-text>
            <template v-else>-</template>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">状态</span>
          <span class="value">
            <a-tag
              :color="currentRecord?.status === 'valid' ? 'green' : 'red'"
              size="small"
            >
              {{ currentRecord?.status === 'valid' ? '有效' : '已撤销' }}
            </a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">类型</span>
          <span class="value">
            <a-tag
              :color="currentRecord?.type === 'permanent' ? 'blue' : 'orange'"
              size="small"
            >
              {{ currentRecord?.type === 'permanent' ? '永久授权' : '临时授权' }}
            </a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">作用域</span>
          <span class="value">
            <a-space wrap>
              <a-tag
                v-for="scope in currentRecord?.scopes || []"
                :key="scope"
                color="arcoblue"
                size="small"
              >
                {{ scope }}
              </a-tag>
              <template v-if="!currentRecord?.scopes?.length">-</template>
            </a-space>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">创建时间</span>
          <span class="value">{{ formatDate(currentRecord?.createdAt) }}</span>
        </div>
      </div>
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
  const currentRecord = ref<IAuthorization | null>(null);

  const columns = [
    { title: '用户标识', dataIndex: 'subject', width: 200, ellipsis: true },
    { title: '状态', slotName: 'status', width: 100 },
    { title: '类型', slotName: 'type', width: 100 },
    { title: '作用域', slotName: 'scopes', width: 200 },
    { title: '创建时间', slotName: 'createdAt', width: 160 },
    { title: '操作', slotName: 'optional', width: 140, align: 'center' },
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
    Object.keys(searchForm).forEach((k) => {
      (searchForm as any)[k] = undefined;
    });
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
    currentRecord.value = record;
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

<style scoped lang="less">
  .detail-content {
    .detail-row {
      display: flex;
      padding: 14px 0;
      border-bottom: 1px solid var(--color-border-1);
      align-items: flex-start;

      &:last-child {
        border-bottom: none;
      }

      .label {
        width: 110px;
        flex-shrink: 0;
        font-size: 14px;
        color: var(--color-text-3);
        line-height: 1.5;
      }

      .value {
        flex: 1;
        font-size: 14px;
        color: var(--color-text-1);
        line-height: 1.5;
        word-break: break-word;
      }
    }
  }
</style>
