<template>
  <div class="container">
    <Breadcrumb
      :items="[
        'menu.administration',
        'menu.administration.openiddict',
        'menu.administration.openiddict.api-resource',
      ]"
    />
    <a-card class="general-card search-card">
      <a-form :model="searchForm" layout="inline" class="search-form">
        <a-row :gutter="[16, 16]" style="width: 100%">
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item field="name" label="名称" class="form-item-block">
              <a-input
                v-model="searchForm.name"
                placeholder="请输入资源名称"
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
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item field="enabled" label="启用" class="form-item-block">
              <a-select
                v-model="searchForm.enabled"
                placeholder="请选择"
                allow-clear
              >
                <template #prefix>
                  <icon-check-circle />
                </template>
                <a-option :value="true">是</a-option>
                <a-option :value="false">否</a-option>
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
              <a-button type="primary" status="success" @click="handleCreate">
                <template #icon><icon-plus /></template>
                新增
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
        <template #enabled="{ record }">
          <a-tag :color="record.enabled ? 'green' : 'gray'">
            {{ record.enabled ? '是' : '否' }}
          </a-tag>
        </template>
        <template #optional="{ record }">
          <a-space>
            <a-button
              type="text"
              size="small"
              status="warning"
              @click="handleEdit(record)"
            >
              <template #icon><icon-edit /></template>
              编辑
            </a-button>
            <a-popconfirm
              content="确定删除该资源吗？"
              @ok="handleDelete(record)"
            >
              <a-button type="text" size="small" status="danger">
                <template #icon><icon-delete /></template>
                删除
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
      :width="600"
      :confirm-loading="submitting"
      :ok-button-props="{ disabled: submitting }"
      :cancel-button-props="{ disabled: submitting }"
      @cancel="handleModalCancel"
      @before-ok="handleModalOk"
    >
      <a-form ref="formRef" :model="formData" layout="vertical">
        <a-form-item field="name" label="名称" required>
          <a-input
            v-model="formData.name"
            placeholder="请输入名称"
            :disabled="isEdit"
          />
        </a-form-item>
        <a-form-item field="displayName" label="显示名" required>
          <a-input v-model="formData.displayName" placeholder="请输入显示名" />
        </a-form-item>
        <a-form-item field="description" label="描述">
          <a-textarea
            v-model="formData.description"
            placeholder="请输入描述"
            :max-length="500"
          />
        </a-form-item>
        <a-form-item field="enabled" label="启用">
          <a-switch v-model="formData.enabled" />
        </a-form-item>
        <a-form-item field="showInDiscoveryDocument" label="显示在发现文档">
          <a-switch v-model="formData.showInDiscoveryDocument" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, onMounted } from 'vue';
  import { Message } from '@arco-design/web-vue';
  import {
    apiResourceApi,
    IApiResource,
    IApiResourceModel,
  } from '@/api/openiddict/api-resource';

  // 搜索表单
  const searchForm = reactive<IApiResourceModel>({});
  const loading = ref(false);
  const data = ref<IApiResource[]>([]);
  const total = ref(0);
  const pageIndex = ref(1);
  const pageSize = ref(10);

  // 弹窗
  const modalVisible = ref(false);
  const modalTitle = ref('新增 API 资源');
  const isEdit = ref(false);
  const formRef = ref<any>(null);
  const formData = reactive<Partial<IApiResource>>({
    name: '',
    displayName: '',
    description: '',
    enabled: true,
    showInDiscoveryDocument: true,
  });
  const submitting = ref(false);

  const columns = [
    { title: '名称', dataIndex: 'name' },
    { title: '显示名', dataIndex: 'displayName' },
    { title: '启用', slotName: 'enabled', width: 80, align: 'center' },
    { title: '操作', slotName: 'optional', width: 140, align: 'center' },
  ];

  // 加载数据
  async function loadData() {
    loading.value = true;
    try {
      const res = await apiResourceApi.getPagedList(
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

  function resetForm() {
    formData.name = '';
    formData.displayName = '';
    formData.description = '';
    formData.enabled = true;
    formData.showInDiscoveryDocument = true;
  }

  function handleCreate() {
    resetForm();
    isEdit.value = false;
    modalTitle.value = '新增 API 资源';
    modalVisible.value = true;
  }

  function handleEdit(record: IApiResource) {
    isEdit.value = true;
    modalTitle.value = '编辑 API 资源';
    formData.id = record.id;
    formData.name = record.name;
    formData.displayName = record.displayName;
    formData.description = record.description;
    formData.enabled = record.enabled;
    formData.showInDiscoveryDocument = record.showInDiscoveryDocument;
    modalVisible.value = true;
  }

  function handleModalCancel() {
    modalVisible.value = false;
  }

  async function handleModalOk(done: (close: boolean) => void) {
    if (submitting.value) {
      done(false);
      return;
    }
    if (!formData.name || !formData.displayName) {
      Message.warning('请填写必填项');
      done(false);
      return;
    }
    submitting.value = true;
    try {
      if (isEdit.value && formData.id) {
        await apiResourceApi.update(formData.id, formData);
        Message.success('更新成功');
      } else {
        await apiResourceApi.create(formData);
        Message.success('新增成功');
      }
      loadData();
      done(true);
    } catch (e: any) {
      Message.error(e?.message || '操作失败');
      done(false);
    } finally {
      submitting.value = false;
    }
  }

  async function handleDelete(record: IApiResource) {
    await apiResourceApi.remove(record.id);
    Message.success('删除成功');
    loadData();
  }

  onMounted(loadData);
</script>

<style scoped lang="less"></style>
