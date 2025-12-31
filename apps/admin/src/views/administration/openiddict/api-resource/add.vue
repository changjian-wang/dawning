<template>
  <div class="api-resource-add">
    <div class="container">
      <Breadcrumb :items="['认证授权', 'API资源', '新增信息']" />
      <a-form
        ref="formRef"
        class="form"
        layout="horizontal"
        auto-label-width
        :model="form"
      >
        <a-tabs v-model:active-key="activeKey" :default-active-key="1">
          <a-tab-pane key="1" tab="基本信息">
            <a-form-item label="资源名称" field="name">
              <a-input v-model:value="form.name" placeholder="请输入资源名称" />
            </a-form-item>
            <a-form-item label="资源路径" field="displayName">
              <a-input
                v-model:value="form.displayName"
                placeholder="请输入资源路径"
              />
            </a-form-item>
            <a-form-item label="资源描述" field="description">
              <a-input
                v-model:value="form.description"
                placeholder="请输入资源描述"
              />
            </a-form-item>
            <a-form-item label="允许访问令牌签名算法">
              <a-select placeholder="请选择">
                <a-option>ES256</a-option>
                <a-option>ES384</a-option>
                <a-option>ES512</a-option>
                <a-option>PS256</a-option>
                <a-option>PS384</a-option>
                <a-option>PS512</a-option>
                <a-option>RS256</a-option>
                <a-option>RS384</a-option>
                <a-option>RS512</a-option>
              </a-select>
            </a-form-item>
          </a-tab-pane>
          <a-tab-pane key="2" tab="其他信息">
            <!-- 其他信息表单项 -->
          </a-tab-pane>
        </a-tabs>
      </a-form>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { reactive, ref } from 'vue';
  import { FormInstance } from '@arco-design/web-vue';

  const activeKey = ref(1);
  const form = reactive<any>({});
  const formRef = ref<FormInstance | null>(null); // Explicit form reference type
</script>
