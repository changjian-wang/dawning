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
                label="用户名"
                class="form-item-block"
              >
                <a-input
                  v-model="model.username"
                  placeholder="请输入用户名"
                  allow-clear
                >
                  <template #prefix>
                    <icon-user />
                  </template>
                </a-input>
              </a-form-item>
            </a-col>
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item field="email" label="邮箱" class="form-item-block">
                <a-input
                  v-model="model.email"
                  placeholder="请输入邮箱"
                  allow-clear
                >
                  <template #prefix>
                    <icon-email />
                  </template>
                </a-input>
              </a-form-item>
            </a-col>
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item field="role" label="角色" class="form-item-block">
                <a-select
                  v-model="model.role"
                  placeholder="请选择角色"
                  allow-clear
                >
                  <template #prefix>
                    <icon-safe />
                  </template>
                  <a-option value="admin">管理员</a-option>
                  <a-option value="user">普通用户</a-option>
                  <a-option value="manager">管理者</a-option>
                </a-select>
              </a-form-item>
            </a-col>
            <a-col
              :xs="24"
              :sm="12"
              :md="24"
              :lg="6"
              :xl="6"
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
                <a-dropdown>
                  <a-button>
                    <template #icon><icon-download /></template>
                    导出
                  </a-button>
                  <template #content>
                    <a-doption @click="handleExport('csv')">
                      <icon-file /> 导出 CSV
                    </a-doption>
                    <a-doption @click="handleExport('xlsx')">
                      <icon-file /> 导出 Excel
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
              已选择 <strong>{{ selectedRowKeys.length }}</strong> 项
            </span>
            <a-button size="small" @click="handleClearSelection">
              取消选择
            </a-button>
            <a-divider direction="vertical" />
            <a-button
              type="primary"
              status="success"
              size="small"
              @click="handleBatchEnable"
            >
              <template #icon><icon-check-circle /></template>
              批量启用
            </a-button>
            <a-button
              type="primary"
              status="warning"
              size="small"
              @click="handleBatchDisable"
            >
              <template #icon><icon-close-circle /></template>
              批量禁用
            </a-button>
            <a-button
              type="primary"
              status="danger"
              size="small"
              @click="handleBatchDelete"
            >
              <template #icon><icon-delete /></template>
              批量删除
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
              启用
            </a-tag>
            <a-tag v-else color="red" size="small">
              <template #icon><icon-close-circle-fill /></template>
              禁用
            </a-tag>
          </template>
          <template #optional="{ record }">
            <a-space>
              <a-button type="text" size="small" @click="handleView(record)">
                <template #icon><icon-eye /></template>
                查看
              </a-button>
              <a-button
                type="text"
                size="small"
                status="warning"
                @click="handleEdit(record)"
              >
                <template #icon><icon-edit /></template>
                编辑
              </a-button>
              <a-button
                type="text"
                size="small"
                status="success"
                @click="handleAssignRoles(record)"
              >
                <template #icon><icon-user-group /></template>
                角色
              </a-button>
              <a-dropdown>
                <a-button type="text" size="small">
                  <template #icon><icon-more /></template>
                </a-button>
                <template #content>
                  <a-doption @click="handleResetPassword(record)">
                    <icon-refresh /> 重置密码
                  </a-doption>
                  <a-doption
                    v-if="!isProtectedUser(record)"
                    style="color: rgb(var(--red-6))"
                    @click="handleDelete(record)"
                  >
                    <icon-delete /> 删除
                  </a-doption>
                  <a-doption
                    v-else
                    :disabled="true"
                    style="color: var(--color-text-4); cursor: not-allowed"
                  >
                    <icon-lock /> 系统用户
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
      width="800px"
      :ok-loading="submitLoading"
      @before-ok="handleBeforeOk"
      @cancel="handleModalCancel"
    >
      <a-form ref="formRef" :rules="rules" :model="form">
        <a-row :gutter="24">
          <a-col :span="12">
            <a-form-item
              field="username"
              label="用户名"
              validate-trigger="blur"
            >
              <a-input
                v-model="form.username"
                placeholder="请输入用户名"
                :disabled="isEdit"
              ></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="email" label="邮箱" validate-trigger="blur">
              <a-input v-model="form.email" placeholder="请输入邮箱"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="phoneNumber" label="手机号">
              <a-input
                v-model="form.phoneNumber"
                placeholder="请输入手机号"
              ></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="displayName" label="显示名称">
              <a-input
                v-model="form.displayName"
                placeholder="请输入显示名称"
              ></a-input>
            </a-form-item>
          </a-col>
          <a-col v-if="!isEdit" :span="12">
            <a-form-item field="password" label="密码" validate-trigger="blur">
              <a-input-password
                v-model="form.password"
                placeholder="请输入密码"
              ></a-input-password>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item field="role" label="角色">
              <a-select v-model="form.role" placeholder="请选择角色">
                <a-option value="admin">管理员</a-option>
                <a-option value="user">普通用户</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="账户状态">
              <a-switch v-model="form.isActive">
                <template #checked>启用</template>
                <template #unchecked>禁用</template>
              </a-switch>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item field="remark" label="备注">
              <a-textarea
                v-model="form.remark"
                placeholder="请输入备注"
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
      title="用户详情"
      width="650px"
      :footer="false"
    >
      <div class="detail-content">
        <div class="detail-row">
          <span class="label">用户名</span>
          <span class="value">{{ currentUser?.username || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">显示名称</span>
          <span class="value">{{ currentUser?.displayName || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">邮箱</span>
          <span class="value">{{ currentUser?.email || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">手机号</span>
          <span class="value">{{ currentUser?.phoneNumber || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">角色</span>
          <span class="value">{{ currentUser?.role }}</span>
        </div>

        <div class="detail-row">
          <span class="label">账户状态</span>
          <span class="value">
            <a-tag v-if="currentUser?.isActive" color="green" size="small"
              >启用</a-tag
            >
            <a-tag v-else color="red" size="small">禁用</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">邮箱已验证</span>
          <span class="value">
            <a-tag
              v-if="currentUser?.emailConfirmed"
              color="arcoblue"
              size="small"
              >是</a-tag
            >
            <a-tag v-else color="gray" size="small">否</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">手机已验证</span>
          <span class="value">
            <a-tag
              v-if="currentUser?.phoneNumberConfirmed"
              color="arcoblue"
              size="small"
              >是</a-tag
            >
            <a-tag v-else color="gray" size="small">否</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">最后登录</span>
          <span class="value">{{
            currentUser?.lastLoginAt
              ? formatDateTime(currentUser.lastLoginAt)
              : '-'
          }}</span>
        </div>

        <div class="detail-row">
          <span class="label">创建时间</span>
          <span class="value">{{
            currentUser?.createdAt ? formatDateTime(currentUser.createdAt) : '-'
          }}</span>
        </div>

        <div class="detail-row">
          <span class="label">更新时间</span>
          <span class="value">{{
            currentUser?.updatedAt ? formatDateTime(currentUser.updatedAt) : '-'
          }}</span>
        </div>

        <div class="detail-row">
          <span class="label">备注</span>
          <span class="value">{{ currentUser?.remark || '-' }}</span>
        </div>
      </div>
    </a-modal>

    <!-- 重置密码弹窗 -->
    <a-modal
      v-model:visible="resetPasswordVisible"
      title="重置密码"
      :ok-loading="resetPasswordLoading"
      @before-ok="handleResetPasswordBeforeOk"
    >
      <a-form :model="resetPasswordForm">
        <a-form-item label="新密码">
          <a-input-password
            v-model="resetPasswordForm.newPassword"
            placeholder="请输入新密码（至少6个字符）"
          ></a-input-password>
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- 角色分配模态框 -->
    <a-modal
      v-model:visible="roleModalVisible"
      :title="`分配角色 - ${currentUser?.username}`"
      width="800px"
      :mask-closable="false"
      @cancel="handleRoleCancel"
      @before-ok="handleRoleBeforeOk"
    >
      <a-spin :loading="roleLoading" style="width: 100%">
        <div class="role-assignment">
          <a-input-search
            v-model="roleSearchText"
            placeholder="搜索角色..."
            allow-clear
            style="margin-bottom: 16px"
          />
          <a-transfer
            v-model="selectedRoleIds"
            :data="allRoles"
            :title="['可分配角色', '已分配角色']"
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

  const loading = ref(false);
  const modalVisible = ref(false);
  const viewModalVisible = ref(false);
  const resetPasswordVisible = ref(false);
  const roleModalVisible = ref(false);
  const isEdit = ref(false);
  const modalTitle = ref('新增用户');
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

  const columns = reactive([
    {
      title: '用户名',
      dataIndex: 'username',
      width: 150,
    },
    {
      title: '显示名称',
      dataIndex: 'displayName',
      width: 150,
    },
    {
      title: '邮箱',
      dataIndex: 'email',
      width: 200,
    },
    {
      title: '手机号',
      dataIndex: 'phoneNumber',
      width: 130,
    },
    {
      title: '角色',
      dataIndex: 'role',
      width: 100,
    },
    {
      title: '账户状态',
      dataIndex: 'isActive',
      slotName: 'isActive',
      width: 100,
    },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      width: 180,
    },
    {
      title: '操作',
      slotName: 'optional',
      width: 200,
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
        message: '用户名不能为空',
      },
      {
        minLength: 3,
        message: '用户名至少3个字符',
      },
    ],
    password: [
      {
        required: true,
        message: '密码不能为空',
      },
      {
        minLength: 6,
        message: '密码至少6个字符',
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
      Message.error(`加载数据失败: ${errorMessage}`);
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
    modalTitle.value = '新增用户';
    Object.assign(form, user.form.create());
    modalVisible.value = true;
  };

  const handleEdit = (record: IUser) => {
    isEdit.value = true;
    modalTitle.value = '编辑用户';
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
        Message.success('更新成功');
      } else {
        await user.api.create(form as ICreateUserModel);
        Message.success('创建成功');
      }

      // 成功后关闭弹窗并刷新数据
      done(true);
      fetchData();
    } catch (error: any) {
      const errorMsg =
        error?.response?.data?.message ||
        (isEdit.value ? '更新失败' : '创建失败');
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
      Message.loading({ content: '正在导出...', id: 'export' });

      // 获取所有数据（或当前筛选条件下的数据）
      const result = await user.api.getPagedList(model, 1, 10000); // 获取最多10000条
      const exportColumns: ExportColumn[] = [
        { field: 'username', title: '用户名' },
        { field: 'email', title: '邮箱' },
        { field: 'phoneNumber', title: '手机号' },
        { field: 'displayName', title: '显示名称' },
        {
          field: 'isActive',
          title: '状态',
          formatter: (value) => (value ? '启用' : '禁用'),
        },
        {
          field: 'createdAt',
          title: '创建时间',
          formatter: (value) => formatDateTime(value),
        },
        {
          field: 'lastLoginAt',
          title: '最后登录',
          formatter: (value) => formatDateTime(value),
        },
      ];

      exportData({
        filename: `用户列表_${new Date()
          .toLocaleDateString('zh-CN')
          .replace(/\//g, '-')}`,
        columns: exportColumns,
        data: result.items,
        format,
      });

      Message.success({ content: '导出成功', id: 'export' });
    } catch (error: any) {
      Message.error({ content: error?.message || '导出失败', id: 'export' });
    }
  };

  const handleDelete = (record: IUser) => {
    if (!record.id) return;

    // 保护 admin 用户
    if (isProtectedUser(record)) {
      Message.warning('系统管理员账号不能删除');
      return;
    }

    Modal.confirm({
      title: '确认删除',
      content: `确定要删除用户 "${record.username}" 吗？此操作不可恢复。`,
      onOk: async () => {
        try {
          await user.api.delete(record.id);
          Message.success('删除成功');
          fetchData();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.message || '删除失败';
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
      Message.warning('密码至少6个字符');
      done(false);
      return;
    }

    try {
      resetPasswordLoading.value = true;
      await user.api.resetPassword(
        currentUserId.value,
        resetPasswordForm.newPassword
      );
      Message.success('密码重置成功');
      done(true);
    } catch (error: any) {
      const errorMsg = error?.response?.data?.message || '密码重置失败';
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
      Message.error('加载角色列表失败');
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
      Message.info('没有变化');
      return true;
    }

    try {
      await user.api.assignRoles(currentUser.value!.id, selectedRoleIds.value);
      Message.success('角色分配成功');
      await fetchData();
      return true;
    } catch (error: any) {
      const errorMsg = error?.response?.data?.message || '角色分配失败';
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
      Message.warning('请先选择要删除的用户');
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
      Message.warning(`系统管理员账号不能删除，已自动排除 ${protectedUsers.length} 个`);
    }

    if (safeIds.length === 0) {
      Message.warning('没有可删除的用户');
      return;
    }

    Modal.confirm({
      title: '批量删除确认',
      content: `确定要删除选中的 ${safeIds.length} 个用户吗？此操作不可恢复。`,
      onOk: async () => {
        try {
          loading.value = true;
          const result = await user.api.batchDelete(safeIds);
          if (result.successCount > 0) {
            Message.success(`成功删除 ${result.successCount} 个用户`);
          }
          if (result.failedCount > 0) {
            Message.warning(`${result.failedCount} 个用户删除失败`);
          }
          selectedRowKeys.value = [];
          fetchData();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.message || '批量删除失败';
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
      Message.warning('请先选择要启用的用户');
      return;
    }

    Modal.confirm({
      title: '批量启用确认',
      content: `确定要启用选中的 ${selectedRowKeys.value.length} 个用户吗？`,
      onOk: async () => {
        try {
          loading.value = true;
          const result = await user.api.batchUpdateStatus(
            selectedRowKeys.value,
            true
          );
          if (result.successCount > 0) {
            Message.success(`成功启用 ${result.successCount} 个用户`);
          }
          if (result.failedCount > 0) {
            Message.warning(`${result.failedCount} 个用户启用失败`);
          }
          selectedRowKeys.value = [];
          fetchData();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.message || '批量启用失败';
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
      Message.warning('请先选择要禁用的用户');
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
      Message.warning(`系统管理员账号不能禁用，已自动排除 ${protectedUsers.length} 个`);
    }

    if (safeIds.length === 0) {
      Message.warning('没有可禁用的用户');
      return;
    }

    Modal.confirm({
      title: '批量禁用确认',
      content: `确定要禁用选中的 ${safeIds.length} 个用户吗？`,
      onOk: async () => {
        try {
          loading.value = true;
          const result = await user.api.batchUpdateStatus(
            safeIds,
            false
          );
          if (result.successCount > 0) {
            Message.success(`成功禁用 ${result.successCount} 个用户`);
          }
          if (result.failedCount > 0) {
            Message.warning(`${result.failedCount} 个用户禁用失败`);
          }
          selectedRowKeys.value = [];
          fetchData();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.message || '批量禁用失败';
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
        justify-content: flex-end;

        :deep(.arco-space) {
          flex-wrap: wrap;
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
