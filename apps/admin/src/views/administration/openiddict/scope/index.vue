<template>
  <div class="container">
    <Breadcrumb
      :items="[
        'menu.administration',
        'menu.administration.openiddict',
        'menu.administration.openiddict.scope',
      ]"
    />
    <a-card class="general-card search-card">
      <a-form :model="searchForm" layout="inline" class="search-form">
        <a-row :gutter="[16, 16]" style="width: 100%">
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item
              field="name"
              label="作用域名称"
              class="form-item-block"
            >
              <a-input
                v-model="searchForm.name"
                placeholder="请输入作用域名称"
                allow-clear
              >
                <template #prefix>
                  <icon-apps />
                </template>
              </a-input>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item
              field="displayName"
              label="显示名称"
              class="form-item-block"
            >
              <a-input
                v-model="searchForm.displayName"
                placeholder="请输入显示名称"
                allow-clear
              >
                <template #prefix>
                  <icon-tag />
                </template>
              </a-input>
            </a-form-item>
          </a-col>
          <a-col
            :xs="24"
            :sm="12"
            :md="24"
            :lg="12"
            :xl="12"
            class="action-col"
          >
            <a-space :size="12">
              <a-button type="primary" @click="handleSearch">
                <template #icon><icon-search /></template>
                查询
              </a-button>
              <a-button @click="handleReset">
                <template #icon><icon-refresh /></template>
                重置
              </a-button>
              <a-button type="primary" status="success" @click="handleAdd">
                <template #icon><icon-plus /></template>
                新增
              </a-button>
            </a-space>
          </a-col>
        </a-row>
      </a-form>
    </a-card>
    <a-card class="general-card table-card">
      <a-table
        :columns="columns"
        :data="tableData"
        :loading="loading"
        :bordered="false"
        :stripe="true"
        :pagination="{
          current: pagination.current,
          pageSize: pagination.pageSize,
          total: pagination.total,
          showTotal: true,
          showPageSize: true,
        }"
        @page-change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      >
        <template #resources="{ record }">
          <a-tag
            v-for="(resource, index) in record.resources?.slice(0, 3)"
            :key="index"
            size="small"
          >
            {{ resource }}
          </a-tag>
          <a-tag
            v-if="record.resources && record.resources.length > 3"
            size="small"
          >
            +{{ record.resources.length - 3 }}
          </a-tag>
        </template>

        <template #createdAt="{ record }">
          {{ formatDateTime(record.createdAt) }}
        </template>

        <template #operations="{ record }">
          <a-space>
            <a-button type="text" size="small" @click="handleView(record)">
              <template #icon><icon-eye /></template>
              {{ $t('common.detail') }}
            </a-button>
            <a-button
              type="text"
              size="small"
              status="warning"
              @click="handleEdit(record)"
            >
              <template #icon><icon-edit /></template>
              {{ $t('common.edit') }}
            </a-button>
            <a-popconfirm
              :content="$t('common.deleteConfirm')"
              @ok="handleDelete(record)"
            >
              <a-button type="text" size="small" status="danger">
                <template #icon><icon-delete /></template>
                {{ $t('common.delete') }}
              </a-button>
            </a-popconfirm>
          </a-space>
        </template>
      </a-table>
    </a-card>

    <!-- 新增/编辑对话框 -->
    <a-modal
      v-model:visible="modalVisible"
      :title="modalTitle"
      width="700px"
      :confirm-loading="submitting"
      :ok-button-props="{ disabled: submitting }"
      :cancel-button-props="{ disabled: submitting }"
      @ok="handleSubmit"
      @cancel="handleCancel"
    >
      <a-form :model="formData" :rules="formRules" layout="vertical">
        <a-form-item label="作用域名称" field="name">
          <a-input
            v-model="formData.name"
            placeholder="请输入作用域名称（如：openid, profile, api）"
            :disabled="isEdit"
          />
        </a-form-item>

        <a-form-item label="显示名称" field="displayName">
          <a-input
            v-model="formData.displayName"
            placeholder="请输入显示名称"
          />
        </a-form-item>

        <a-form-item label="描述" field="description">
          <a-textarea
            v-model="formData.description"
            placeholder="请输入描述"
            :auto-size="{ minRows: 3, maxRows: 6 }"
          />
        </a-form-item>

        <a-form-item label="关联资源" field="resources">
          <a-select
            v-model="formData.resources"
            placeholder="选择关联的API资源"
            multiple
            allow-clear
            allow-search
          >
            <a-option value="api">API</a-option>
            <a-option value="user-api">用户API</a-option>
            <a-option value="admin-api">管理API</a-option>
          </a-select>
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- 查看详情对话框 -->
    <a-modal
      v-model:visible="detailVisible"
      :footer="false"
      width="560px"
      title="作用域详情"
    >
      <div class="detail-content">
        <div class="detail-row">
          <span class="label">作用域名称</span>
          <span class="value">
            <a-tag color="blue" size="small">{{ currentRecord?.name }}</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">显示名称</span>
          <span class="value">{{ currentRecord?.displayName }}</span>
        </div>

        <div class="detail-row">
          <span class="label">描述</span>
          <span class="value">{{ currentRecord?.description || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">关联资源</span>
          <span class="value">
            <template v-if="currentRecord?.resources?.length">
              <a-space wrap :size="4">
                <a-tag
                  v-for="(resource, index) in currentRecord?.resources"
                  :key="index"
                  color="purple"
                  size="small"
                >
                  {{ resource }}
                </a-tag>
              </a-space>
            </template>
            <span v-else>-</span>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">创建时间</span>
          <span class="value">{{
            formatDateTime(currentRecord?.createdAt)
          }}</span>
        </div>

        <div class="detail-row">
          <span class="label">更新时间</span>
          <span class="value">{{
            formatDateTime(currentRecord?.updatedAt) || '-'
          }}</span>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, onMounted } from 'vue';
  import { Message } from '@arco-design/web-vue';
  import {
    scope,
    type IScope,
    type IScopeQuery,
    type ICreateScopeDto,
  } from '@/api/openiddict/scope';
  import { formatDateTime } from '@/utils/date';

  // 搜索表单
  const searchForm = reactive<IScopeQuery>({
    name: '',
    displayName: '',
  });

  // 表格数据
  const tableData = ref<IScope[]>([]);
  const loading = ref(false);
  const pagination = reactive({
    current: 1,
    pageSize: 10,
    total: 0,
  });

  // 表格列定义
  const columns = [
    { title: '作用域名称', dataIndex: 'name', width: 150 },
    { title: '显示名称', dataIndex: 'displayName', width: 150 },
    { title: '描述', dataIndex: 'description' },
    {
      title: '关联资源',
      dataIndex: 'resources',
      slotName: 'resources',
      width: 200,
    },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      slotName: 'createdAt',
      width: 180,
    },
    { title: '操作', slotName: 'operations', width: 180, align: 'center', fixed: 'right' },
  ];

  // 对话框状态
  const modalVisible = ref(false);
  const modalTitle = ref('新增作用域');
  const isEdit = ref(false);
  const detailVisible = ref(false);
  const currentRecord = ref<IScope | null>(null);
  const submitting = ref(false);

  // 表单数据
  const formData = reactive<ICreateScopeDto>({
    name: '',
    displayName: '',
    description: '',
    resources: [],
  });

  // 表单验证规则
  const formRules = {
    name: [{ required: true, message: '请输入作用域名称' }],
    displayName: [{ required: true, message: '请输入显示名称' }],
  };

  // 加载数据
  const loadData = async () => {
    loading.value = true;
    try {
      const result = await scope.getPagedList(
        searchForm,
        pagination.current,
        pagination.pageSize
      );
      tableData.value = result.items;
      pagination.total = result.totalCount;
    } catch (error) {
      Message.error('加载数据失败');
      console.error(error);
    } finally {
      loading.value = false;
    }
  };

  // 搜索
  const handleSearch = () => {
    pagination.current = 1;
    loadData();
  };

  // 重置
  const handleReset = () => {
    searchForm.name = '';
    searchForm.displayName = '';
    handleSearch();
  };

  // 分页变化
  const handlePageChange = (page: number) => {
    pagination.current = page;
    loadData();
  };

  const handlePageSizeChange = (pageSize: number) => {
    pagination.pageSize = pageSize;
    pagination.current = 1;
    loadData();
  };

  // 重置表单
  const resetForm = () => {
    formData.name = '';
    formData.displayName = '';
    formData.description = '';
    formData.resources = [];
  };

  // 新增
  const handleAdd = () => {
    isEdit.value = false;
    modalTitle.value = '新增作用域';
    resetForm();
    modalVisible.value = true;
  };

  // 编辑
  const handleEdit = (record: IScope) => {
    isEdit.value = true;
    modalTitle.value = '编辑作用域';
    currentRecord.value = record;

    // 填充表单
    formData.name = record.name;
    formData.displayName = record.displayName || '';
    formData.description = record.description || '';
    formData.resources = record.resources || [];

    modalVisible.value = true;
  };

  // 查看
  const handleView = (record: IScope) => {
    currentRecord.value = record;
    detailVisible.value = true;
  };

  // 删除
  const handleDelete = async (record: IScope) => {
    try {
      if (record.id) {
        await scope.delete(record.id);
        Message.success('删除成功');
        loadData();
      }
    } catch (error) {
      Message.error('删除失败');
      console.error(error);
    }
  };

  // 提交表单
  const handleSubmit = async () => {
    if (submitting.value) return;
    submitting.value = true;
    try {
      if (isEdit.value && currentRecord.value?.id) {
        await scope.update({
          ...formData,
          id: currentRecord.value.id,
        });
        Message.success('更新成功');
      } else {
        await scope.create(formData);
        Message.success('创建成功');
      }

      modalVisible.value = false;
      loadData();
    } catch (error) {
      Message.error(isEdit.value ? '更新失败' : '创建失败');
      console.error(error);
    } finally {
      submitting.value = false;
    }
  };

  // 取消
  const handleCancel = () => {
    modalVisible.value = false;
    resetForm();
  };

  onMounted(() => {
    loadData();
  });
