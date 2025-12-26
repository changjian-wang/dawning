<template>
  <div class="container">
    <Breadcrumb
      :items="[
        'menu.administration',
        'menu.administration.openiddict',
        'menu.administration.openiddict.application',
      ]"
    />
    <a-card class="general-card search-card">
      <a-form :model="searchForm" layout="inline" class="search-form">
        <a-row :gutter="[16, 16]" style="width: 100%">
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item
              field="clientId"
              :label="$t('application.form.clientId')"
              class="form-item-block"
            >
              <a-input
                v-model="searchForm.clientId"
                :placeholder="$t('application.form.clientId.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-code />
                </template>
              </a-input>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item
              field="displayName"
              :label="$t('application.form.displayName')"
              class="form-item-block"
            >
              <a-input
                v-model="searchForm.displayName"
                :placeholder="$t('application.form.displayName.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-apps />
                </template>
              </a-input>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item field="type" :label="$t('application.form.type')" class="form-item-block">
              <a-select
                v-model="searchForm.type"
                :placeholder="$t('application.form.type.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-safe />
                </template>
                <a-option value="">{{ $t('application.form.type.all') }}</a-option>
                <a-option value="confidential">{{ $t('application.form.type.confidential') }}</a-option>
                <a-option value="public">{{ $t('application.form.type.public') }}</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="24" :lg="6" :xl="6" class="action-col">
            <a-space :size="12">
              <a-button type="primary" size="small" @click="handleSearch">
                <template #icon><icon-search /></template>
                {{ $t('application.button.search') }}
              </a-button>
              <a-button size="small" @click="handleReset">
                <template #icon><icon-refresh /></template>
                {{ $t('application.button.reset') }}
              </a-button>
              <a-button type="primary" size="small" status="success" @click="handleAdd">
                <template #icon><icon-plus /></template>
                {{ $t('application.button.add') }}
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
        :stripe="true"
        :bordered="false"
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
        <template #type="{ record }">
          <a-tag :color="record.type === 'confidential' ? 'blue' : 'green'">
            {{ record.type === 'confidential' ? $t('application.form.type.confidential') : $t('application.form.type.public') }}
          </a-tag>
        </template>

        <template #consentType="{ record }">
          <a-tag>
            {{
              record.consentType === 'explicit'
                ? $t('application.consentType.explicit')
                : record.consentType === 'implicit'
                ? $t('application.consentType.implicit')
                : $t('application.consentType.systematic')
            }}
          </a-tag>
        </template>

        <template #permissions="{ record }">
          <a-tag
            v-for="(perm, index) in record.permissions?.slice(0, 2)"
            :key="index"
            size="small"
          >
            {{ formatPermission(perm) }}
          </a-tag>
          <a-tag
            v-if="record.permissions && record.permissions.length > 2"
            size="small"
          >
            +{{ record.permissions.length - 2 }}
          </a-tag>
        </template>

        <template #createdAt="{ record }">
          {{ formatDateTime(record.createdAt) }}
        </template>

        <template #actions="{ record }">
          <a-space>
            <a-tooltip :content="$t('common.detail')">
              <a-button type="text" size="small" @click="handleView(record)">
                <template #icon><icon-eye /></template>
              </a-button>
            </a-tooltip>
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
                <a-button type="text" size="small" status="danger">
                  <template #icon><icon-delete /></template>
                </a-button>
              </a-tooltip>
            </a-popconfirm>
          </a-space>
        </template>
      </a-table>
    </a-card>

    <!-- 新增/编辑对话框 -->
    <a-modal
      v-model:visible="modalVisible"
      :title="modalTitle"
      width="800px"
      @ok="handleSubmit"
      @cancel="handleCancel"
    >
      <a-form :model="formData" :rules="formRules" layout="vertical">
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item :label="$t('application.form.clientId')" field="clientId">
              <a-input
                v-model="formData.clientId"
                :placeholder="$t('application.form.clientId.placeholder')"
                :disabled="isEdit"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item :label="$t('application.form.displayName')" field="displayName">
              <a-input
                v-model="formData.displayName"
                :placeholder="$t('application.form.displayName.placeholder')"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item :label="$t('application.form.clientType')" field="type">
              <a-select v-model="formData.type" :placeholder="$t('application.form.clientType.placeholder')">
                <a-option value="confidential">{{ $t('application.form.type.confidential') }}</a-option>
                <a-option value="public">{{ $t('application.form.type.public') }}</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item :label="$t('application.form.consentType')" field="consentType">
              <a-select v-model="formData.consentType" :placeholder="$t('application.form.consentType.placeholder')">
                <a-option value="explicit">{{ $t('application.consentType.explicit') }}</a-option>
                <a-option value="implicit">{{ $t('application.consentType.implicit') }}</a-option>
                <a-option value="systematic">{{ $t('application.consentType.systematic') }}</a-option>
              </a-select>
            </a-form-item>
          </a-col>
        </a-row>

        <a-form-item
          v-if="formData.type === 'confidential'"
          :label="$t('application.form.clientSecret')"
          field="clientSecret"
        >
          <a-input-password
            v-model="formData.clientSecret"
            :placeholder="$t('application.form.clientSecret.placeholder')"
          />
        </a-form-item>

        <a-form-item :label="$t('application.form.grantTypes')" field="permissions">
          <a-checkbox-group v-model="selectedGrantTypes">
            <a-checkbox value="password">{{ $t('application.form.grantTypes.password') }}</a-checkbox>
            <a-checkbox value="client_credentials">{{ $t('application.form.grantTypes.clientCredentials') }}</a-checkbox>
            <a-checkbox value="authorization_code">{{ $t('application.form.grantTypes.authorizationCode') }}</a-checkbox>
            <a-checkbox value="refresh_token">{{ $t('application.form.grantTypes.refreshToken') }}</a-checkbox>
          </a-checkbox-group>
        </a-form-item>

        <a-form-item :label="$t('application.form.scopes')" field="scopes">
          <a-checkbox-group v-model="selectedScopes">
            <a-checkbox value="openid">OpenID</a-checkbox>
            <a-checkbox value="profile">Profile</a-checkbox>
            <a-checkbox value="email">Email</a-checkbox>
            <a-checkbox value="roles">Roles</a-checkbox>
            <a-checkbox value="api">API</a-checkbox>
          </a-checkbox-group>
        </a-form-item>

        <a-form-item :label="$t('application.form.redirectUris')" field="redirectUris">
          <a-textarea
            v-model="redirectUrisText"
            :placeholder="$t('application.form.redirectUris.placeholder')"
            :auto-size="{ minRows: 3, maxRows: 6 }"
          />
        </a-form-item>

        <a-form-item :label="$t('application.form.postLogoutRedirectUris')" field="postLogoutRedirectUris">
          <a-textarea
            v-model="postLogoutRedirectUrisText"
            :placeholder="$t('application.form.postLogoutRedirectUris.placeholder')"
            :auto-size="{ minRows: 2, maxRows: 4 }"
          />
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- 查看详情对话框 -->
    <a-modal
      v-model:visible="detailVisible"
      :title="$t('application.modal.detail')"
      width="600px"
      :footer="false"
    >
      <div class="detail-content">
        <div class="detail-row">
          <span class="label">{{ $t('application.detail.clientId') }}</span>
          <span class="value">{{ currentRecord?.clientId }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('application.detail.displayName') }}</span>
          <span class="value">{{ currentRecord?.displayName }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('application.detail.clientType') }}</span>
          <span class="value">
            <a-tag
              :color="currentRecord?.type === 'confidential' ? 'blue' : 'green'"
              size="small"
            >
              {{ currentRecord?.type === 'confidential' ? $t('application.form.type.confidential') : $t('application.form.type.public') }}
            </a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('application.detail.consentType') }}</span>
          <span class="value">{{ currentRecord?.consentType }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('application.detail.permissions') }}</span>
          <span class="value">
            <a-space wrap :size="4">
              <a-tag
                v-for="(perm, index) in currentRecord?.permissions"
                :key="index"
                color="purple"
                size="small"
              >
                {{ formatPermission(perm) }}
              </a-tag>
            </a-space>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('application.detail.redirectUris') }}</span>
          <span class="value">
            <div v-if="currentRecord?.redirectUris?.length">
              <div
                v-for="(uri, index) in currentRecord?.redirectUris"
                :key="index"
                style="margin-bottom: 4px"
              >
                {{ uri }}
              </div>
            </div>
            <span v-else>-</span>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('application.detail.postLogoutRedirectUris') }}</span>
          <span class="value">
            <div v-if="currentRecord?.postLogoutRedirectUris?.length">
              <div
                v-for="(uri, index) in currentRecord?.postLogoutRedirectUris"
                :key="index"
                style="margin-bottom: 4px"
              >
                {{ uri }}
              </div>
            </div>
            <span v-else>-</span>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('application.detail.createdAt') }}</span>
          <span class="value">{{
            formatDateTime(currentRecord?.createdAt)
          }}</span>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, onMounted, computed } from 'vue';
  import { useI18n } from 'vue-i18n';
  import { Message } from '@arco-design/web-vue';
  import {
    application,
    type IApplication,
    type IApplicationQuery,
    type ICreateApplicationDto,
  } from '@/api/openiddict/application';
  import { formatDateTime } from '@/utils/date';

  // 搜索表单
  const searchForm = reactive<IApplicationQuery>({
    clientId: '',
    displayName: '',
    type: '',
  });

  const { t } = useI18n();

  // 表格数据
  const tableData = ref<IApplication[]>([]);
  const loading = ref(false);
  const pagination = reactive({
    current: 1,
    pageSize: 10,
    total: 0,
  });

  // 表格列定义
  const columns = computed(() => [
    { title: t('application.column.clientId'), dataIndex: 'clientId', width: 100, ellipsis: true, tooltip: true },
    { title: t('application.column.displayName'), dataIndex: 'displayName', width: 110, ellipsis: true, tooltip: true },
    { title: t('application.column.type'), dataIndex: 'type', slotName: 'type', width: 55 },
    {
      title: t('application.column.consentType'),
      dataIndex: 'consentType',
      slotName: 'consentType',
      width: 65,
    },
    {
      title: t('application.column.permissions'),
      dataIndex: 'permissions',
      slotName: 'permissions',
      width: 200,
    },
    {
      title: t('application.column.createdAt'),
      dataIndex: 'createdAt',
      slotName: 'createdAt',
      width: 145,
    },
    { title: t('common.actions'), slotName: 'actions', width: 100, align: 'center' },
  ]);

  // 对话框状态
  const modalVisible = ref(false);
  const modalTitle = ref('');
  const isEdit = ref(false);
  const detailVisible = ref(false);
  const currentRecord = ref<IApplication | null>(null);

  // 表单数据
  const formData = reactive<ICreateApplicationDto>({
    clientId: '',
    clientSecret: '',
    displayName: '',
    type: 'public',
    consentType: 'implicit',
    permissions: [],
    redirectUris: [],
    postLogoutRedirectUris: [],
  });

  // 选中的授权类型和作用域
  const selectedGrantTypes = ref<string[]>([]);
  const selectedScopes = ref<string[]>([]);
  const redirectUrisText = ref('');
  const postLogoutRedirectUrisText = ref('');

  // 表单验证规则
  const formRules = computed(() => ({
    clientId: [{ required: true, message: t('application.form.clientId.required') }],
    displayName: [{ required: true, message: t('application.form.displayName.required') }],
    type: [{ required: true, message: t('application.form.type.required') }],
  }));

  // ========== 辅助函数（定义在使用之前） ==========

  // 重置表单
  const resetForm = () => {
    formData.clientId = '';
    formData.clientSecret = '';
    formData.displayName = '';
    formData.type = 'public';
    formData.consentType = 'implicit';
    selectedGrantTypes.value = [];
    selectedScopes.value = [];
    redirectUrisText.value = '';
    postLogoutRedirectUrisText.value = '';
  };

  // 构建权限列表
  const buildPermissions = (): string[] => {
    const permissions: string[] = [];

    // 授权类型
    selectedGrantTypes.value.forEach((grant) => {
      permissions.push(`gt:${grant}`);
    });

    // 端点
    permissions.push('ept:token');
    if (selectedGrantTypes.value.includes('authorization_code')) {
      permissions.push('ept:authorization');
    }

    // 作用域
    selectedScopes.value.forEach((scope) => {
      permissions.push(`scp:${scope}`);
    });

    return permissions;
  };

  // 解析权限列表
  const parsePermissions = (permissions: string[]) => {
    selectedGrantTypes.value = [];
    selectedScopes.value = [];

    permissions.forEach((perm) => {
      if (perm.startsWith('gt:')) {
        selectedGrantTypes.value.push(perm.substring(3));
      } else if (perm.startsWith('scp:')) {
        selectedScopes.value.push(perm.substring(4));
      }
    });
  };

  // 格式化权限显示
  const formatPermission = (perm: string): string => {
    if (perm.startsWith('gt:')) return `${t('application.permission.grantType')}:${perm.substring(3)}`;
    if (perm.startsWith('ept:')) return `${t('application.permission.endpoint')}:${perm.substring(4)}`;
    if (perm.startsWith('scp:')) return `${t('application.permission.scope')}:${perm.substring(4)}`;
    return perm;
  };

  // 加载数据
  const loadData = async () => {
    loading.value = true;
    try {
      const result = await application.getPagedList(
        searchForm,
        pagination.current,
        pagination.pageSize
      );
      tableData.value = result.items;
      pagination.total = result.totalCount;
    } catch (error) {
      Message.error(t('application.message.loadFailed'));
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
    searchForm.clientId = '';
    searchForm.displayName = '';
    searchForm.type = '';
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

  // 新增
  const handleAdd = () => {
    isEdit.value = false;
    modalTitle.value = t('application.modal.add');
    resetForm();
    modalVisible.value = true;
  };

  // 编辑
  const handleEdit = (record: IApplication) => {
    isEdit.value = true;
    modalTitle.value = t('application.modal.edit');
    currentRecord.value = record;

    // 填充表单
    formData.clientId = record.clientId;
    formData.displayName = record.displayName || '';
    formData.type = (record.type as any) || 'public';
    formData.consentType = (record.consentType as any) || 'implicit';

    // 解析权限
    parsePermissions(record.permissions || []);

    // URI列表
    redirectUrisText.value = (record.redirectUris || []).join('\n');
    postLogoutRedirectUrisText.value = (
      record.postLogoutRedirectUris || []
    ).join('\n');

    modalVisible.value = true;
  };

  // 查看
  const handleView = (record: IApplication) => {
    currentRecord.value = record;
    detailVisible.value = true;
  };

  // 删除
  const handleDelete = async (record: IApplication) => {
    try {
      if (record.id) {
        await application.delete(record.id);
        Message.success(t('application.message.deleteSuccess'));
        loadData();
      }
    } catch (error) {
      Message.error(t('common.deleteFailed'));
      console.error(error);
    }
  };

  // 提交表单
  const handleSubmit = async () => {
    try {
      // 构建权限列表
      formData.permissions = buildPermissions();

      // 解析URI
      formData.redirectUris = redirectUrisText.value
        .split('\n')
        .filter((uri) => uri.trim());
      formData.postLogoutRedirectUris = postLogoutRedirectUrisText.value
        .split('\n')
        .filter((uri) => uri.trim());

      if (isEdit.value && currentRecord.value?.id) {
        await application.update({
          ...formData,
          id: currentRecord.value.id,
        });
        Message.success(t('application.message.updateSuccess'));
      } else {
        await application.create(formData);
        Message.success(t('application.message.createSuccess'));
      }

      modalVisible.value = false;
      loadData();
    } catch (error) {
      Message.error(isEdit.value ? t('common.updateFailed') : t('common.createFailed'));
      console.error(error);
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
  // 表格标题不加粗
  :deep(.arco-table-th) {
    font-weight: normal !important;
  }

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
        width: 100px;
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
