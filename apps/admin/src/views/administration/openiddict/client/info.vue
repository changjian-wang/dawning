<template>
  <div class="container">
    <Breadcrumb :items="['menu.administration.openiddict', 'menu.administration.openiddict.client', 'client.info.title']" />

    <a-form
      ref="formRef"
      class="form"
      layout="horizontal"
      auto-label-width
      :model="form"
    >
      <a-card class="general-card">
        <template #title>
          {{ $t('client.info.title') }}
        </template>
        <a-row :gutter="80">
          <a-col :span="24">
            <a-form-item :label="$t('client.form.editable')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.clientId')">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.displayName')">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.description')">
              <a-textarea
                :placeholder="$t('client.form.name.placeholder')"
                :auto-size="{ minRows: 2, maxRows: 5 }"
                allow-clear
              />
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.clientUri')">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.logoUri')">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.requireConsent')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.allowRememberConsent')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.consentLifetime')">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.requireRequestObject')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.requirePkce')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.allowPlainTextPkce')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.alwaysIncludeUserClaimsInIdToken')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.allowAccessTokensViaBrowser')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.allowOfflineAccess')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.updateAccessTokenClaimsOnRefresh')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.alwaysSendClientClaims')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.pairWiseSubjectSalt')">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.userSsoLifetime')">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.userCodeType')">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
        </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ $t('client.info.authLogout') }}
        </template>
        <a-row :gutter="80">
          <a-col :span="24">
            <a-form-item :label="$t('client.form.frontChannelLogoutUri')">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.frontChannelLogoutSessionRequired')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.backChannelLogoutUri')">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item :label="$t('client.form.backChannelLogoutSessionRequired')">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
        </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ $t('client.info.secrets') }}
        </template>
        <a-row :gutter="80">
          <a-col :span="20">
            <a-row :gutter="80">
              <a-col :span="12">
                <a-form-item :label="$t('client.form.secretValue')">
                  <a-input v-model="form.value"></a-input>
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item :label="$t('client.form.secretExpiration')">
                  <a-date-picker v-model="form.value" style="width: 100%" />
                </a-form-item>
              </a-col>
              <a-col :span="24">
                <a-form-item :label="$t('client.form.secretDescription')">
                  <a-textarea
                    :placeholder="$t('client.form.secretDescriptionPlaceholder')"
                    :auto-size="{ minRows: 2, maxRows: 5 }"
                    allow-clear
                  />
                </a-form-item>
              </a-col>
            </a-row>
          </a-col>
          <a-col :span="4">
            <div>
              <a-button type="outline" status="danger">
                <template #icon>
                  <icon-delete />
                </template>
                {{ $t('common.delete') }}
              </a-button>
            </div>
          </a-col>
          <a-col :span="24" style="text-align: center">
            <a-button type="primary">
              <template #icon> <icon-plus /> </template>
              {{ $t('common.add') }}
            </a-button>
          </a-col>
        </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ $t('client.info.resources') }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ $t('client.info.tokens') }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ $t('client.info.claims') }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ $t('client.info.grantTypes') }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ $t('client.info.restrictions') }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ $t('client.info.properties') }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card :style="{ textAlign: 'center' }">
        <a-space>
          <a-button type="primary">{{ $t('common.submit') }}</a-button>
          <a-button type="text">{{ $t('common.reset') }}</a-button>
        </a-space>
      </a-card>
    </a-form>
  </div>
</template>

<script lang="ts" setup>
  import { reactive } from 'vue';

  const form = reactive({
    value: '',
    isTrue: true,
  });
</script>

<style lang="less" scoped></style>
