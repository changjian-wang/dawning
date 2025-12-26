<template>
  <div class="user-management">
    <div class="container">
      <Breadcrumb
        :items="['menu.administration', 'menu.administration.user']"
      />
      <a-card class="general-card search-card">
        <a-form :model="model" layout="inline" class="search-form">
          <a-row :gutter="[16, 16]" style="width: 100%">
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item
                field="username"
                :label="$t('user.form.userName')"
                class="form-item-block"
              >
                <a-input
                  v-model="model.username"
                  :placeholder="$t('user.form.userName.placeholder')"
                  allow-clear
                >
                  <template #prefix>
                    <icon-user />
                  </template>
                </a-input>
              </a-form-item>
            </a-col>
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item field="email" :label="$t('user.form.email')" class="form-item-block">
                <a-input
                  v-model="model.email"
                  :placeholder="$t('user.form.email.placeholder')"
                  allow-clear
                >
                  <template #prefix>
                    <icon-email />
                  </template>
                </a-input>
              </a-form-item>
            </a-col>
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item field="role" :label="$t('user.form.role')" class="form-item-block">
                <a-select
                  v-model="model.role"
                  :placeholder="$t('user.form.role.placeholder')"
                  allow-clear
                >
                  <template #prefix>
                    <icon-safe />
                  </template>
                  <a-option value="admin">{{ $t('user.role.admin') }}</a-option>
                  <a-option value="user">{{ $t('user.role.user') }}</a-option>
                  <a-option value="manager">{{ $t('user.role.manager') }}</a-option>
                </a-select>
              </a-form-item>
            </a-col>
            <a-col
              :xs="24"
              :sm="24"
              :md="24"
              :lg="24"
              :xl="24"
              class="action-col"
            >
              <a-space :size="12">
                <a-button type="primary" @click="handleSearch">
                  <template #icon><icon-search /></template>
                  {{ $t('common.search') }}
                </a-button>
                <a-button @click="handleReset">
                  <template #icon><icon-refresh /></template>
                  {{ $t('common.reset') }}
                </a-button>
                <a-button type="primary" status="success" @click="handleAdd">
                  <template #icon><icon-plus /></template>
                  {{ $t('common.add') }}
                </a-button>
                <a-dropdown>
                  <a-button>
                    <template #icon><icon-download /></template>
                    {{ $t('user.button.export') }}
                  </a-button>
                  <template #content>
                    <a-doption @click="handleExport('csv')">
                      <icon-file /> {{ $t('user.button.exportCsv') }}
                    </a-doption>
                    <a-doption @click="handleExport('xlsx')">
                      <icon-file /> {{ $t('user.button.exportExcel') }}
                    </a-doption>
                  </template>
                </a-dropdown>
              </a-space>
            </a-col>
          </a-row>
        </a-form>
      </a-card>
      <a-card class="general-card table-card">
        <!-- 批量操作栏 -->
        <div v-if="selectedRowKeys.length > 0" class="batch-action-bar">
          <a-space>
            <span class="selected-count">
              {{ $t('user.batch.selected', { count: selectedRowKeys.length }) }}
            </span>
            <a-button size="small" @click="handleClearSelection">
              {{ $t('user.batch.clearSelection') }}
            </a-button>
            <a-divider direction="vertical" />
            <a-button
              type="primary"
              status="success"
              size="small"
              @click="handleBatchEnable"
            >
              <template #icon><icon-check-circle /></template>
              {{ $t('user.batch.enable') }}
            </a-button>
            <a-button
              type="primary"
              status="warning"
              size="small"
              @click="handleBatchDisable"
            >
              <template #icon><icon-close-circle /></template>
              {{ $t('user.batch.disable') }}
            </a-button>
            <a-button
              type="primary"
              status="danger"
              size="small"
              @click="handleBatchDelete"
            >
              <template #icon><icon-delete /></template>
              {{ $t('user.batch.delete') }}
            </a-button>
          </a-space>
        </div>
        <a-table
          :columns="columns"
          :data="data"
          :pagination="pagination"
          :bordered="false"
          :stripe="true"
          :loading="loading"
          :row-selection="rowSelection"
          :selected-keys="selectedRowKeys"
          row-key="id"
          @page-change="handlePaginationChange"
          @selection-change="handleSelectionChange"
        >
          <template #isActive="{ record }">
            <a-tag v-if="record.isActive" color="arcoblue" size="small">
              <template #icon><icon-check-circle-fill /></template>
              {{ $t('user.status.enabled') }}
            </a-tag>
            <a-tag v-else color="red" size="small">
              <template #icon><icon-close-circle-fill /></template>
              {{ $t('user.status.disabled') }}
            </a-tag>
          </template>
          <template #createdAt="{ record }">
            {{ formatDateTime(record.createdAt) }}
          </template>
          <template #optional="{ record }">
            <a-space>
              <a-tooltip :content="$t('user.action.view')">
                <a-button type="text" size="small" @click="handleView(record)">
                  <template #icon><icon-eye /></template>
                </a-button>
              </a-tooltip>
              <a-tooltip :content="$t('user.action.edit')">
                <a-button
                  type="text"
                  size="small"
                  status="warning"
                  @click="handleEdit(record)"
                >
                  <template #icon><icon-edit /></template>
                </a-button>
              </a-tooltip>
              <a-tooltip :content="$t('user.action.roles')">
                <a-button
                  type="text"
                  size="small"
                  status="success"
                  @click="handleAssignRoles(record)"
                >
                  <template #icon><icon-user-group /></template>
                </a-button>
              </a-tooltip>
              <a-dropdown>
                <a-button type="text" size="small">
                  <template #icon><icon-more /></template>
                </a-button>
                <template #content>
                  <a-doption @click="handleResetPassword(record)">
                    <icon-refresh /> {{ $t('user.action.resetPassword') }}
                  </a-doption>
                  <a-doption
                    v-if="!isProtectedUser(record)"
                    style="color: rgb(var(--red-6))"
                    @click="handleDelete(record)"
                  >
                    <icon-delete /> {{ $t('user.action.delete') }}
                  </a-doption>
                  <a-doption
                    v-else
                    :disabled="true"
                    style="color: var(--color-text-4); cursor: not-allowed"
                  >
                    <icon-lock /> {{ $t('user.action.systemUser') }}
                  </a-doption>
                </template>
              </a-dropdown>
            </a-space>
          </template>
        </a-table>
      </a-card>
    </div>

    <!-- 新增/编辑用户弹窗 -->
    <a-modal
      v-model:visible="modalVisible"
      :title="modalTitle"
      width="900px"
      :ok-loading="submitLoading"
      @before-ok="handleBeforeOk"
      @cancel="handleModalCancel"
    >
      <a-form ref="formRef" :rules="rules" :model="form" layout="vertical">
        <a-row :gutter="24">
          <a-col :span="12">
            <a-form-item
              field="username"
              :label="$t('user.form.userName')"
              validate-trigger="blur"
            >
              <a-input
                v-model="form.username"
                :placeholder="$t('user.form.userName.placeholder')"
                :disabled="isEdit"
              ></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="email" :label="$t('user.form.email')" validate-trigger="blur">
              <a-input v-model="form.email" :placeholder="$t('user.form.email.placeholder')"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="phoneNumber" :label="$t('user.form.phoneNumber')">
              <a-input
                v-model="form.phoneNumber"
                :placeholder="$t('user.form.phoneNumber.placeholder')"
              ></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="displayName" :label="$t('user.form.displayName')">
              <a-input
                v-model="form.displayName"
                :placeholder="$t('user.form.displayName.placeholder')"
              ></a-input>
            </a-form-item>
          </a-col>
          <a-col v-if="!isEdit" :span="12">
            <a-form-item field="password" :label="$t('user.form.password')" validate-trigger="blur">
              <a-input-password
                v-model="form.password"
                :placeholder="$t('user.form.password.placeholder')"
              ></a-input-password>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="role" :label="$t('user.form.role')">
              <a-select v-model="form.role" :placeholder="$t('user.form.role.placeholder')">
                <a-option value="admin">{{ $t('user.form.role.admin') }}</a-option>
                <a-option value="user">{{ $t('user.form.role.user') }}</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item :label="$t('user.form.accountStatus')">
              <a-switch v-model="form.isActive">
                <template #checked>{{ $t('user.status.enabled') }}</template>
                <template #unchecked>{{ $t('user.status.disabled') }}</template>
              </a-switch>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item field="remark" :label="$t('user.form.remark')">
              <a-textarea
                v-model="form.remark"
                :placeholder="$t('user.form.remark.placeholder')"
                :max-length="200"
                show-word-limit
              ></a-textarea>
            </a-form-item>
          </a-col>
        </a-row>
      </a-form>
    </a-modal>

    <!-- 查看用户详情弹窗 -->
    <a-modal
      v-model:visible="viewModalVisible"
      :title="$t('user.modal.detail.title')"
      width="650px"
      :footer="false"
    >
      <div class="detail-content">
        <div class="detail-row">
          <span class="label">{{ $t('user.form.userName') }}</span>
          <span class="value">{{ currentUser?.username || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.displayName') }}</span>
          <span class="value">{{ currentUser?.displayName || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.email') }}</span>
          <span class="value">{{ currentUser?.email || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.phoneNumber') }}</span>
          <span class="value">{{ currentUser?.phoneNumber || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.role') }}</span>
          <span class="value">{{ currentUser?.role }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.accountStatus') }}</span>
          <span class="value">
            <a-tag v-if="currentUser?.isActive" color="green" size="small"
              >{{ $t('user.status.enabled') }}</a-tag
            >
            <a-tag v-else color="red" size="small">{{ $t('user.status.disabled') }}</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.emailConfirmed') }}</span>
          <span class="value">
            <a-tag
              v-if="currentUser?.emailConfirmed"
              color="arcoblue"
              size="small"
              >{{ $t('user.form.emailConfirmed.yes') }}</a-tag
            >
            <a-tag v-else color="gray" size="small">{{ $t('user.form.emailConfirmed.no') }}</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.phoneNumberConfirmed') }}</span>
          <span class="value">
            <a-tag
              v-if="currentUser?.phoneNumberConfirmed"
              color="arcoblue"
              size="small"
              >{{ $t('user.form.phoneConfirmed.yes') }}</a-tag
            >
            <a-tag v-else color="gray" size="small">{{ $t('user.form.phoneConfirmed.no') }}</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.lastLogin') }}</span>
          <span class="value">{{
            currentUser?.lastLoginAt
              ? formatDateTime(currentUser.lastLoginAt)
              : '-'
          }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.createdAt') }}</span>
          <span class="value">{{
            currentUser?.createdAt ? formatDateTime(currentUser.createdAt) : '-'
          }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.updatedAt') }}</span>
          <span class="value">{{
            currentUser?.updatedAt ? formatDateTime(currentUser.updatedAt) : '-'
          }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('user.form.remark') }}</span>
          <span class="value">{{ currentUser?.remark || '-' }}</span>
        </div>
      </div>
    </a-modal>

    <!-- 重置密码弹窗 -->
    <a-modal
      v-model:visible="resetPasswordVisible"
      :title="$t('user.resetPassword.title')"
      :ok-loading="resetPasswordLoading"
      @before-ok="handleResetPasswordBeforeOk"
    >
      <a-form :model="resetPasswordForm">
        <a-form-item :label="$t('user.resetPassword.newPassword')">
          <a-input-password
            v-model="resetPasswordForm.newPassword"
            :placeholder="$t('user.resetPassword.placeholder')"
          ></a-input-password>
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- 角色分配模态框 -->
    <a-modal
      v-model:visible="roleModalVisible"
      :title="$t('user.role.assignTitle', { username: currentUser?.username })"
      width="800px"
      :mask-closable="false"
      @cancel="handleRoleCancel"
      @before-ok="handleRoleBeforeOk"
    >
      <a-spin :loading="roleLoading" style="width: 100%">
        <div class="role-assignment">
          <a-input-search
            v-model="roleSearchText"
            :placeholder="$t('user.role.searchPlaceholder')"
            allow-clear
            style="margin-bottom: 16px"
          />
          <a-transfer
            v-model="selectedRoleIds"
            :data="allRoles"
            :title="[$t('user.role.available'), $t('user.role.assigned')]"
            :show-search="false"
            @change="handleRoleChange"
          >
            <template #source="{ data: sourceData, selectedKeys, onSelect }">
              <div class="role-list">
                <div
                  v-for="item in sourceData"
                  :key="item.value"
                  class="role-item"
                  @click="
                    onSelect([
                      ...(selectedKeys.includes(item.value)
                        ? selectedKeys.filter((k: string) => k !== item.value)
                        : [...selectedKeys, item.value]),
                    ])
                  "
                >
                  <a-checkbox
                    :model-value="selectedKeys.includes(item.value)"
                  />
                  <div class="role-info">
                    <div class="role-name">{{ item.label }}</div>
                    <div class="role-code">{{ item.name }}</div>
                  </div>
                </div>
              </div>
            </template>
            <template #target="{ data: targetData, selectedKeys, onSelect }">
              <div class="role-list">
                <div
                  v-for="item in targetData"
                  :key="item.value"
                  class="role-item"
                  @click="
                    onSelect([
                      ...(selectedKeys.includes(item.value)
                        ? selectedKeys.filter((k: string) => k !== item.value)
                        : [...selectedKeys, item.value]),
                    ])
                  "
                >
                  <a-checkbox
                    :model-value="selectedKeys.includes(item.value)"
                  />
                  <div class="role-info">
                    <div class="role-name">{{ item.label }}</div>
                    <div class="role-code">{{ item.name }}</div>
                  </div>
                </div>
              </div>
            </template>
          </a-transfer>
        </div>
      </a-spin>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { reactive, ref, onMounted, onUnmounted, computed } from 'vue';
  import { useI18n } from 'vue-i18n';
  import {
    IUser,
    IUserModel,
    ICreateUserModel,
    IUpdateUserModel,
    user,
  } from '@/api/administration/user';
  import { getAllActiveRoles, type RoleModel } from '@/api/administration/role';
  import { FieldRule, PaginationProps, Message, Modal } from '@arco-design/web-vue';
  import {
    exportData,
    formatDateTime,
    formatBoolean,
    type ExportColumn,
  } from '@/utils/export';

  const { t } = useI18n();

  const loading = ref(false);
  const modalVisible = ref(false);
  const viewModalVisible = ref(false);
  const resetPasswordVisible = ref(false);
  const roleModalVisible = ref(false);
  const isEdit = ref(false);
  const modalTitle = ref('');
  const formRef = ref<any>(null);
  const currentUserId = ref<string>('');
  const currentUser = ref<IUser | null>(null);

  // 批量选择相关状态
  const selectedRowKeys = ref<string[]>([]);
  const rowSelection = reactive({
    type: 'checkbox' as const,
    showCheckedAll: true,
    onlyCurrent: false,
  });

  // 判断用户是否是受保护的系统用户
  const isProtectedUser = (record: IUser) => {
    return record.isSystem === true;
  };

  // 角色分配相关状态
  const roleLoading = ref(false);
  const roleSearchText = ref('');
  const allRolesRaw = ref<
    Array<{ value: string; label: string; name: string }>
  >([]);
  const selectedRoleIds = ref<string[]>([]);
  const initialRoleIds = ref<string[]>([]);

  // 过滤后的角色列表
  const allRoles = computed(() => {
    if (!roleSearchText.value) {
      return allRolesRaw.value;
    }
    const keyword = roleSearchText.value.toLowerCase();
    return allRolesRaw.value.filter(
      (role) =>
        role.label.toLowerCase().includes(keyword) ||
        role.name.toLowerCase().includes(keyword)
    );
  });

  const columns = computed(() => [
    {
      title: t('user.column.userName'),
      dataIndex: 'username',
      width: 150,
    },
    {
      title: t('user.column.displayName'),
      dataIndex: 'displayName',
      width: 150,
    },
    {
      title: t('user.column.email'),
      dataIndex: 'email',
      width: 200,
    },
    {
      title: t('user.column.phoneNumber'),
      dataIndex: 'phoneNumber',
      width: 130,
    },
    {
      title: t('user.column.role'),
      dataIndex: 'role',
      width: 100,
    },
    {
      title: t('user.column.isActive'),
      dataIndex: 'isActive',
      slotName: 'isActive',
      width: 100,
    },
    {
      title: t('user.column.createdAt'),
      dataIndex: 'createdAt',
      slotName: 'createdAt',
      width: 180,
    },
    {
      title: t('user.column.action'),
      slotName: 'optional',
      width: 130,
      align: 'center',
      fixed: 'right',
    },
  ]);

  const data = ref<IUser[]>([]);
  const form = reactive<any>({ ...user.form.create() });
  const resetPasswordForm = reactive({ newPassword: '' });

  const rules: Record<string, FieldRule<any> | FieldRule<any>[]> | undefined = {
    username: [
      {
        required: true,
        message: t('user.validation.usernameRequired'),
      },
      {
        minLength: 3,
        message: t('user.validation.usernameMinLength'),
      },
    ],
    password: [
      {
        required: true,
        message: t('user.validation.passwordRequired'),
      },
      {
        minLength: 6,
        message: t('user.validation.passwordMinLength'),
      },
    ],
  };

  const model: IUserModel = reactive({
    username: '',
    email: '',
    role: undefined,
    isActive: undefined,
  });

  const pagination = reactive<PaginationProps>({
    current: 1,
    pageSize: 10,
    total: 0,
    showTotal: true,
  });

  const viewData = ref<any[]>([]);

  const fetchData = async () => {
    loading.value = true;
    try {
      console.log('🔍 Component - Calling API with:', {
        model,
        current: pagination.current || 1,
        pageSize: pagination.pageSize || 10,
      });

      console.log('🔍 Component - Before API call');
      const result = await user.api.getPagedList(
        model,
        pagination.current || 1,
        pagination.pageSize || 10
      );
      console.log('🔍 Component - After API call');

      console.log('🔍 Component - API Result:', result);
      console.log('🔍 Component - result.items:', result.items);
      console.log('🔍 Component - result.totalCount:', result.totalCount);

      pagination.total = result.totalCount;
      pagination.current = result.pageIndex;
      pagination.pageSize = result.pageSize;

      data.value = result.items || [];
      console.log('✅ Component - Data assigned:', data.value);
      console.log('✅ Component - data.value length:', data.value.length);
    } catch (error) {
      console.error('❌ Component - Fetch error:', error);
      console.error(
        '❌ Component - Error stack:',
        error instanceof Error ? error.stack : 'No stack'
      );
      const errorMessage =
        error instanceof Error ? error.message : String(error);
      Message.error(t('user.message.loadFailed', { error: errorMessage }));
    } finally {
      loading.value = false;
      console.log('🔍 Component - Finally block, loading set to false');
    }
  };

  const handlePaginationChange = (page: number) => {
    pagination.current = page;
    fetchData();
  };

  onMounted(async () => {
    fetchData();
  });

  onUnmounted(() => {
    data.value = [];
  });

  const handleAdd = () => {
    isEdit.value = false;
    modalTitle.value = t('user.modal.create.title');
    Object.assign(form, user.form.create());
    modalVisible.value = true;
  };

  const handleEdit = (record: IUser) => {
    isEdit.value = true;
    modalTitle.value = t('user.modal.edit.title');
    Object.assign(form, user.form.clone(record));
    modalVisible.value = true;
  };

  const handleView = (record: IUser) => {
    currentUser.value = record;
    viewModalVisible.value = true;
  };

  const submitLoading = ref(false);

  const handleBeforeOk = async (done: (closed: boolean) => void) => {
    try {
      // 先验证表单
      const errors = await formRef.value?.validate();
      if (errors) {
        // 验证失败，不关闭弹窗
        done(false);
        return;
      }

      submitLoading.value = true;

      // 提交数据
      if (isEdit.value) {
        await user.api.update(form as IUpdateUserModel);
        Message.success(t('user.message.updateSuccess'));
      } else {
        await user.api.create(form as ICreateUserModel);
        Message.success(t('user.message.createSuccess'));
      }

      // 成功后关闭弹窗并刷新数据
      done(true);
      fetchData();
    } catch (error: any) {
      const errorMsg =
        error?.response?.data?.message ||
        (isEdit.value ? t('user.message.updateFailed') : t('user.message.createFailed'));
      Message.error(errorMsg);
      console.error(error);
      // 提交失败，不关闭弹窗
      done(false);
    } finally {
      submitLoading.value = false;
    }
  };

  const handleModalCancel = () => {
    formRef.value?.resetFields();
  };

  const handleSearch = async () => {
    pagination.current = 1;
    fetchData();
  };

  const handleReset = () => {
    model.username = '';
    model.email = '';
    model.role = undefined;
    model.isActive = undefined;
    pagination.current = 1;
    fetchData();
  };

  // 导出用户数据
  const handleExport = async (format: 'csv' | 'xlsx') => {
    try {
      Message.loading({ content: t('user.message.exporting'), id: 'export' });

      // 获取所有数据（或当前筛选条件下的数据）
      const result = await user.api.getPagedList(model, 1, 10000); // 获取最多10000条
      const exportColumns: ExportColumn[] = [
        { field: 'username', title: t('user.export.username') },
        { field: 'email', title: t('user.export.email') },
        { field: 'phoneNumber', title: t('user.export.phoneNumber') },
        { field: 'displayName', title: t('user.export.displayName') },
        {
          field: 'isActive',
          title: t('user.export.status'),
          formatter: (value) => (value ? t('user.export.statusEnabled') : t('user.export.statusDisabled')),
        },
        {
          field: 'createdAt',
          title: t('user.export.createdAt'),
          formatter: (value) => formatDateTime(value),
        },
        {
          field: 'lastLoginAt',
          title: t('user.export.lastLogin'),
          formatter: (value) => formatDateTime(value),
        },
      ];

      exportData({
        filename: `${t('user.export.filename')}_${new Date()
          .toLocaleDateString('zh-CN')
          .replace(/\//g, '-')}`,
        columns: exportColumns,
        data: result.items,
        format,
      });

      Message.success({ content: t('user.message.exportSuccess'), id: 'export' });
    } catch (error: any) {
      Message.error({ content: error?.message || t('user.message.exportFailed'), id: 'export' });
    }
  };

  const handleDelete = (record: IUser) => {
    if (!record.id) return;

    // 保护 admin 用户
    if (isProtectedUser(record)) {
      Message.warning(t('user.delete.adminProtected'));
      return;
    }

    Modal.confirm({
      title: t('user.delete.title'),
      content: t('user.delete.content', { username: record.username }),
      okText: t('user.modal.ok'),
      cancelText: t('user.modal.cancel'),
      onOk: async () => {
        try {
          await user.api.delete(record.id);
          Message.success(t('user.delete.success'));
          fetchData();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.message || t('user.delete.failed');
          Message.error(errorMsg);
          console.error(error);
        }
      },
    });
  };

  const handleResetPassword = (record: IUser) => {
    currentUserId.value = record.id;
    resetPasswordForm.newPassword = '';
    resetPasswordVisible.value = true;
  };

  const resetPasswordLoading = ref(false);

  const handleResetPasswordBeforeOk = async (
    done: (closed: boolean) => void
  ) => {
    if (
      !resetPasswordForm.newPassword ||
      resetPasswordForm.newPassword.length < 6
    ) {
      Message.warning(t('user.resetPassword.minLength'));
      done(false);
      return;
    }

    try {
      resetPasswordLoading.value = true;
      await user.api.resetPassword(
        currentUserId.value,
        resetPasswordForm.newPassword
      );
      Message.success(t('user.resetPassword.success'));
      done(true);
    } catch (error: any) {
      const errorMsg = error?.response?.data?.message || t('user.resetPassword.failed');
      Message.error(errorMsg);
      console.error(error);
      done(false);
    } finally {
      resetPasswordLoading.value = false;
    }
  };

  // 角色分配相关处理函数
  const handleAssignRoles = async (record: IUser) => {
    currentUser.value = record;
    roleLoading.value = true;
    roleModalVisible.value = true;
    roleSearchText.value = ''; // 重置搜索

    try {
      // 加载所有活动角色
      const roles = await getAllActiveRoles();
      allRolesRaw.value = roles.map((role) => ({
        value: role.id!,
        label: role.displayName,
        name: role.name,
      }));

      // 加载用户当前角色
      const userRoles = await user.api.getUserRoles(record.id);
      const userRoleIds = userRoles.map((role: any) => role.id);
      selectedRoleIds.value = [...userRoleIds];
      initialRoleIds.value = [...userRoleIds];
    } catch (error: any) {
      Message.error(t('user.role.loadFailed'));
      console.error(error);
      roleModalVisible.value = false;
    } finally {
      roleLoading.value = false;
    }
  };

  const handleRoleChange = (newTargetKeys: string[]) => {
    selectedRoleIds.value = newTargetKeys;
  };

  const handleRoleBeforeOk = async () => {
    // 检查是否有变化
    const hasChanges =
      selectedRoleIds.value.length !== initialRoleIds.value.length ||
      !selectedRoleIds.value.every((id) => initialRoleIds.value.includes(id));

    if (!hasChanges) {
      Message.info(t('user.role.noChange'));
      return true;
    }

    try {
      await user.api.assignRoles(currentUser.value!.id, selectedRoleIds.value);
      Message.success(t('user.role.assignSuccess'));
      await fetchData();
      return true;
    } catch (error: any) {
      const errorMsg = error?.response?.data?.message || t('user.role.assignFailed');
      Message.error(errorMsg);
      console.error(error);
      return false;
    }
  };

  const handleRoleCancel = () => {
    selectedRoleIds.value = [];
    initialRoleIds.value = [];
    allRolesRaw.value = [];
    roleSearchText.value = '';
    currentUser.value = null;
  };

  // 批量操作相关处理函数
  const handleSelectionChange = (rowKeys: string[]) => {
    selectedRowKeys.value = rowKeys;
  };

  const handleClearSelection = () => {
    selectedRowKeys.value = [];
  };

  const handleBatchDelete = async () => {
    if (selectedRowKeys.value.length === 0) {
      Message.warning(t('user.batch.selectUsersToDelete'));
      return;
    }

    // 过滤掉受保护的 admin 用户
    const protectedUsers = data.value.filter(
      (u: IUser) => selectedRowKeys.value.includes(u.id) && isProtectedUser(u)
    );
    const safeIds = selectedRowKeys.value.filter(
      (id) => !protectedUsers.some((u: IUser) => u.id === id)
    );

    if (protectedUsers.length > 0) {
      Message.warning(t('user.batch.adminProtected', { count: protectedUsers.length }));
    }

    if (safeIds.length === 0) {
      Message.warning(t('user.batch.noUsers'));
      return;
    }

    Modal.confirm({
      title: t('user.batch.deleteTitle'),
      content: t('user.batch.deleteContent', { count: safeIds.length }),
      okText: t('user.modal.ok'),
      cancelText: t('user.modal.cancel'),
      onOk: async () => {
        try {
          loading.value = true;
          const result = await user.api.batchDelete(safeIds);
          if (result.successCount > 0) {
            Message.success(t('user.batch.deleteSuccess', { count: result.successCount }));
          }
          if (result.failedCount > 0) {
            Message.warning(t('user.batch.deleteFailed', { count: result.failedCount }));
          }
          selectedRowKeys.value = [];
          fetchData();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.message || t('user.batch.deleteFailed.all');
          Message.error(errorMsg);
          console.error(error);
        } finally {
          loading.value = false;
        }
      },
    });
  };

  const handleBatchEnable = async () => {
    if (selectedRowKeys.value.length === 0) {
      Message.warning(t('user.batch.selectUsersToEnable'));
      return;
    }

    Modal.confirm({
      title: t('user.batch.enableTitle'),
      content: t('user.batch.enableContent', { count: selectedRowKeys.value.length }),
      okText: t('user.modal.ok'),
      cancelText: t('user.modal.cancel'),
      onOk: async () => {
        try {
          loading.value = true;
          const result = await user.api.batchUpdateStatus(
            selectedRowKeys.value,
            true
          );
          if (result.successCount > 0) {
            Message.success(t('user.batch.enableSuccess', { count: result.successCount }));
          }
          if (result.failedCount > 0) {
            Message.warning(t('user.batch.enableFailed', { count: result.failedCount }));
          }
          selectedRowKeys.value = [];
          fetchData();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.message || t('user.batch.enableFailed.all');
          Message.error(errorMsg);
          console.error(error);
        } finally {
          loading.value = false;
        }
      },
    });
  };

  const handleBatchDisable = async () => {
    if (selectedRowKeys.value.length === 0) {
      Message.warning(t('user.batch.selectUsersToDisable'));
      return;
    }

    // 过滤掉受保护的 admin 用户
    const protectedUsers = data.value.filter(
      (u: IUser) => selectedRowKeys.value.includes(u.id) && isProtectedUser(u)
    );
    const safeIds = selectedRowKeys.value.filter(
      (id) => !protectedUsers.some((u: IUser) => u.id === id)
    );

    if (protectedUsers.length > 0) {
      Message.warning(t('user.batch.adminProtected', { count: protectedUsers.length }));
    }

    if (safeIds.length === 0) {
      Message.warning(t('user.batch.noUsersToDisable'));
      return;
    }

    Modal.confirm({
      title: t('user.batch.disableTitle'),
      content: t('user.batch.disableContent', { count: safeIds.length }),
      okText: t('user.modal.ok'),
      cancelText: t('user.modal.cancel'),
      onOk: async () => {
        try {
          loading.value = true;
          const result = await user.api.batchUpdateStatus(
            safeIds,
            false
          );
          if (result.successCount > 0) {
            Message.success(t('user.batch.disableSuccess', { count: result.successCount }));
          }
          if (result.failedCount > 0) {
            Message.warning(t('user.batch.disableFailed', { count: result.failedCount }));
          }
          selectedRowKeys.value = [];
          fetchData();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.message || t('user.batch.disableFailed.all');
          Message.error(errorMsg);
          console.error(error);
        } finally {
          loading.value = false;
        }
      },
    });
  };
</script>

<style scoped lang="less">
  .user-management {
    .search-card {
      margin-bottom: 16px;
    }

    // 表格标题不加粗
    :deep(.arco-table-th) {
      font-weight: normal !important;
    }

    .batch-action-bar {
      background-color: #e8f4ff;
      padding: 12px 16px;
      margin-bottom: 16px;
      border-radius: 4px;
      border: 1px solid #bedaff;

      .selected-count {
        color: #1d2129;

        strong {
          color: #165dff;
        }
      }
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
        justify-content: flex-start;
        margin-top: 8px;

        :deep(.arco-space) {
          flex-wrap: nowrap;
        }

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
      box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
      border-radius: 8px;
      overflow: hidden;
    }

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

  // 用户表单弹窗样式 - 防止标签换行
  :deep(.arco-modal) {
    .arco-form-item-label {
      white-space: nowrap;
      flex-shrink: 0;
      min-width: 90px;
    }
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

  // 角色分配样式
  .role-assignment {
    :deep(.arco-transfer) {
      .arco-transfer-view {
        width: calc(50% - 25px);
        height: 400px;
      }
    }

    .role-list {
      padding: 8px;
      max-height: 360px;
      overflow-y: auto;

      .role-item {
        display: flex;
        align-items: flex-start;
        padding: 8px;
        margin-bottom: 4px;
        border-radius: 4px;
        cursor: pointer;
        transition: all 0.3s;

        &:hover {
          background-color: var(--color-fill-2);
        }

        .arco-checkbox {
          margin-right: 8px;
          margin-top: 2px;
        }

        .role-info {
          flex: 1;

          .role-name {
            font-size: 14px;
            color: var(--color-text-1);
            margin-bottom: 2px;
          }

          .role-code {
            font-size: 12px;
            color: var(--color-text-3);
            font-family: 'Courier New', monospace;
          }
        }
      }
    }
  }

  @media (max-width: 768px) {
    .user-management {
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
