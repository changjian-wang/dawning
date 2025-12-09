<template>
  <div class="role-management">
    <div class="container">
      <Breadcrumb :items="['menu.administration', 'menu.administration.role']" />
      <a-card class="general-card search-card">
        <a-form :model="searchForm" layout="inline" class="search-form">
          <a-row :gutter="[16, 16]" style="width: 100%">
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item field="name" :label="$t('role.form.name')" class="form-item-block">
                <a-input
                  v-model="searchForm.name"
                  :placeholder="$t('role.form.name.placeholder')"
                  allow-clear
                  @press-enter="search"
                >
                  <template #prefix>
                    <icon-user-group />
                  </template>
                </a-input>
              </a-form-item>
            </a-col>
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item field="isActive" :label="$t('role.columns.isActive')" class="form-item-block">
                <a-select
                  v-model="searchForm.isActive"
                  :placeholder="$t('role.form.status.placeholder')"
                  allow-clear
                >
                  <template #prefix>
                    <icon-check-circle />
                  </template>
                  <a-option :value="true">{{ $t('role.status.active') }}</a-option>
                  <a-option :value="false">{{ $t('role.status.inactive') }}</a-option>
                </a-select>
              </a-form-item>
            </a-col>
            <a-col :xs="24" :sm="12" :md="24" :lg="12" :xl="12" class="action-col">
              <a-space :size="12">
                <a-button type="primary" @click="search">
                  <template #icon><icon-search /></template>
                  {{ $t('searchTable.form.search') }}
                </a-button>
                <a-button @click="reset">
                  <template #icon><icon-refresh /></template>
                  {{ $t('searchTable.form.reset') }}
                </a-button>
                <a-button type="primary" status="success" @click="handleCreate">
                  <template #icon><icon-plus /></template>
                  {{ $t('role.button.create') }}
                </a-button>
              </a-space>
            </a-col>
          </a-row>
        </a-form>
      </a-card>
      <a-card class="general-card table-card">

      <!-- 表格 -->
      <a-table
        row-key="id"
        :loading="loading"
        :pagination="pagination"
        :columns="columns"
        :data="tableData"
        :bordered="false"
        @page-change="onPageChange"
        @page-size-change="onPageSizeChange"
      >
        <template #isSystem="{ record }">
          <a-tag :color="record.isSystem ? 'orange' : 'gray'">
            {{ record.isSystem ? $t('role.system.yes') : $t('role.system.no') }}
          </a-tag>
        </template>
        <template #isActive="{ record }">
          <a-tag v-if="record.isActive" color="arcoblue" size="small">
            <template #icon><icon-check-circle-fill /></template>
            {{ $t('role.status.active') }}
          </a-tag>
          <a-tag v-else color="red" size="small">
            <template #icon><icon-close-circle-fill /></template>
            {{ $t('role.status.inactive') }}
          </a-tag>
        </template>
        <template #permissions="{ record }">
          <div v-if="record.permissions && record.permissions.length > 0" class="permissions-cell">
            <a-space :size="4" wrap>
              <a-tag 
                v-for="(perm, idx) in record.permissions.slice(0, 2)" 
                :key="idx" 
                size="small"
                color="blue"
              >
                {{ perm }}
              </a-tag>
              <a-popover v-if="record.permissions.length > 2" position="top" :content-style="{ maxWidth: '400px', maxHeight: '300px', overflowY: 'auto' }">
                <a-tag size="small" color="arcoblue" style="cursor: pointer;">
                  +{{ record.permissions.length - 2 }}
                </a-tag>
                <template #content>
                  <div class="permissions-popover">
                    <div class="permission-header">全部权限 ({{ record.permissions.length }})</div>
                    <div v-for="(perm, idx) in record.permissions" :key="idx" class="permission-item">
                      <icon-check-circle style="color: #00b42a; margin-right: 4px;" />
                      {{ perm }}
                    </div>
                  </div>
                </template>
              </a-popover>
            </a-space>
          </div>
          <span v-else style="color: var(--color-text-3)">-</span>
        </template>
        <template #createdAt="{ record }">
          {{ formatDateTime(record.createdAt) }}
        </template>
        <template #operations="{ record }">
          <a-space>
            <a-button
              type="text"
              size="medium"
              status="warning"
              @click="handleEdit(record)"
              :disabled="record.isSystem"
            >
              <template #icon><icon-edit :size="18" /></template>
            </a-button>
            <a-button
              type="text"
              size="medium"
              status="danger"
              @click="handleDelete(record)"
              :disabled="record.isSystem"
            >
              <template #icon><icon-delete :size="18" /></template>
            </a-button>
          </a-space>
        </template>
      </a-table>
    </a-card>

    <!-- 创建/编辑对话框 -->
    <a-modal
      v-model:visible="modalVisible"
      :title="isEdit ? $t('role.button.edit') : $t('role.button.create')"
      @ok="handleSubmit"
      @cancel="handleCancel"
      width="600px"
    >
      <a-form ref="formRef" :model="formData" layout="vertical">
        <a-form-item
          field="name"
          :label="$t('role.form.name')"
          :rules="[{ required: true, message: $t('role.form.name.placeholder') }]"
        >
          <a-input
            v-model="formData.name"
            :placeholder="$t('role.form.name.placeholder')"
            :disabled="isEdit"
          />
        </a-form-item>
        <a-form-item
          field="displayName"
          :label="$t('role.form.displayName')"
          :rules="[{ required: true, message: $t('role.form.displayName.placeholder') }]"
        >
          <a-input
            v-model="formData.displayName"
            :placeholder="$t('role.form.displayName.placeholder')"
          />
        </a-form-item>
        <a-form-item field="description" :label="$t('role.form.description')">
          <a-textarea
            v-model="formData.description"
            :placeholder="$t('role.form.description.placeholder')"
            :auto-size="{ minRows: 2, maxRows: 4 }"
          />
        </a-form-item>
        <a-form-item field="isActive" :label="$t('role.form.isActive')">
          <a-switch v-model="formData.isActive" />
        </a-form-item>
        <a-form-item field="permissions" :label="$t('role.form.permissions')">
          <a-space direction="vertical" fill>
            <a-input
              v-for="(perm, idx) in formData.permissions"
              :key="idx"
              v-model="formData.permissions[idx]"
              :placeholder="$t('role.form.permissions.placeholder')"
            >
              <template #suffix>
                <icon-delete @click="removePermission(idx)" style="cursor: pointer" />
              </template>
            </a-input>
            <a-button type="dashed" long @click="addPermission">
              <template #icon>
                <icon-plus />
              </template>
              {{ $t('role.form.permissions.add') }}
            </a-button>
          </a-space>
        </a-form-item>
      </a-form>
    </a-modal>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { Message, Modal } from '@arco-design/web-vue';
