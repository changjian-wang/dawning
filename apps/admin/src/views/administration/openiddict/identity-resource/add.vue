<template>
  <div class="identity-resource-add">
    <div class="container">
      <Breadcrumb :items="['认证授权', '身份资源', '新增信息']" />
      <a-form
        ref="formRef"
        layout="horizontal"
        auto-label-width
        :model="form"
        :rules="rules"
        @submit.prevent="handleSubmit"
      >
        <a-tabs v-model:active-key="activeKey" :default-active-key="1">
          <a-tab-pane :key="1" title="基本信息">
            <a-card class="general-card">
              <a-row :gutter="80">
                <a-col :span="24">
                  <a-form-item
                    field="name"
                    label="资源名称"
                    :rules="[{ required: true, message: '请输入资源名称' }]"
                  >
                    <a-input
                      v-model="form.name"
                      placeholder="请输入资源名称（唯一标识）"
                    />
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item
                    field="displayName"
                    label="显示名称"
                    :rules="[{ required: true, message: '请输入显示名称' }]"
                  >
                    <a-input
                      v-model="form.displayName"
                      placeholder="请输入显示名称"
                    />
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="描述">
                    <a-textarea
                      v-model="form.description"
                      :auto-size="{ minRows: 2, maxRows: 5 }"
                      allow-clear
                      placeholder="请输入描述"
                    />
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="启用">
                    <a-switch v-model="form.enabled">
                      <template #checked-icon>
                        <icon-check />
                      </template>
                      <template #unchecked-icon>
                        <icon-close />
                      </template>
                    </a-switch>
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="是否为必选资源">
                    <a-switch v-model="form.required">
                      <template #checked-icon>
                        <icon-check />
                      </template>
                      <template #unchecked-icon>
                        <icon-close />
                      </template>
                    </a-switch>
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="是否在OIDC发现文档中显示">
                    <a-switch v-model="form.showInDiscoveryDocument">
                      <template #checked-icon>
                        <icon-check />
                      </template>
                      <template #unchecked-icon>
                        <icon-close />
                      </template>
                    </a-switch>
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="是否强调显示该资源">
                    <a-switch v-model="form.emphasize">
                      <template #checked-icon>
                        <icon-check />
                      </template>
                      <template #unchecked-icon>
                        <icon-close />
                      </template>
                    </a-switch>
                  </a-form-item>
                </a-col>
              </a-row>
            </a-card>
          </a-tab-pane>
          <a-tab-pane :key="2" title="声明">
            <a-card class="general-card">
              <a-row :gutter="80">
                <a-col :span="24">
                  <a-form-item label="声明类型">
                    <a-select
                      v-model="form.userClaims"
                      placeholder="请选择声明类型"
                      multiple
                      allow-clear
                      :max-tag-count="5"
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
                </a-col>
              </a-row>
            </a-card>
          </a-tab-pane>
          <a-tab-pane :key="3" title="属性">
            <a-card class="general-card">
              <a-empty description="属性配置（暂未实现）" />
            </a-card>
          </a-tab-pane>
        </a-tabs>
        <div style="margin-top: 20px; text-align: right">
          <a-space>
            <a-button @click="handleCancel">{{ $t('common.cancel') }}</a-button>
            <a-button type="primary" html-type="submit">{{ $t('common.submit') }}</a-button>
          </a-space>
        </div>
      </a-form>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { claimType, IClaimType } from '@/api/administration/claim-type';
  import { identityResourceApi } from '@/api/openiddict/identity-resource-api';
  import { FormInstance, Message } from '@arco-design/web-vue';
  import { reactive, ref, onMounted } from 'vue';
  import { useRouter } from 'vue-router';

  const router = useRouter();
  const activeKey = ref(1);
  const form = reactive<any>({
    name: '',
    displayName: '',
    description: '',
    required: false,
    showInDiscoveryDocument: true,
    emphasize: false,
    enabled: true,
    userClaims: [],
  });
  const formRef = ref<FormInstance | null>(null);
  const rules = {
    name: [{ required: true, message: '请输入资源名称' }],
    displayName: [{ required: true, message: '请输入显示名称' }],
  };
  
  // 声明类型选项
  const claimTypeOptions = reactive<IClaimType[]>([]);

  const handleGetAllClaimType = async () => {
    claimTypeOptions.splice(0, claimTypeOptions.length);
    const result = await claimType.api.getAll();
    result.forEach((item) => claimTypeOptions.push(item));
  };

  const handleSubmit = async () => {
    try {
      const valid = await formRef.value?.validate();
      if (valid) {
        return;
      }
      await identityResourceApi.create(form);
      Message.success('新增成功');
      router.push('/administration/identity-resource');
    } catch (error: any) {
      Message.error(error?.message || '新增失败');
    }
  };

  const handleCancel = () => {
    router.push('/administration/identity-resource');
  };

  onMounted(async () => {
    await handleGetAllClaimType();
  });
</script>

<style lang="less" scoped></style>
