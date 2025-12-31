<template>
  <div class="permission-management">
    <div class="container">
      <Breadcrumb
        :items="['menu.administration', 'menu.administration.permission']"
      />

      <!-- 搜索卡片 -->
      <a-card class="general-card search-card">
        <a-form :model="searchForm" layout="inline" class="search-form">
          <a-row :gutter="[16, 16]" style="width: 100%">
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item
                field="code"
                :label="$t('permission.form.code')"
                class="form-item-block"
              >
                <a-input
                  v-model="searchForm.code"
                  :placeholder="$t('permission.form.code.placeholder')"
                  allow-clear
                  @press-enter="search"
                >
                  <template #prefix>
                    <icon-safe />
                  </template>
                </a-input>
              </a-form-item>
            </a-col>
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item
                field="isActive"
                :label="$t('permission.columns.isActive')"
                class="form-item-block"
              >
                <a-select
                  v-model="searchForm.isActive"
                  :placeholder="$t('permission.form.status.placeholder')"
                  allow-clear
                >
                  <template #prefix>
                    <icon-check-circle />
                  </template>
                  <a-option :value="true">{{
                    $t('permission.status.active')
                  }}</a-option>
                  <a-option :value="false">{{
                    $t('permission.status.inactive')
                  }}</a-option>
                </a-select>
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
                <a-button type="primary" size="small" @click="search">
                  <template #icon><icon-search /></template>
                  {{ $t('searchTable.form.search') }}
                </a-button>
                <a-button size="small" @click="reset">
                  <template #icon><icon-refresh /></template>
                  {{ $t('searchTable.form.reset') }}
                </a-button>
                <a-button type="primary" size="small" status="success" @click="handleCreate">
                  <template #icon><icon-plus /></template>
                  {{ $t('permission.button.create') }}
                </a-button>
              </a-space>
            </a-col>
          </a-row>
        </a-form>
      </a-card>

      <!-- 表格卡片 -->
      <a-card class="general-card table-card">
        <a-table
          row-key="id"
          :loading="loading"
          :pagination="pagination"
          :columns="columns"
          :data="tableData"
          :bordered="false"
          :stripe="true"
          @page-change="onPageChange"
          @page-size-change="onPageSizeChange"
        >
          <template #code="{ record }">
            <a-tag color="blue">{{ record.code }}</a-tag>
          </template>
          <template #resource="{ record }">
            <a-tag color="arcoblue">{{ record.resource }}</a-tag>
          </template>
          <template #category="{ record }">
            <a-tag :color="getCategoryColor(record.category)">
              {{ record.category }}
            </a-tag>
          </template>
          <template #isSystem="{ record }">
            <a-tag :color="record.isSystem ? 'orange' : 'gray'">
              {{ record.isSystem ? $t('permission.system.yes') : $t('permission.system.no') }}
            </a-tag>
          </template>
          <template #isActive="{ record }">
            <a-tag v-if="record.isActive" color="arcoblue" size="small">
              <template #icon><icon-check-circle-fill /></template>
              {{ $t('permission.status.active') }}
            </a-tag>
            <a-tag v-else color="red" size="small">
              <template #icon><icon-close-circle-fill /></template>
              {{ $t('permission.status.inactive') }}
            </a-tag>
          </template>
          <template #createdAt="{ record }">
            {{ formatDateTime(record.createdAt) }}
          </template>
          <template #actions="{ record }">
            <a-space>
              <a-tooltip :content="$t('common.edit')">
                <a-button
                  type="text"
                  size="small"
                  status="warning"
                  @click="handleEdit(record)"
                >
                  <template #icon><icon-edit /></template>
                </a-button>
              </a-tooltip>
              <a-popconfirm
                :content="$t('common.deleteConfirm')"
                @ok="handleDelete(record)"
              >
                <a-tooltip :content="$t('common.delete')">
                  <a-button
                    type="text"
                    size="small"
                    status="danger"
                  >
                    <template #icon><icon-delete /></template>
                  </a-button>
                </a-tooltip>
              </a-popconfirm>
            </a-space>
          </template>
        </a-table>
      </a-card>

      <!-- 创建/编辑对话框 -->
      <a-modal
        v-model:visible="modalVisible"
        :title="isEdit ? $t('permission.modal.edit') : $t('permission.modal.create')"
        width="600px"
        :ok-loading="submitLoading"
        @before-ok="handleBeforeOk"
        @cancel="handleCancel"
      >
        <a-form ref="formRef" :model="formData" layout="vertical">
          <a-row :gutter="16">
            <a-col :span="12">
              <a-form-item
                field="resource"
                :label="$t('permission.form.resource')"
                :rules="[{ required: true, message: $t('permission.form.resource.required') }]"
              >
                <a-input
                  v-model="formData.resource"
                  :placeholder="$t('permission.form.resource.placeholder')"
                  :disabled="isEdit"
                />
              </a-form-item>
            </a-col>
            <a-col :span="12">
              <a-form-item
                field="action"
                :label="$t('permission.form.action')"
                :rules="[{ required: true, message: $t('permission.form.action.required') }]"
              >
                <a-input
                  v-model="formData.action"
                  :placeholder="$t('permission.form.action.placeholder')"
                  :disabled="isEdit"
                />
              </a-form-item>
            </a-col>
          </a-row>
          <a-form-item
            field="code"
            :label="$t('permission.form.code')"
            :rules="[{ required: true, message: $t('permission.form.code.required') }]"
          >
            <a-input
              v-model="formData.code"
              :placeholder="$t('permission.form.code.format')"
              :disabled="isEdit"
            />
            <template #extra>
              <div style="color: var(--color-text-3)">
                {{ $t('permission.form.code.suggest') }}: {{ formData.resource || 'resource' }}:{{
                  formData.action || 'action'
                }}
              </div>
            </template>
          </a-form-item>
          <a-form-item
            field="name"
            :label="$t('permission.form.name')"
            :rules="[{ required: true, message: $t('permission.form.name.required') }]"
          >
            <a-input v-model="formData.name" :placeholder="$t('permission.form.name.placeholder')" />
          </a-form-item>
          <a-form-item field="description" :label="$t('permission.form.description')">
            <a-textarea
              v-model="formData.description"
              :placeholder="$t('permission.form.description.placeholder')"
              :max-length="500"
              show-word-limit
            />
          </a-form-item>
          <a-row :gutter="16">
            <a-col :span="12">
              <a-form-item
                field="category"
                :label="$t('permission.form.category')"
                :rules="[{ required: true, message: $t('permission.form.category.required') }]"
              >
                <a-select v-model="formData.category" :placeholder="$t('permission.form.category.placeholder')">
                  <a-option value="administration">{{ $t('permission.category.administration') }}</a-option>
                  <a-option value="openiddict">{{ $t('permission.category.openiddict') }}</a-option>
                  <a-option value="system">{{ $t('permission.category.system') }}</a-option>
                </a-select>
              </a-form-item>
            </a-col>
            <a-col :span="12">
              <a-form-item field="displayOrder" :label="$t('permission.form.displayOrder')">
                <a-input-number
                  v-model="formData.displayOrder"
                  :min="0"
                  :max="9999"
                  placeholder="0"
                />
              </a-form-item>
            </a-col>
          </a-row>
          <a-form-item field="isActive" :label="$t('permission.form.isActive')">
            <a-switch v-model="formData.isActive">
              <template #checked>{{ $t('permission.status.active') }}</template>
              <template #unchecked>{{ $t('permission.status.inactive') }}</template>
            </a-switch>
          </a-form-item>
        </a-form>
      </a-modal>

      <!-- 分组查看对话框 -->
      <a-modal
        v-model:visible="groupedModalVisible"
        :title="$t('permission.modal.viewGrouped')"
        width="900px"
        :footer="false"
        @cancel="groupedModalVisible = false"
      >
        <a-spin :loading="groupedLoading" style="width: 100%">
          <a-collapse
            v-if="groupedPermissions.length > 0"
            :default-active-key="[0]"
            expand-icon-position="right"
          >
            <a-collapse-item
              v-for="(group, index) in groupedPermissions"
              :key="index"
              :header="`${group.resource} (${group.permissions.length})`"
            >
              <a-table
                :columns="groupedColumns"
                :data="group.permissions"
                :pagination="false"
                :bordered="false"
                size="small"
              >
                <template #code="{ record }">
                  <a-tag color="blue" size="small">{{ record.code }}</a-tag>
                </template>
                <template #isActive="{ record }">
                  <a-tag v-if="record.isActive" color="green" size="small">
                    {{ $t('permission.status.enabled') }}
                  </a-tag>
                  <a-tag v-else color="red" size="small">{{
                    $t('permission.status.disabled')
                  }}</a-tag>
                </template>
              </a-table>
            </a-collapse-item>
          </a-collapse>
          <a-empty v-else description="暂无权限数据" />
        </a-spin>
      </a-modal>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, computed, onMounted } from 'vue';
  import { useI18n } from 'vue-i18n';
  import { Message, Modal } from '@arco-design/web-vue';
  import {
    getPermissionList,
    getGroupedPermissions,
    getResourceTypes,
    getCategories,
    createPermission,
    updatePermission,
    deletePermission,
    type PermissionModel,
    type PermissionQueryParams,
    type CreatePermissionDto,
    type UpdatePermissionDto,
    type PermissionGroup,
  } from '@/api/administration/permission';
  import dayjs from 'dayjs';

  const { t } = useI18n();

  // Search form
  const searchForm = reactive<PermissionQueryParams>({
    code: '',
    resource: '',
    category: '',
    isActive: undefined,
    page: 1,
    pageSize: 10,
  });

  // Table data
  const loading = ref(false);
  const tableData = ref<PermissionModel[]>([]);
  const pagination = reactive({
    current: 1,
    pageSize: 10,
    total: 0,
    showTotal: true,
    showPageSize: true,
  });

  // Resource options (fetched from backend)
  const resourceOptions = ref<string[]>([]);
  // Category options (fetched from backend)
  const categoryOptions = ref<string[]>([]);

  // Table column definitions
  const columns = computed(() => [
    {
      title: t('permission.columns.code'),
      dataIndex: 'code',
      slotName: 'code',
      width: 180,
    },
    {
      title: t('permission.columns.name'),
      dataIndex: 'name',
      width: 150,
    },
    {
      title: t('permission.columns.resource'),
      dataIndex: 'resource',
      slotName: 'resource',
      width: 120,
    },
    {
      title: t('permission.columns.action'),
      dataIndex: 'action',
      width: 100,
    },
    {
      title: t('permission.columns.category'),
      dataIndex: 'category',
      slotName: 'category',
      width: 120,
    },
    {
      title: t('permission.columns.isSystem'),
      dataIndex: 'isSystem',
      slotName: 'isSystem',
      width: 90,
    },
    {
      title: t('permission.columns.isActive'),
      dataIndex: 'isActive',
      slotName: 'isActive',
      width: 90,
    },
    {
      title: t('common.actions'),
      slotName: 'actions',
      width: 80,
      align: 'center',
      fixed: 'right',
    },
  ]);

  // Grouped view column definitions
  const groupedColumns = [
    {
      title: '代码',
      dataIndex: 'code',
      slotName: 'code',
    },
    {
      title: '名称',
      dataIndex: 'name',
    },
    {
      title: '操作',
      dataIndex: 'action',
    },
    {
      title: '状态',
      dataIndex: 'isActive',
      slotName: 'isActive',
    },
  ];

  // Modal
  const modalVisible = ref(false);
  const isEdit = ref(false);
  const formRef = ref();
  const formData = reactive({
    id: '',
    code: '',
    name: '',
    description: '',
    resource: '',
    action: '',
    category: 'administration',
    isActive: true,
    displayOrder: 0,
  });

  // Grouped view
  const groupedModalVisible = ref(false);
  const groupedLoading = ref(false);
  const groupedPermissions = ref<PermissionGroup[]>([]);

  // Load data
  const fetchData = async () => {
    loading.value = true;
    try {
      const params = {
        ...searchForm,
        page: pagination.current,
        pageSize: pagination.pageSize,
      };
      const result = await getPermissionList(params);
      tableData.value = result.items;
      pagination.total = result.totalCount;
    } catch (error) {
      Message.error('加载权限列表失败');
      console.error(error);
    } finally {
      loading.value = false;
    }
  };

  // Load dropdown options (resource types and categories)
  const loadOptions = async () => {
    try {
      const [resources, categories] = await Promise.all([
        getResourceTypes(),
        getCategories(),
      ]);
      resourceOptions.value = resources;
      categoryOptions.value = categories;
    } catch (error) {
      console.error('加载选项失败', error);
    }
  };

  // Search
  const search = () => {
    pagination.current = 1;
    fetchData();
  };

  // Reset
  const reset = () => {
    Object.assign(searchForm, {
      code: '',
      resource: '',
      category: '',
      isActive: undefined,
    });
    pagination.current = 1;
    fetchData();
  };

  // Pagination
  const onPageChange = (page: number) => {
    pagination.current = page;
    fetchData();
  };

  const onPageSizeChange = (pageSize: number) => {
    pagination.pageSize = pageSize;
    pagination.current = 1;
    fetchData();
  };

  // Create
  const handleCreate = () => {
    isEdit.value = false;
    Object.assign(formData, {
      id: '',
      code: '',
      name: '',
      description: '',
      resource: '',
      action: '',
      category: 'administration',
      isActive: true,
      displayOrder: 0,
    });
    modalVisible.value = true;
  };

  // Edit
  const handleEdit = (record: PermissionModel) => {
    isEdit.value = true;
    Object.assign(formData, record);
    modalVisible.value = true;
  };

  // Delete
  const handleDelete = (record: PermissionModel) => {
    Modal.confirm({
      title: '确认删除',
      content: `确定要删除权限 "${record.name}" (${record.code}) 吗？此操作不可恢复。`,
      okText: '确认',
      cancelText: '取消',
      onOk: async () => {
        try {
          await deletePermission(record.id);
          Message.success('删除成功');
          fetchData();
        } catch (error: any) {
          Message.error(error.response?.data?.message || '删除失败');
        }
      },
    });
  };

  const submitLoading = ref(false);

  // Submit form
  const handleBeforeOk = async (done: (closed: boolean) => void) => {
    const errors = await formRef.value?.validate();
    if (errors) {
      done(false);
      return;
    }

    try {
      submitLoading.value = true;
      if (isEdit.value) {
        const updateData: UpdatePermissionDto = {
          name: formData.name,
          description: formData.description,
          isActive: formData.isActive,
          displayOrder: formData.displayOrder,
        };
        await updatePermission(formData.id, updateData);
        Message.success('更新成功');
      } else {
        const createData: CreatePermissionDto = {
          code: formData.code,
          name: formData.name,
          description: formData.description,
          resource: formData.resource,
          action: formData.action,
          category: formData.category,
          isActive: formData.isActive,
          displayOrder: formData.displayOrder,
        };
        await createPermission(createData);
        Message.success('创建成功');
      }
      done(true);
      fetchData();
    } catch (error: any) {
      Message.error(error.response?.data?.message || '操作失败');
      done(false);
    } finally {
      submitLoading.value = false;
    }
  };

  // Cancel
  const handleCancel = () => {
    modalVisible.value = false;
  };

  // View by resource
  const handleViewGrouped = async () => {
    groupedModalVisible.value = true;
    groupedLoading.value = true;
    try {
      const response = await getGroupedPermissions();
      groupedPermissions.value = response;
    } catch (error) {
      Message.error('加载分组数据失败');
    } finally {
      groupedLoading.value = false;
    }
  };

  // Format date time
  const formatDateTime = (dateStr: string | undefined) => {
    if (!dateStr) return '-';
    return dayjs(dateStr).format('YYYY-MM-DD HH:mm:ss');
  };

  // Get category color
  const getCategoryColor = (category: string) => {
    const colors: Record<string, string> = {
      administration: 'purple',
      openiddict: 'orange',
      system: 'cyan',
    };
    return colors[category] || 'gray';
  };

  // Initialize
  onMounted(() => {
    loadOptions(); // Load dropdown options
    fetchData();
  });
</script>

<style scoped lang="less">
  .permission-management {
    // Table header without bold font
    :deep(.arco-table-th) {
      font-weight: normal !important;
    }

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
</style>