import type { TableColumnData } from '@arco-design/web-vue/es/table/interface';
import {
  getRoleList,
  createRole,
  updateRole,
  deleteRole,
  type RoleModel,
  type RoleQueryParams,
} from '@/api/administration/role';
import useLoading from '@/hooks/loading';

const { t } = useI18n();
const { loading, setLoading } = useLoading(true);

// 表格数据
const tableData = ref<RoleModel[]>([]);
const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showTotal: true,
  showPageSize: true,
});

// 搜索表单
const searchForm = reactive<RoleQueryParams>({
  name: '',
  isActive: undefined,
});

// 表格列配置
const columns = computed<TableColumnData[]>(() => [
  {
    title: t('role.columns.name'),
    dataIndex: 'name',
    width: 150,
  },
  {
    title: t('role.columns.displayName'),
    dataIndex: 'displayName',
    width: 150,
  },
  {
    title: t('role.columns.description'),
    dataIndex: 'description',
    ellipsis: true,
    tooltip: true,
  },
  {
    title: t('role.columns.isSystem'),
    dataIndex: 'isSystem',
    slotName: 'isSystem',
    width: 100,
  },
  {
    title: t('role.columns.isActive'),
    dataIndex: 'isActive',
    slotName: 'isActive',
    width: 100,
  },
  {
    title: t('role.columns.permissions'),
    dataIndex: 'permissions',
    slotName: 'permissions',
    width: 280,
    ellipsis: true,
    tooltip: false,
  },
  {
    title: t('role.columns.createdAt'),
    dataIndex: 'createdAt',
    slotName: 'createdAt',
    width: 180,
  },
  {
    title: t('role.columns.operations'),
    slotName: 'operations',
    width: 150,
    fixed: 'right',
  },
]);

// 模态框
const modalVisible = ref(false);
const isEdit = ref(false);
const formRef = ref();
const formData = reactive<any>({
  id: '',
  name: '',
  displayName: '',
  description: '',
  isActive: true,
  permissions: [],
});

// 格式化日期时间
const formatDateTime = (dateStr: string) => {
  if (!dateStr) return '-';
  return new Date(dateStr).toLocaleString('zh-CN');
};

// 加载数据
const fetchData = async () => {
  setLoading(true);
  try {
    const params: RoleQueryParams = {
      ...searchForm,
      page: pagination.current,
      pageSize: pagination.pageSize,
    };
    const response = await getRoleList(params);
    // 响应拦截器已经解包，response.data = { code, message, data: { list, pagination } }
    // 所以response.data.data = { list, pagination }
    const result = response.data;
    if (result && result.list) {
      tableData.value = result.list;
      pagination.total = result.pagination?.total || 0;
    } else {
      tableData.value = [];
      pagination.total = 0;
    }
  } catch (err: any) {
    console.error('Error fetching roles:', err);
    Message.error(err.response?.data?.message || err.message || t('role.message.createFailed'));
    tableData.value = [];
    pagination.total = 0;
  } finally {
    setLoading(false);
  }
};

// 搜索
const search = () => {
  pagination.current = 1;
  fetchData();
};

// 重置
const reset = () => {
  searchForm.name = '';
  searchForm.isActive = undefined;
  search();
};

// 分页变化
const onPageChange = (page: number) => {
  pagination.current = page;
  fetchData();
};

