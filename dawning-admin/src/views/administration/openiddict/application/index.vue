<template>
  <div class="container">
    <Breadcrumb
      :items="[
        'menu.administration',
        'menu.administration.openiddict',
        'menu.administration.openiddict.application',
      ]"
    />
    <a-card class="general-card" :title="$t('menu.administration.openiddict.application')">
      <!-- 搜索表单 -->
      <a-row :gutter="20" class="search-row">
        <a-col :span="6">
          <a-input
            v-model="searchForm.clientId"
            placeholder="客户端ID"
            allow-clear
          />
        </a-col>
        <a-col :span="6">
          <a-input
            v-model="searchForm.displayName"
            placeholder="显示名称"
            allow-clear
          />
        </a-col>
        <a-col :span="6">
          <a-select
            v-model="searchForm.type"
            placeholder="客户端类型"
            allow-clear
          >
            <a-option value="">全部</a-option>
            <a-option value="confidential">Confidential (机密)</a-option>
            <a-option value="public">Public (公共)</a-option>
          </a-select>
        </a-col>
        <a-col :span="6">
          <a-space>
            <a-button type="primary" @click="handleSearch">
              <template #icon><icon-search /></template>
              查询
            </a-button>
            <a-button @click="handleReset">
              <template #icon><icon-refresh /></template>
              重置
            </a-button>
            <a-button type="primary" @click="handleAdd">
              <template #icon><icon-plus /></template>
              新增
            </a-button>
          </a-space>
        </a-col>
      </a-row>

      <a-divider />

      <!-- 数据表格 -->
      <a-table
        :columns="columns"
        :data="tableData"
        :loading="loading"
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
            {{ record.type === 'confidential' ? '机密' : '公共' }}
          </a-tag>
        </template>

        <template #consentType="{ record }">
          <a-tag>
            {{
              record.consentType === 'explicit'
                ? '显式'
                : record.consentType === 'implicit'
                ? '隐式'
                : '系统'
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

        <template #operations="{ record }">
          <a-space>
            <a-button type="text" size="small" @click="handleView(record)">
              <template #icon><icon-eye /></template>
            </a-button>
            <a-button
              type="text"
              size="small"
              status="warning"
              @click="handleEdit(record)"
            >
              <template #icon><icon-edit /></template>
            </a-button>
            <a-popconfirm
              content="确定要删除此应用程序吗？"
              @ok="handleDelete(record)"
            >
              <a-button type="text" size="small" status="danger">
                <template #icon><icon-delete /></template>
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
      width="800px"
      @ok="handleSubmit"
      @cancel="handleCancel"
    >
      <a-form :model="formData" :rules="formRules" layout="vertical">
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="客户端ID" field="clientId">
              <a-input
                v-model="formData.clientId"
                placeholder="请输入客户端ID"
                :disabled="isEdit"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="显示名称" field="displayName">
              <a-input
                v-model="formData.displayName"
                placeholder="请输入显示名称"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="客户端类型" field="type">
              <a-select v-model="formData.type" placeholder="请选择">
                <a-option value="confidential">Confidential (机密)</a-option>
                <a-option value="public">Public (公共)</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="同意类型" field="consentType">
              <a-select v-model="formData.consentType" placeholder="请选择">
                <a-option value="explicit">Explicit (显式)</a-option>
                <a-option value="implicit">Implicit (隐式)</a-option>
                <a-option value="systematic">Systematic (系统)</a-option>
              </a-select>
            </a-form-item>
          </a-col>
        </a-row>

        <a-form-item
          v-if="formData.type === 'confidential'"
          label="客户端密钥"
          field="clientSecret"
        >
          <a-input-password
            v-model="formData.clientSecret"
            placeholder="请输入客户端密钥"
          />
        </a-form-item>

        <a-form-item label="授权流程" field="permissions">
          <a-checkbox-group v-model="selectedGrantTypes">
            <a-checkbox value="password">密码模式 (Password)</a-checkbox>
            <a-checkbox value="client_credentials"
              >客户端凭证 (Client Credentials)</a-checkbox
            >
            <a-checkbox value="authorization_code"
              >授权码模式 (Authorization Code)</a-checkbox
            >
            <a-checkbox value="refresh_token">刷新令牌 (Refresh Token)</a-checkbox>
          </a-checkbox-group>
        </a-form-item>

        <a-form-item label="作用域 (Scopes)" field="scopes">
          <a-checkbox-group v-model="selectedScopes">
            <a-checkbox value="openid">OpenID</a-checkbox>
            <a-checkbox value="profile">Profile</a-checkbox>
            <a-checkbox value="email">Email</a-checkbox>
            <a-checkbox value="roles">Roles</a-checkbox>
            <a-checkbox value="api">API</a-checkbox>
          </a-checkbox-group>
        </a-form-item>

        <a-form-item label="重定向URI" field="redirectUris">
          <a-textarea
            v-model="redirectUrisText"
            placeholder="每行一个URI，例如：&#10;http://localhost:5173/callback"
            :auto-size="{ minRows: 3, maxRows: 6 }"
          />
        </a-form-item>

        <a-form-item label="登出重定向URI" field="postLogoutRedirectUris">
          <a-textarea
            v-model="postLogoutRedirectUrisText"
            placeholder="每行一个URI，例如：&#10;http://localhost:5173/login"
            :auto-size="{ minRows: 2, maxRows: 4 }"
          />
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- 查看详情对话框 -->
    <a-modal
      v-model:visible="detailVisible"
      title="应用程序详情"
      width="800px"
      :footer="false"
    >
      <a-descriptions :column="2" bordered>
        <a-descriptions-item label="客户端ID">
          {{ currentRecord?.clientId }}
        </a-descriptions-item>
        <a-descriptions-item label="显示名称">
          {{ currentRecord?.displayName }}
        </a-descriptions-item>
        <a-descriptions-item label="客户端类型">
          <a-tag
            :color="currentRecord?.type === 'confidential' ? 'blue' : 'green'"
          >
            {{ currentRecord?.type === 'confidential' ? '机密' : '公共' }}
          </a-tag>
        </a-descriptions-item>
        <a-descriptions-item label="同意类型">
          {{ currentRecord?.consentType }}
        </a-descriptions-item>
        <a-descriptions-item label="创建时间" :span="2">
          {{ formatDateTime(currentRecord?.createdAt) }}
        </a-descriptions-item>
        <a-descriptions-item label="权限列表" :span="2">
          <a-tag
            v-for="(perm, index) in currentRecord?.permissions"
            :key="index"
            style="margin: 2px"
          >
            {{ formatPermission(perm) }}
          </a-tag>
        </a-descriptions-item>
        <a-descriptions-item label="重定向URI" :span="2">
          <div v-for="(uri, index) in currentRecord?.redirectUris" :key="index">
            {{ uri }}
          </div>
        </a-descriptions-item>
        <a-descriptions-item label="登出重定向URI" :span="2">
          <div
            v-for="(uri, index) in currentRecord?.postLogoutRedirectUris"
            :key="index"
          >
            {{ uri }}
          </div>
        </a-descriptions-item>
      </a-descriptions>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, onMounted } from 'vue';
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

  // 表格数据
  const tableData = ref<IApplication[]>([]);
  const loading = ref(false);
  const pagination = reactive({
    current: 1,
    pageSize: 10,
    total: 0,
  });

  // 表格列定义
  const columns = [
    { title: '客户端ID', dataIndex: 'clientId', width: 180 },
    { title: '显示名称', dataIndex: 'displayName', width: 150 },
    { title: '类型', dataIndex: 'type', slotName: 'type', width: 100 },
    {
      title: '同意类型',
      dataIndex: 'consentType',
      slotName: 'consentType',
      width: 100,
    },
    { title: '权限', dataIndex: 'permissions', slotName: 'permissions' },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      slotName: 'createdAt',
      width: 180,
    },
    { title: '操作', slotName: 'operations', width: 150, fixed: 'right' },
  ];

  // 对话框状态
  const modalVisible = ref(false);
  const modalTitle = ref('新增应用程序');
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
  const formRules = {
    clientId: [{ required: true, message: '请输入客户端ID' }],
    displayName: [{ required: true, message: '请输入显示名称' }],
    type: [{ required: true, message: '请选择客户端类型' }],
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
    modalTitle.value = '新增应用程序';
    resetForm();
    modalVisible.value = true;
  };

  // 编辑
  const handleEdit = (record: IApplication) => {
    isEdit.value = true;
    modalTitle.value = '编辑应用程序';
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
        Message.success('更新成功');
      } else {
        await application.create(formData);
        Message.success('创建成功');
      }

      modalVisible.value = false;
      loadData();
    } catch (error) {
      Message.error(isEdit.value ? '更新失败' : '创建失败');
      console.error(error);
    }
  };

  // 取消
  const handleCancel = () => {
    modalVisible.value = false;
    resetForm();
  };

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
    if (perm.startsWith('gt:')) return `授权:${perm.substring(3)}`;
    if (perm.startsWith('ept:')) return `端点:${perm.substring(4)}`;
    if (perm.startsWith('scp:')) return `作用域:${perm.substring(4)}`;
    return perm;
  };

  onMounted(() => {
    loadData();
  });
</script>

<style scoped lang="less">
  .container {
    padding: 20px;
  }

  .search-row {
    margin-bottom: 16px;
  }

  .general-card {
    margin-top: 16px;
  }
</style>
