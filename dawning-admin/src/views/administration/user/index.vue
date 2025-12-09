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
              <a-form-item field="username" label="用户名" class="form-item-block">
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
          :data="data"
          :pagination="pagination"
          :bordered="false"
          :loading="loading"
          @page-change="handlePaginationChange"
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
              <a-button type="text" size="medium" @click="handleView(record)">
                <template #icon><icon-eye :size="18" /></template>
              </a-button>
              <a-button
                type="text"
                size="medium"
                status="warning"
                @click="handleEdit(record)"
              >
                <template #icon><icon-edit :size="18" /></template>
              </a-button>
              <a-dropdown>
                <a-button type="text" size="medium">
                  <template #icon><icon-more :size="18" /></template>
                </a-button>
                <template #content>
                  <a-doption @click="handleResetPassword(record)">
                    <icon-refresh /> 重置密码
                  </a-doption>
                  <a-doption @click="handleDelete(record)">
                    <icon-delete /> 删除
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
      @before-ok="handleValidateForm"
      @ok="handleSubmit"
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
      :footer="false"
      width="800px"
    >
      <a-descriptions :data="viewData" :column="2" bordered />
    </a-modal>

    <!-- 重置密码弹窗 -->
    <a-modal
      v-model:visible="resetPasswordVisible"
      title="重置密码"
      @ok="handleResetPasswordSubmit"
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
  </div>
</template>

<script lang="ts" setup>
  import { reactive, ref, onMounted, onUnmounted } from 'vue';
  import {
    IUser,
    IUserModel,
    ICreateUserModel,
    IUpdateUserModel,
    user,
  } from '@/api/administration/user';
  import { FieldRule, PaginationProps, Message } from '@arco-design/web-vue';

  const loading = ref(false);
  const modalVisible = ref(false);
  const viewModalVisible = ref(false);
  const resetPasswordVisible = ref(false);
  const isEdit = ref(false);
  const modalTitle = ref('新增用户');
  const formRef = ref<any>(null);
  const currentUserId = ref<string>('');

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
      width: 180,
      fixed: 'right',
    },
  ]);

  const data = reactive<IUser[]>([]);
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
      const result = await user.api.getPagedList(
        model,
        pagination.current || 1,
        pagination.pageSize || 10
      );

      console.log('API Result:', result);
      console.log('Items:', result.items);

      pagination.total = result.totalCount;
      pagination.current = result.pageIndex;
      pagination.pageSize = result.pageSize;

      data.splice(0, data.length, ...result.items);
      console.log('Data after splice:', data);
    } catch (error) {
      Message.error('加载数据失败');
      console.error('Fetch error:', error);
    } finally {
      loading.value = false;
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
    data.splice(0, data.length);
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
    viewData.value = [
      { label: '用户名', value: record.username },
      { label: '显示名称', value: record.displayName || '-' },
      { label: '邮箱', value: record.email || '-' },
      { label: '手机号', value: record.phoneNumber || '-' },
      { label: '角色', value: record.role },
      { label: '账户状态', value: record.isActive ? '启用' : '禁用' },
      { label: '邮箱已验证', value: record.emailConfirmed ? '是' : '否' },
      { label: '手机已验证', value: record.phoneNumberConfirmed ? '是' : '否' },
      { label: '最后登录', value: record.lastLoginAt || '-' },
      { label: '创建时间', value: record.createdAt || '-' },
      { label: '更新时间', value: record.updatedAt || '-' },
      { label: '备注', value: record.remark || '-' },
    ];
    viewModalVisible.value = true;
  };

  const handleValidateForm = async () => {
    if (await formRef.value.validate()) {
      return false;
    }
    return true;
  };

  const handleSubmit = async () => {
    try {
      if (isEdit.value) {
        await user.api.update(form as IUpdateUserModel);
        Message.success('更新成功');
      } else {
        await user.api.create(form as ICreateUserModel);
        Message.success('创建成功');
      }
      modalVisible.value = false;
      fetchData();
    } catch (error: any) {
      const errorMsg =
        error?.response?.data?.message ||
        (isEdit.value ? '更新失败' : '创建失败');
      Message.error(errorMsg);
      console.error(error);
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

  const handleDelete = (record: IUser) => {
    if (!record.id) return;

    const modal = (window as any).$modal;
    modal.confirm({
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

  const handleResetPasswordSubmit = async () => {
    if (
      !resetPasswordForm.newPassword ||
      resetPasswordForm.newPassword.length < 6
    ) {
      Message.warning('密码至少6个字符');
      return false;
    }

    try {
      await user.api.resetPassword(
        currentUserId.value,
        resetPasswordForm.newPassword
      );
      Message.success('密码重置成功');
      resetPasswordVisible.value = false;
      return true;
    } catch (error: any) {
      const errorMsg = error?.response?.data?.message || '密码重置失败';
      Message.error(errorMsg);
      console.error(error);
      return false;
    }
  };
</script>

<style scoped lang="less">
  .user-management {
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