const onPageSizeChange = (pageSize: number) => {
  pagination.pageSize = pageSize;
  fetchData();
};

// 创建角色
const handleCreate = () => {
  isEdit.value = false;
  Object.assign(formData, {
    id: '',
    name: '',
    displayName: '',
    description: '',
    isActive: true,
    permissions: [],
  });
  modalVisible.value = true;
};

// 编辑角色
const handleEdit = (record: RoleModel) => {
  if (record.isSystem) {
    Message.warning(t('role.message.systemRoleCannotModify'));
    return;
  }
  isEdit.value = true;
  Object.assign(formData, {
    ...record,
    permissions: [...record.permissions],
  });
  modalVisible.value = true;
};

// 删除角色
const handleDelete = (record: RoleModel) => {
  if (record.isSystem) {
    Message.warning(t('role.message.systemRoleCannotModify'));
    return;
  }
  
  Modal.confirm({
    title: t('role.message.deleteConfirm'),
    content: `${record.displayName} (${record.name})`,
    onOk: async () => {
      try {
        await deleteRole(record.id!);
        Message.success(t('role.message.deleteSuccess'));
        fetchData();
      } catch (err) {
        Message.error(t('role.message.deleteFailed'));
      }
    },
  });
};

// 提交表单
const handleSubmit = async () => {
  const errors = await formRef.value?.validate();
  if (errors) return;

  try {
    if (isEdit.value) {
      await updateRole(formData.id, {
        displayName: formData.displayName,
        description: formData.description,
        isActive: formData.isActive,
        permissions: formData.permissions.filter((p: string) => p),
      });
      Message.success(t('role.message.updateSuccess'));
    } else {
      await createRole({
        name: formData.name,
        displayName: formData.displayName,
        description: formData.description,
        isActive: formData.isActive,
        permissions: formData.permissions.filter((p: string) => p),
      });
      Message.success(t('role.message.createSuccess'));
    }
    modalVisible.value = false;
    fetchData();
  } catch (err: any) {
    Message.error(
      isEdit.value ? t('role.message.updateFailed') : t('role.message.createFailed')
    );
  }
};

// 取消
const handleCancel = () => {
  modalVisible.value = false;
};

// 添加权限
const addPermission = () => {
  formData.permissions.push('');
};

// 移除权限
const removePermission = (index: number) => {
  formData.permissions.splice(index, 1);
};

// 初始化
fetchData();
</script>

<style scoped lang="less">
  .role-management {
    .container {
      padding: 0 20px 20px 20px;
    }

    .search-card {
      margin-bottom: 20px;
      box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
      border-radius: 8px;

      .search-form {
        width: 100%;

        .form-item-block {
          width: 100%;
          margin-bottom: 0;

          :deep(.arco-form-item-label-col) {
            padding-right: 8px;
          }

          :deep(.arco-input-wrapper),
          :deep(.arco-select-view) {
            width: 100%;
          }
        }

        .action-col {
          display: flex;
          align-items: flex-end;
          justify-content: flex-start;

          :deep(.arco-space) {
            width: 100%;
            flex-wrap: wrap;
          }
        }
      }
    }

    .table-card {
      box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
      border-radius: 8px;
      overflow: hidden;

      :deep(.arco-table-th) {
        background-color: #f7f8fa;
        font-weight: 600;
        color: #1d2129;

        &:last-child {
          .arco-table-th-item-title {
            margin: 0 auto;
          }
        }
      }

      :deep(.arco-table-tr) {
        transition: all 0.3s ease;

        &:hover {
          background-color: #f7f8fa;
          transform: scale(1.002);
        }
      }
    }

    :deep(.arco-form-item-wrapper-col) {
      width: 100%;
    }
  }

  // 权限单元格样式
  .permissions-cell {
    max-width: 100%;
    
    :deep(.arco-space) {
      max-width: 100%;
    }

    :deep(.arco-tag) {
      max-width: 200px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      cursor: default;
    }
  }

  // 权限popover样式
  .permissions-popover {
    .permission-header {
      font-weight: 600;
      font-size: 14px;
      color: var(--color-text-1);
      padding-bottom: 8px;
      margin-bottom: 8px;
      border-bottom: 2px solid var(--color-border-2);
    }

    .permission-item {
      display: flex;
      align-items: center;
      padding: 6px 0;
      font-size: 13px;
      line-height: 1.5;
      color: var(--color-text-2);
      word-break: break-all;

      &:not(:last-child) {
        border-bottom: 1px dashed var(--color-border-2);
      }

      &:hover {
        color: var(--color-text-1);
        background-color: var(--color-fill-2);
        padding-left: 4px;
        padding-right: 4px;
        margin-left: -4px;
        margin-right: -4px;
        border-radius: 2px;
      }
    }
  }

  @media (max-width: 768px) {
    .role-management {
      .search-form {
        .action-col {
          justify-content: flex-start;
          margin-top: 8px;

          :deep(.arco-btn) {
            flex: 1;
            min-width: auto;
          }
        }
      }
    }
  }
</style>
