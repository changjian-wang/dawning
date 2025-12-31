<template>
  <div class="container">
    <Breadcrumb :items="['menu.administration', 'menu.administration.tenant']" />

    <a-card class="general-card" :title="$t('tenant.list.title')">
      <!-- Search and filter -->
      <a-row :gutter="16" style="margin-bottom: 16px">
        <a-col :span="8">
          <a-input-search
            v-model="searchKeyword"
            :placeholder="$t('tenant.search.placeholder')"
            allow-clear
            @search="handleSearch"
            @clear="handleSearch"
          />
        </a-col>
        <a-col :span="6">
          <a-select
            v-model="filterStatus"
            :placeholder="$t('tenant.filter.status')"
            allow-clear
            @change="handleSearch"
          >
            <a-option :value="undefined">{{ $t('tenant.filter.status.all') }}</a-option>
            <a-option :value="true">{{ $t('tenant.filter.status.active') }}</a-option>
            <a-option :value="false">{{ $t('tenant.filter.status.inactive') }}</a-option>
          </a-select>
        </a-col>
        <a-col :span="10" style="text-align: right">
          <a-button type="primary" size="small" @click="handleCreate">
            <template #icon><icon-plus /></template>
            {{ $t('tenant.button.create') }}
          </a-button>
        </a-col>
      </a-row>

      <!-- Data table -->
      <a-table
        row-key="id"
        :columns="columns"
        :data="tableData"
        :loading="loading"
        :pagination="pagination"
        :bordered="false"
        :stripe="true"
        @page-change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      >
        <template #code="{ record }">
          <a-tag color="blue">{{ record.code }}</a-tag>
        </template>

        <template #domain="{ record }">
          <span v-if="record.domain">{{ record.domain }}</span>
          <span v-else class="text-gray">-</span>
        </template>

        <template #plan="{ record }">
          <a-tag :color="getPlanColor(record.plan)">
            {{ $t(`tenant.plan.${record.plan}`) }}
          </a-tag>
        </template>

        <template #maxUsers="{ record }">
          <span v-if="record.maxUsers">{{ record.maxUsers }}</span>
          <span v-else class="text-gray">{{ $t('tenant.status.unlimited') }}</span>
        </template>

        <template #isActive="{ record }">
          <a-tag v-if="record.isActive" color="arcoblue" size="small">
            <template #icon><icon-check-circle-fill /></template>
            {{ $t('tenant.status.active') }}
          </a-tag>
          <a-tag v-else color="red" size="small">
            <template #icon><icon-close-circle-fill /></template>
            {{ $t('tenant.status.inactive') }}
          </a-tag>
        </template>

        <template #createdAt="{ record }">
          {{ formatDateTime(record.createdAt) }}
        </template>

        <template #actions="{ record }">
          <a-space>
            <a-tooltip :content="$t('tenant.action.view')">
              <a-button type="text" size="small" @click="handleView(record)">
                <template #icon><icon-eye /></template>
              </a-button>
            </a-tooltip>
            <a-tooltip :content="$t('tenant.action.edit')">
              <a-button
                type="text"
                size="small"
                status="warning"
                @click="handleEdit(record)"
              >
                <template #icon><icon-edit /></template>
              </a-button>
            </a-tooltip>
            <a-dropdown>
              <a-button type="text" size="small">
                <template #icon><icon-more /></template>
              </a-button>
              <template #content>
                <a-doption @click="handleToggleActive(record, !record.isActive)">
                  <icon-swap /> {{ record.isActive ? $t('tenant.action.disable') : $t('tenant.action.enable') }}
                </a-doption>
                <a-doption
                  style="color: rgb(var(--red-6))"
                  @click="handleDelete(record)"
                >
                  <icon-delete /> {{ $t('tenant.action.delete') }}
                </a-doption>
              </template>
            </a-dropdown>
          </a-space>
        </template>
      </a-table>
    </a-card>

    <!-- Create/Edit modal -->
    <a-modal
      v-model:visible="modalVisible"
      :title="isEdit ? $t('tenant.modal.edit') : $t('tenant.modal.create')"
      :ok-loading="submitting"
      @ok="handleSubmit"
      @cancel="handleCancel"
    >
      <a-form ref="formRef" :model="formData" :rules="formRules" layout="vertical">
        <a-form-item field="code" :label="$t('tenant.form.code')">
          <a-input
            v-model="formData.code"
            :placeholder="$t('tenant.form.code.placeholder')"
            :disabled="isEdit"
          />
          <template #extra>
            <span class="form-help">{{ $t('tenant.form.code.help') }}</span>
          </template>
        </a-form-item>

        <a-form-item field="name" :label="$t('tenant.form.name')">
          <a-input
            v-model="formData.name"
            :placeholder="$t('tenant.form.name.placeholder')"
          />
        </a-form-item>

        <a-form-item field="description" :label="$t('tenant.form.description')">
          <a-textarea
            v-model="formData.description"
            :placeholder="$t('tenant.form.description.placeholder')"
            :max-length="500"
            show-word-limit
          />
        </a-form-item>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item field="domain" :label="$t('tenant.form.domain')">
              <a-input
                v-model="formData.domain"
                :placeholder="$t('tenant.form.domain.placeholder')"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="plan" :label="$t('tenant.form.plan')">
              <a-select v-model="formData.plan" :placeholder="$t('tenant.form.plan.placeholder')">
                <a-option value="free">{{ $t('tenant.plan.free') }}</a-option>
                <a-option value="basic">{{ $t('tenant.plan.basic') }}</a-option>
                <a-option value="pro">{{ $t('tenant.plan.pro') }}</a-option>
                <a-option value="enterprise">{{ $t('tenant.plan.enterprise') }}</a-option>
              </a-select>
            </a-form-item>
          </a-col>
        </a-row>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item field="email" :label="$t('tenant.form.email')">
              <a-input
                v-model="formData.email"
                :placeholder="$t('tenant.form.email.placeholder')"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="phone" :label="$t('tenant.form.phone')">
              <a-input
                v-model="formData.phone"
                :placeholder="$t('tenant.form.phone.placeholder')"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item field="maxUsers" :label="$t('tenant.form.maxUsers')">
              <a-input-number
                v-model="formData.maxUsers"
                :placeholder="$t('tenant.form.maxUsers.placeholder')"
                :min="0"
                style="width: 100%"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="maxStorageMB" :label="$t('tenant.form.maxStorageMB')">
              <a-input-number
                v-model="formData.maxStorageMB"
                :placeholder="$t('tenant.form.maxStorageMB.placeholder')"
                :min="0"
                style="width: 100%"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-form-item field="isActive" :label="$t('tenant.form.isActive')">
          <a-switch v-model="formData.isActive" />
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- View details modal -->
    <a-modal
      v-model:visible="viewModalVisible"
      :title="$t('tenant.modal.view')"
      :footer="false"
      width="650px"
    >
      <div class="detail-content">
        <div class="detail-row">
          <span class="label">{{ $t('tenant.column.code') }}</span>
          <span class="value">
            <a-tag color="blue">{{ viewRecord?.code }}</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('tenant.column.name') }}</span>
          <span class="value">{{ viewRecord?.name || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('tenant.column.domain') }}</span>
          <span class="value">{{ viewRecord?.domain || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('tenant.column.email') }}</span>
          <span class="value">{{ viewRecord?.email || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('tenant.form.phone') }}</span>
          <span class="value">{{ viewRecord?.phone || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('tenant.column.plan') }}</span>
          <span class="value">
            <a-tag :color="getPlanColor(viewRecord?.plan || 'free')">
              {{ $t(`tenant.plan.${viewRecord?.plan || 'free'}`) }}
            </a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('tenant.column.maxUsers') }}</span>
          <span class="value">{{ viewRecord?.maxUsers || $t('tenant.status.unlimited') }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('tenant.form.maxStorageMB') }}</span>
          <span class="value">{{ viewRecord?.maxStorageMB ? `${viewRecord.maxStorageMB} MB` : $t('tenant.status.unlimited') }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('tenant.column.isActive') }}</span>
          <span class="value">
            <a-tag v-if="viewRecord?.isActive" color="green" size="small">{{ $t('tenant.status.active') }}</a-tag>
            <a-tag v-else color="red" size="small">{{ $t('tenant.status.inactive') }}</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('tenant.column.createdAt') }}</span>
          <span class="value">{{ formatDateTime(viewRecord?.createdAt || '') }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('tenant.form.description') }}</span>
          <span class="value">{{ viewRecord?.description || '-' }}</span>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, computed, onMounted, onActivated } from 'vue';
  import { useI18n } from 'vue-i18n';
  import { Message, Modal } from '@arco-design/web-vue';
  import dayjs from 'dayjs';
  import {
    getTenantList,
    createTenant,
    updateTenant,
    deleteTenant,
    setTenantActive,
    type Tenant,
    type CreateTenantRequest,
    type UpdateTenantRequest,
  } from '@/api/tenant';

  const { t } = useI18n();

  // 表格数据
  const loading = ref(false);
  const tableData = ref<Tenant[]>([]);
  const searchKeyword = ref('');
  const filterStatus = ref<boolean | undefined>(undefined);

  // 分页
  const pagination = reactive({
    current: 1,
    pageSize: 20,
    total: 0,
    showTotal: true,
    showPageSize: true,
  });

  // 表格列定义
  const columns = computed(() => [
    {
      title: t('tenant.column.code'),
      dataIndex: 'code',
      slotName: 'code',
      width: 120,
    },
    {
      title: t('tenant.column.name'),
      dataIndex: 'name',
      width: 150,
    },
    {
      title: t('tenant.column.domain'),
      dataIndex: 'domain',
      slotName: 'domain',
      width: 150,
    },
    {
      title: t('tenant.column.email'),
      dataIndex: 'email',
      width: 180,
      ellipsis: true,
      tooltip: true,
    },
    {
      title: t('tenant.column.plan'),
      dataIndex: 'plan',
      slotName: 'plan',
      width: 100,
    },
    {
      title: t('tenant.column.maxUsers'),
      dataIndex: 'maxUsers',
      slotName: 'maxUsers',
      width: 100,
      align: 'center',
    },
    {
      title: t('tenant.column.isActive'),
      dataIndex: 'isActive',
      slotName: 'isActive',
      width: 80,
      align: 'center',
    },
    {
      title: t('tenant.column.createdAt'),
      dataIndex: 'createdAt',
      slotName: 'createdAt',
      width: 150,
    },
    {
      title: t('common.actions'),
      slotName: 'actions',
      width: 180,
      align: 'center',
      fixed: 'right',
    },
  ]);

  // 弹窗状态
  const modalVisible = ref(false);
  const viewModalVisible = ref(false);
  const viewRecord = ref<Tenant | null>(null);
  const isEdit = ref(false);
  const submitting = ref(false);
  const formRef = ref();
  const editingId = ref<string | null>(null);

  // 表单数据
  const formData = reactive<CreateTenantRequest & { isActive: boolean }>({
    code: '',
    name: '',
    description: '',
    domain: '',
    email: '',
    phone: '',
    plan: 'free',
    maxUsers: undefined,
    maxStorageMB: undefined,
    isActive: true,
  });

  // 表单验证规则
  const formRules = {
    code: [
      { required: true, message: t('tenant.validation.codeRequired') },
      { match: /^[a-z0-9_-]+$/, message: t('tenant.validation.codePattern') },
      { minLength: 2, maxLength: 50, message: t('tenant.validation.codeLength') },
    ],
    name: [
      { required: true, message: t('tenant.validation.nameRequired') },
      { maxLength: 100, message: t('tenant.validation.nameMaxLength') },
    ],
    email: [{ type: 'email', message: t('tenant.validation.emailInvalid') }],
  };

  // 获取订阅计划颜色
  const getPlanColor = (plan: string) => {
    const colors: Record<string, string> = {
      free: 'gray',
      basic: 'green',
      pro: 'blue',
      enterprise: 'purple',
    };
    return colors[plan] || 'gray';
  };

  // 格式化日期时间
  const formatDateTime = (dateStr: string) => {
    return dayjs(dateStr).format('YYYY-MM-DD HH:mm');
  };

  // 获取数据
  const fetchData = async () => {
    loading.value = true;
    try {
      const { data: response } = await getTenantList({
        keyword: searchKeyword.value || undefined,
        isActive: filterStatus.value,
        page: pagination.current,
        pageSize: pagination.pageSize,
      });
      // 拦截器已经解包了 data，response 直接是 {list, pagination}
      tableData.value = response?.list || [];
      pagination.total = response?.pagination?.total || 0;
    } catch (error) {
      Message.error(t('tenant.message.loadFailed'));
    } finally {
      loading.value = false;
    }
  };

  // 搜索
  const handleSearch = () => {
    pagination.current = 1;
    fetchData();
  };

  // 分页变化
  const handlePageChange = (page: number) => {
    pagination.current = page;
    fetchData();
  };

  const handlePageSizeChange = (pageSize: number) => {
    pagination.pageSize = pageSize;
    pagination.current = 1;
    fetchData();
  };

  // 重置表单
  const resetForm = () => {
    formData.code = '';
    formData.name = '';
    formData.description = '';
    formData.domain = '';
    formData.email = '';
    formData.phone = '';
    formData.plan = 'free';
    formData.maxUsers = undefined;
    formData.maxStorageMB = undefined;
    formData.isActive = true;
  };

  // 查看详情
  const handleView = (record: Tenant) => {
    viewRecord.value = record;
    viewModalVisible.value = true;
  };

  // 新建租户
  const handleCreate = () => {
    isEdit.value = false;
    editingId.value = null;
    resetForm();
    modalVisible.value = true;
  };

  // 编辑租户
  const handleEdit = (record: Tenant) => {
    isEdit.value = true;
    editingId.value = record.id;
    formData.code = record.code;
    formData.name = record.name;
    formData.description = record.description || '';
    formData.domain = record.domain || '';
    formData.email = record.email || '';
    formData.phone = record.phone || '';
    formData.plan = record.plan;
    formData.maxUsers = record.maxUsers;
    formData.maxStorageMB = record.maxStorageMB;
    formData.isActive = record.isActive;
    modalVisible.value = true;
  };

  // 提交表单
  const handleSubmit = async () => {
    const valid = await formRef.value?.validate();
    if (valid) return;

    submitting.value = true;
    try {
      if (isEdit.value && editingId.value) {
        const updateData: UpdateTenantRequest = {
          name: formData.name,
          description: formData.description || undefined,
          domain: formData.domain || undefined,
          email: formData.email || undefined,
          phone: formData.phone || undefined,
          plan: formData.plan,
          maxUsers: formData.maxUsers,
          maxStorageMB: formData.maxStorageMB,
          isActive: formData.isActive,
        };
        await updateTenant(editingId.value, updateData);
        Message.success(t('tenant.message.updateSuccess'));
      } else {
        await createTenant(formData);
        Message.success(t('tenant.message.createSuccess'));
      }
      modalVisible.value = false;
      fetchData();
    } catch (error: any) {
      Message.error(error.response?.data?.message || t('tenant.message.operationFailed'));
    } finally {
      submitting.value = false;
    }
  };

  // 取消
  const handleCancel = () => {
    modalVisible.value = false;
    formRef.value?.resetFields();
  };

  // 删除租户
  const handleDelete = async (record: Tenant) => {
    Modal.warning({
      title: t('tenant.modal.delete.title'),
      content: t('tenant.modal.delete.content', { name: record.name }),
      okText: t('tenant.action.delete'),
      cancelText: t('common.cancel'),
      hideCancel: false,
      onOk: async () => {
        try {
          await deleteTenant(record.id);
          Message.success(t('tenant.message.deleteSuccess'));
          fetchData();
        } catch (error) {
          Message.error(t('tenant.message.deleteFailed'));
        }
      },
    });
  };

  // 切换启用状态
  const handleToggleActive = async (record: Tenant, isActive: boolean) => {
    try {
      await setTenantActive(record.id, isActive);
      Message.success(
        isActive ? t('tenant.message.enableSuccess') : t('tenant.message.disableSuccess')
      );
      fetchData();
    } catch (error) {
      Message.error(t('tenant.message.operationFailed'));
    }
  };

  onMounted(() => {
    fetchData();
  });

  // 处理从缓存激活时重新加载数据
  onActivated(() => {
    fetchData();
  });
</script>

<style lang="less" scoped>
  // 表格标题不加粗
  :deep(.arco-table-th) {
    font-weight: normal !important;
  }

  .text-gray {
    color: var(--color-text-3);
  }

  .form-help {
    color: var(--color-text-3);
    font-size: 12px;
  }

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
