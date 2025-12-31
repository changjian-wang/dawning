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
            <a-form-item field="name" :label="$t('apiResource.form.name')" class="form-item-block">
              <a-input
                v-model="searchForm.name"
                :placeholder="$t('apiResource.form.name.placeholder')"
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
              :label="$t('apiResource.form.displayName')"
              class="form-item-block"
            >
              <a-input
                v-model="searchForm.displayName"
                :placeholder="$t('apiResource.form.displayName.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-tag />
                </template>
              </a-input>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item field="enabled" :label="$t('apiResource.form.enabled')" class="form-item-block">
              <a-select
                v-model="searchForm.enabled"
                :placeholder="$t('apiResource.form.enabled.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-check-circle />
                </template>
                <a-option :value="true">{{ $t('apiResource.form.enabled.yes') }}</a-option>
                <a-option :value="false">{{ $t('apiResource.form.enabled.no') }}</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="24" :lg="6" :xl="6" class="action-col">
            <a-space :size="12">
              <a-button type="primary" size="small" @click="handleSearch">
                <template #icon><icon-search /></template>
                {{ $t('apiResource.button.search') }}
              </a-button>
              <a-button size="small" @click="handleReset">
                <template #icon><icon-refresh /></template>
                {{ $t('apiResource.button.reset') }}
              </a-button>
              <a-button type="primary" size="small" status="success" @click="handleCreate">
                <template #icon><icon-plus /></template>
                {{ $t('apiResource.button.add') }}
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
            {{ record.enabled ? $t('apiResource.form.enabled.yes') : $t('apiResource.form.enabled.no') }}
          </a-tag>
        </template>
        <template #optional="{ record }">
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
              :content="$t('apiResource.message.deleteConfirm')"
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
      :width="600"
      :confirm-loading="submitting"
      :ok-button-props="{ disabled: submitting }"
      :cancel-button-props="{ disabled: submitting }"
      @cancel="handleModalCancel"
      @before-ok="handleModalOk"
    >
      <a-form ref="formRef" :model="formData" layout="vertical">
        <a-form-item field="name" :label="$t('apiResource.form.name')" required>
          <a-input
            v-model="formData.name"
            :placeholder="$t('apiResource.form.name.placeholder')"
            :disabled="isEdit"
          />
        </a-form-item>
        <a-form-item field="displayName" :label="$t('apiResource.form.displayName')" required>
          <a-input v-model="formData.displayName" :placeholder="$t('apiResource.form.displayName.placeholder')" />
        </a-form-item>
        <a-form-item field="description" :label="$t('apiResource.form.description')">
          <a-textarea
            v-model="formData.description"
            :placeholder="$t('apiResource.form.description.placeholder')"
            :max-length="500"
          />
        </a-form-item>
        <a-form-item field="enabled" :label="$t('apiResource.form.enabled')">
          <a-switch v-model="formData.enabled" />
        </a-form-item>
        <a-form-item field="showInDiscoveryDocument" :label="$t('apiResource.form.showInDiscoveryDocument')">
          <a-switch v-model="formData.showInDiscoveryDocument" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, computed, onMounted } from 'vue';
  import { useI18n } from 'vue-i18n';
  import { Message } from '@arco-design/web-vue';
  import {
    apiResourceApi,
    IApiResource,
    IApiResourceModel,
  } from '@/api/openiddict/api-resource';

  const { t } = useI18n();

  // Search form
  const searchForm = reactive<IApiResourceModel>({});
  const loading = ref(false);
  const data = ref<IApiResource[]>([]);
  const total = ref(0);
  const pageIndex = ref(1);
  const pageSize = ref(10);

  // Modal
  const modalVisible = ref(false);
  const modalTitle = ref('');
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

  const columns = computed(() => [
    { title: t('apiResource.column.name'), dataIndex: 'name' },
    { title: t('apiResource.column.displayName'), dataIndex: 'displayName' },
    { title: t('apiResource.column.enabled'), slotName: 'enabled', width: 80, align: 'center' },
    { title: t('common.actions'), slotName: 'optional', width: 100, align: 'center' },
  ]);

  // Load data
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
    modalTitle.value = t('apiResource.modal.add');
    modalVisible.value = true;
  }

  function handleEdit(record: IApiResource) {
    isEdit.value = true;
    modalTitle.value = t('apiResource.modal.edit');
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
      Message.warning(t('apiResource.validation.required'));
      done(false);
      return;
    }
    submitting.value = true;
    try {
      if (isEdit.value && formData.id) {
        await apiResourceApi.update(formData.id, formData);
        Message.success(t('apiResource.message.updateSuccess'));
      } else {
        await apiResourceApi.create(formData);
        Message.success(t('apiResource.message.createSuccess'));
      }
      loadData();
      done(true);
    } catch (e: any) {
      Message.error(e?.message || t('apiResource.message.operationFailed'));
      done(false);
    } finally {
      submitting.value = false;
    }
  }

  async function handleDelete(record: IApiResource) {
    await apiResourceApi.remove(record.id);
    Message.success(t('apiResource.message.deleteSuccess'));
    loadData();
  }

  onMounted(loadData);
</script>

<style scoped lang="less"></style>
