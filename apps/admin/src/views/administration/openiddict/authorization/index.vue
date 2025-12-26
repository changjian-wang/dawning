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
              :label="$t('authorization.form.subject')"
              class="form-item-block"
            >
              <a-input
                v-model="searchForm.subject"
                :placeholder="$t('authorization.form.subject.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-user />
                </template>
              </a-input>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item field="status" :label="$t('authorization.form.status')" class="form-item-block">
              <a-select
                v-model="searchForm.status"
                :placeholder="$t('authorization.form.status.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-check-circle />
                </template>
                <a-option value="valid">{{ $t('authorization.form.status.valid') }}</a-option>
                <a-option value="revoked">{{ $t('authorization.form.status.revoked') }}</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item field="type" :label="$t('authorization.form.type')" class="form-item-block">
              <a-select
                v-model="searchForm.type"
                :placeholder="$t('authorization.form.type.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-safe />
                </template>
                <a-option value="permanent">{{ $t('authorization.form.type.permanent') }}</a-option>
                <a-option value="ad-hoc">{{ $t('authorization.form.type.adhoc') }}</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="24" :lg="6" :xl="6" class="action-col">
            <a-space :size="12">
              <a-button type="primary" size="small" @click="handleSearch">
                <template #icon><icon-search /></template>
                {{ $t('authorization.button.search') }}
              </a-button>
              <a-button size="small" @click="handleReset">
                <template #icon><icon-refresh /></template>
                {{ $t('authorization.button.reset') }}
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
            {{ record.status === 'valid' ? $t('authorization.form.status.valid') : $t('authorization.form.status.revoked') }}
          </a-tag>
        </template>
        <template #type="{ record }">
          <a-tag :color="record.type === 'permanent' ? 'blue' : 'orange'">
            {{ record.type === 'permanent' ? $t('authorization.form.type.permanent') : $t('authorization.form.type.adhoc') }}
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
            <a-tooltip :content="$t('authorization.action.detail')">
              <a-button
                type="text"
                size="small"
                @click="handleView(record)"
              >
                <template #icon><icon-eye /></template>
              </a-button>
            </a-tooltip>
            <a-popconfirm
              v-if="record.status === 'valid'"
              :content="$t('authorization.action.revokeConfirm')"
              @ok="handleRevoke(record)"
            >
              <a-tooltip :content="$t('authorization.action.revoke')">
                <a-button type="text" size="small" status="danger">
                  <template #icon><icon-stop /></template>
                </a-button>
              </a-tooltip>
            </a-popconfirm>
          </a-space>
        </template>
      </a-table>
    </a-card>

    <!-- 详情对话框 -->
    <a-modal
      v-model:visible="detailVisible"
      :title="$t('authorization.modal.detail')"
      width="650px"
      :footer="false"
    >
      <div class="detail-content">
        <div class="detail-row">
          <span class="label">{{ $t('authorization.detail.id') }}</span>
          <span class="value">
            <a-typography-text copyable>
              {{ currentRecord?.id }}
            </a-typography-text>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('authorization.detail.subject') }}</span>
          <span class="value">
            <a-typography-text v-if="currentRecord?.subject" copyable>
              {{ currentRecord.subject }}
            </a-typography-text>
            <template v-else>-</template>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('authorization.detail.applicationId') }}</span>
          <span class="value">
            <a-typography-text v-if="currentRecord?.applicationId" copyable>
              {{ currentRecord.applicationId }}
            </a-typography-text>
            <template v-else>-</template>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('authorization.detail.status') }}</span>
          <span class="value">
            <a-tag
              :color="currentRecord?.status === 'valid' ? 'green' : 'red'"
              size="small"
            >
              {{ currentRecord?.status === 'valid' ? $t('authorization.form.status.valid') : $t('authorization.form.status.revoked') }}
            </a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('authorization.detail.type') }}</span>
          <span class="value">
            <a-tag
              :color="currentRecord?.type === 'permanent' ? 'blue' : 'orange'"
              size="small"
            >
              {{ currentRecord?.type === 'permanent' ? $t('authorization.form.type.permanent') : $t('authorization.form.type.adhoc') }}
            </a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('authorization.detail.scopes') }}</span>
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
          <span class="label">{{ $t('authorization.detail.createdAt') }}</span>
          <span class="value">{{ formatDate(currentRecord?.createdAt) }}</span>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, computed, onMounted } from 'vue';
  import { useI18n } from 'vue-i18n';
  import { Message } from '@arco-design/web-vue';
  import {
    authorizationApi,
    IAuthorization,
    IAuthorizationModel,
  } from '@/api/openiddict/authorization';

  const { t } = useI18n();

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

  const columns = computed(() => [
    { title: t('authorization.column.subject'), dataIndex: 'subject', width: 200, ellipsis: true },
    { title: t('authorization.column.status'), slotName: 'status', width: 100 },
    { title: t('authorization.column.type'), slotName: 'type', width: 100 },
    { title: t('authorization.column.scopes'), slotName: 'scopes', width: 200 },
    { title: t('authorization.column.createdAt'), slotName: 'createdAt', width: 160 },
    { title: t('common.actions'), slotName: 'optional', width: 100, align: 'center' },
  ]);

  function formatDate(dateStr?: string) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    const seconds = String(d.getSeconds()).padStart(2, '0');
    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
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
      Message.success(t('authorization.message.revokeSuccess'));
      loadData();
    } catch (e: any) {
      Message.error(e?.message || t('authorization.message.revokeFailed'));
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
