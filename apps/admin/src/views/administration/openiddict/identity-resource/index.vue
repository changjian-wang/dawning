<template>
  <div class="container">
    <Breadcrumb
      :items="[
        'menu.administration',
        'menu.administration.openiddict',
        'menu.administration.openiddict.identity-resource',
      ]"
    />
    <a-card class="general-card search-card">
      <a-form :model="searchForm" layout="inline" class="search-form">
        <a-row :gutter="[16, 16]" style="width: 100%">
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item field="name" :label="$t('identityResource.form.name')" class="form-item-block">
              <a-input
                v-model="searchForm.name"
                :placeholder="$t('identityResource.form.name.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-idcard />
                </template>
              </a-input>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item
              field="displayName"
              :label="$t('identityResource.form.displayName')"
              class="form-item-block"
            >
              <a-input
                v-model="searchForm.displayName"
                :placeholder="$t('identityResource.form.displayName.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-tag />
                </template>
              </a-input>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
            <a-form-item field="enabled" :label="$t('identityResource.form.enabled')" class="form-item-block">
              <a-select
                v-model="searchForm.enabled"
                :placeholder="$t('identityResource.form.enabled.placeholder')"
                allow-clear
              >
                <template #prefix>
                  <icon-check-circle />
                </template>
                <a-option :value="true">{{ $t('identityResource.form.enabled.yes') }}</a-option>
                <a-option :value="false">{{ $t('identityResource.form.enabled.no') }}</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :xs="24" :sm="12" :md="24" :lg="6" :xl="6" class="action-col">
            <a-space :size="12">
              <a-button type="primary" size="small" @click="handleSearch">
                <template #icon><icon-search /></template>
                {{ $t('identityResource.button.search') }}
              </a-button>
              <a-button size="small" @click="handleReset">
                <template #icon><icon-refresh /></template>
                {{ $t('identityResource.button.reset') }}
              </a-button>
              <a-button type="primary" size="small" status="success" @click="handleCreate">
                <template #icon><icon-plus /></template>
                {{ $t('identityResource.button.add') }}
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
            {{ record.enabled ? $t('identityResource.form.enabled.yes') : $t('identityResource.form.enabled.no') }}
          </a-tag>
        </template>
        <template #required="{ record }">
          <a-tag :color="record.required ? 'orange' : 'gray'">
            {{ record.required ? $t('identityResource.form.enabled.yes') : $t('identityResource.form.enabled.no') }}
          </a-tag>
        </template>
        <template #userClaims="{ record }">
          <a-space wrap>
            <a-tag
              v-for="claim in (record.userClaims || []).slice(0, 3)"
              :key="claim"
              color="purple"
            >
              {{ claim }}
            </a-tag>
            <a-tag v-if="(record.userClaims || []).length > 3" color="gray">
              +{{ record.userClaims.length - 3 }}
            </a-tag>
          </a-space>
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
              :content="$t('identityResource.message.deleteConfirm')"
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
      @cancel="handleModalCancel"
      @before-ok="handleModalOk"
    >
      <a-form ref="formRef" :model="formData" layout="vertical">
        <a-form-item field="name" :label="$t('identityResource.form.name')" required>
          <a-input
            v-model="formData.name"
            :placeholder="$t('identityResource.form.name.placeholder')"
            :disabled="isEdit"
          />
        </a-form-item>
        <a-form-item field="displayName" :label="$t('identityResource.form.displayName')" required>
          <a-input v-model="formData.displayName" :placeholder="$t('identityResource.form.displayName.placeholder')" />
        </a-form-item>
        <a-form-item field="description" :label="$t('identityResource.form.description')">
          <a-textarea
            v-model="formData.description"
            :placeholder="$t('identityResource.form.description.placeholder')"
            :max-length="500"
          />
        </a-form-item>
        <a-row :gutter="16">
          <a-col :span="8">
            <a-form-item field="enabled" :label="$t('identityResource.form.enabled')">
              <a-switch v-model="formData.enabled" />
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item field="required" :label="$t('identityResource.form.required')">
              <a-switch v-model="formData.required" />
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item field="emphasize" :label="$t('identityResource.form.emphasize')">
              <a-switch v-model="formData.emphasize" />
            </a-form-item>
          </a-col>
        </a-row>
        <a-form-item field="showInDiscoveryDocument" :label="$t('identityResource.form.showInDiscoveryDocument')">
          <a-switch v-model="formData.showInDiscoveryDocument" />
        </a-form-item>
        <a-form-item field="userClaims" :label="$t('identityResource.form.userClaims')">
          <a-select
            v-model="formData.userClaims"
            :placeholder="$t('identityResource.form.userClaims.placeholder')"
            multiple
            allow-clear
            :max-tag-count="3"
            style="width: 100%"
          >
            <a-option
              v-for="item in claimTypeOptions"
              :key="item.id"
              :value="item.name"
            >
              {{ item.displayName || item.name }}
            </a-option>
          </a-select>
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
    identityResourceApi,
    IIdentityResource,
    IIdentityResourceModel,
  } from '@/api/openiddict/identity-resource-api';
  import { claimType, IClaimType } from '@/api/administration/claim-type';

  const { t } = useI18n();

  // Claim type options
  const claimTypeOptions = reactive<IClaimType[]>([]);

  const handleGetAllClaimType = async () => {
    claimTypeOptions.splice(0, claimTypeOptions.length);
    const result = await claimType.api.getAll();
    result.forEach((item) => claimTypeOptions.push(item));
  };

  // Search form
  const searchForm = reactive<IIdentityResourceModel>({});
  const loading = ref(false);
  const data = ref<IIdentityResource[]>([]);
  const total = ref(0);
  const pageIndex = ref(1);
  const pageSize = ref(10);

  // Modal
  const modalVisible = ref(false);
  const modalTitle = ref('');
  const isEdit = ref(false);
  const formRef = ref<any>(null);
  const formData = reactive<Partial<IIdentityResource>>({
    name: '',
    displayName: '',
    description: '',
    enabled: true,
    required: false,
    emphasize: false,
    showInDiscoveryDocument: true,
    userClaims: [],
  });

  const columns = computed(() => [
    { title: t('identityResource.column.name'), dataIndex: 'name', width: 120 },
    { title: t('identityResource.column.displayName'), dataIndex: 'displayName', width: 160 },
    { title: t('identityResource.column.userClaims'), slotName: 'userClaims' },
    { title: t('identityResource.column.enabled'), slotName: 'enabled', width: 70, align: 'center' },
    { title: t('identityResource.column.required'), slotName: 'required', width: 100, align: 'center' },
    { title: t('common.actions'), slotName: 'optional', width: 100, align: 'center' },
  ]);

  // Load data
  async function loadData() {
    loading.value = true;
    try {
      const res = await identityResourceApi.getPagedList(
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
    formData.required = false;
    formData.emphasize = false;
    formData.showInDiscoveryDocument = true;
    formData.userClaims = [];
  }

  function handleCreate() {
    resetForm();
    isEdit.value = false;
    modalTitle.value = t('identityResource.modal.add');
    modalVisible.value = true;
  }

  function handleEdit(record: IIdentityResource) {
    isEdit.value = true;
    modalTitle.value = t('identityResource.modal.edit');
    formData.id = record.id;
    formData.name = record.name;
    formData.displayName = record.displayName;
    formData.description = record.description;
    formData.enabled = record.enabled;
    formData.required = record.required;
    formData.emphasize = record.emphasize;
    formData.showInDiscoveryDocument = record.showInDiscoveryDocument;
    formData.userClaims = record.userClaims || [];
    modalVisible.value = true;
  }

  function handleModalCancel() {
    modalVisible.value = false;
  }

  async function handleModalOk(done: (close: boolean) => void) {
    if (!formData.name || !formData.displayName) {
      Message.warning(t('identityResource.validation.required'));
      done(false);
      return;
    }
    try {
      if (isEdit.value && formData.id) {
        await identityResourceApi.update(formData.id, formData);
        Message.success(t('identityResource.message.updateSuccess'));
      } else {
        await identityResourceApi.create(formData);
        Message.success(t('identityResource.message.createSuccess'));
      }
      loadData();
      done(true);
    } catch (e: any) {
      Message.error(e?.message || t('identityResource.message.operationFailed'));
      done(false);
    }
  }

  async function handleDelete(record: IIdentityResource) {
    await identityResourceApi.remove(record.id);
    Message.success(t('identityResource.message.deleteSuccess'));
    loadData();
  }

  onMounted(async () => {
    await handleGetAllClaimType();
    loadData();
  });
</script>

<style scoped lang="less"></style>
