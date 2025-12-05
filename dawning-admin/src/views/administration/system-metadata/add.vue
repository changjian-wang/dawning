<template>
  <div class="system-metadata-info">
    <div class="container">
      <Breadcrumb :items="['系统管理', '元数据', '基本信息']" />
      <a-form ref="formRef" :rules="rules" :model="form">
        <a-card class="general-card">
          <a-row :gutter="80">
            <a-col :span="12">
              <a-form-item field="name" label="名称" validate-trigger="blur">
                <a-input v-model="form.name" placeholder="请输入..."></a-input>
              </a-form-item>
            </a-col>
            <a-col :span="12">
              <a-form-item field="key" label="键名">
                <a-input v-model="form.key" placeholder="请输入..."></a-input>
              </a-form-item>
            </a-col>
            <a-col :span="12">
              <a-form-item field="value" label="有效值">
                <a-input v-model="form.value" placeholder="请输入..."></a-input>
              </a-form-item>
            </a-col>
            <a-col :span="12">
              <a-form-item label="描述">
                <a-textarea
                  v-model="form.description"
                  placeholder="请输入..."
                />
              </a-form-item>
            </a-col>
            <a-col :span="12">
              <a-form-item label="用户可编辑">
                <a-switch v-model="form.nonEditable">
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
        <a-card class="general-card card-footer">
          <a-space>
            <a-button class="reset">
              {{ '重置' }}
            </a-button>
            <a-button
              class="submit"
              type="primary"
              :loading="false"
              @click="handleSubmit"
            >
              {{ '提交' }}
            </a-button>
          </a-space>
        </a-card>
      </a-form>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { onMounted, reactive } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import {
    ISystemMetadata,
    metadata,
  } from '@/api/administration/system-metadata';
  import { FieldRule } from '@arco-design/web-vue';

  const router = useRouter();
  const route = useRoute();
  const id: string = Array.isArray(route.params.id)
    ? route.params.id[0]
    : route.params.id || '';
  const form = reactive<ISystemMetadata>({ ...metadata.form.create() });
  const rules: Record<string, FieldRule<any> | FieldRule<any>[]> | undefined = {
    name: [
      {
        required: true,
        message: '不能为空',
      },
    ],
    key: [
      {
        required: true,
        message: '不能为空',
      },
    ],
    value: [
      {
        required: true,
        message: '不能为空',
      },
    ],
  };

  onMounted(async () => {
    const systemMetadata = await metadata.api.get(id);
    Object.assign(form, { ...systemMetadata });
  });

  const handleSubmit = async () => {
    const result = await metadata.api.update(form);
    if (result) router.push({ name: 'SystemMetadata' });
  };
</script>

<style lang="less" scoped></style>
