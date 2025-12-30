<template>
  <div class="claim-type-info">
    <div class="container">
      <Breadcrumb :items="[$t('menu.administration.openiddict'), $t('menu.administration.claim.type'), $t('claimType.info.title')]" />
      <a-form
        ref="formRef"
        class="form"
        layout="horizontal"
        auto-label-width
        :model="form"
      >
        <a-card class="general-card">
          <template #title>
            {{ $t('claimType.info.title') }}
          </template>
          <a-row :gutter="80">
            <a-col :span="8">
              <a-form-item :label="$t('claimType.form.name')">
                <a-input v-model="form.name" :placeholder="$t('claimType.placeholder.input')"></a-input>
              </a-form-item>
            </a-col>
            <a-col :span="8">
              <a-form-item :label="$t('claimType.form.displayName')">
                <a-input
                  v-model="form.displayName"
                  :placeholder="$t('claimType.placeholder.input')"
                ></a-input>
              </a-form-item>
            </a-col>
            <a-col :span="8">
              <a-form-item :label="$t('claimType.form.type')">
                <a-select v-model="form.type" :placeholder="$t('claimType.placeholder.select')">
                  <a-option>String</a-option>
                  <a-option>Int</a-option>
                  <a-option>DateTime</a-option>
                  <a-option>Boolean</a-option>
                  <a-option>Enum</a-option>
                </a-select>
              </a-form-item>
            </a-col>
            <a-col>
              <a-form-item :label="$t('claimType.form.description')">
                <a-textarea
                  v-model="form.description"
                  :placeholder="$t('claimType.placeholder.input')"
                />
              </a-form-item>
            </a-col>
            <a-col>
              <a-form-item :label="$t('claimType.form.required')">
                <a-checkbox v-model="form.required"></a-checkbox>
              </a-form-item>
              <a-form-item :label="$t('claimType.form.nonEditable')">
                <a-checkbox v-model="form.nonEditable"></a-checkbox>
              </a-form-item>
            </a-col>
          </a-row>
        </a-card>
        <a-card class="general-card card-footer">
          <a-space>
            <a-button class="reset">
              {{ $t('common.reset') }}
            </a-button>
            <a-button
              class="submit"
              type="primary"
              :loading="false"
              @click="handleSubmit"
            >
              {{ $t('common.submit') }}
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
  import { claimType, type IClaimType } from '@/api/administration/claim-type';

  const router = useRouter();
  const route = useRoute();
  const id: string = Array.isArray(route.params.id)
    ? route.params.id[0]
    : route.params.id || '';
  const form = reactive<IClaimType>(claimType.form.create());

  onMounted(async () => {
    const response = await claimType.api.get(id);
    Object.assign(form, response);
  });

  const handleSubmit = async () => {
    const result = await claimType.api.update(form);
    if (result) router.push({ name: 'ClaimType' });
  };
</script>

<style lang="less" scoped></style>
