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
        <a-tabs :default-active-key="1" v-model:active-key="activeKey">
          <a-tab-pane :key="1" title="基本信息">
            <a-card class="general-card">
              <a-row :gutter="80">
                <a-col :span="24">
                  <a-form-item label="显示名称" aria-labelledby="Display Name">
                    <a-input v-model="form.displayName" />
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="描述" aria-labelledby="Description">
                    <a-textarea
                      v-model="form.description"
                      :auto-size="{ minRows: 2, maxRows: 5 }"
                      allow-clear
                    />
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
              <a-row :gutter="80"> </a-row>
            </a-card>
          </a-tab-pane>
          <a-tab-pane :key="3" title="属性">
            <ClientConfig />
          </a-tab-pane>
        </a-tabs>
      </a-form>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import IdentityResource from '@/api/ids/identity-resource';
  import { FormInstance } from '@arco-design/web-vue';
  import { reactive, ref } from 'vue';

  const activeKey = ref(1);
  const form = reactive<IdentityResource>(new IdentityResource());
  const formRef = ref<FormInstance | null>(null); // 明确表单引用类型
  const rules = {};
  const claimTypes = ref([]);

  const handleSubmit = async () => {};
</script>

<style lang="less" scoped></style>