</script>

<style scoped lang="less">
  .container {
    padding: 20px;

    .search-card {
      margin-bottom: 16px;
    }

    .search-form {
      :deep(.arco-form-item) {
        margin-bottom: 0;
      }

      .form-item-block {
        width: 100%;

        :deep(.arco-form-item-wrapper-col) {
          width: 100%;
        }
      }

      .action-col {
        display: flex;
        align-items: flex-end;
        justify-content: flex-end;

        :deep(.arco-btn) {
          font-weight: 500;
          border-radius: 4px;
          transition: all 0.3s ease;

          &.arco-btn-primary {
            &:not(.arco-btn-status-success) {
              background-color: #165dff;
              border-color: #165dff;

              &:hover {
                background-color: #4080ff;
                border-color: #4080ff;
              }
            }

            &.arco-btn-status-success {
              background-color: #00b42a;
              border-color: #00b42a;

              &:hover {
                background-color: #23c343;
                border-color: #23c343;
              }
            }
          }

          &.arco-btn-secondary {
            &:hover {
              background-color: #f2f3f5;
            }
          }
        }
      }
    }

    .table-card {
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
      border-radius: 4px;
    }

    :deep(.arco-table-th) {
      background-color: #f7f8fa;
      font-weight: 600;
    }
  }

  // 极简列表风格
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
        width: 90px;
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
