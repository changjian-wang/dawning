<template>
  <div class="container">
    <Breadcrumb :items="['认证授权', '客户端', '基本信息']" />

    <a-form ref="formRef" class="form" layout="horizontal" auto-label-width :model="form">
      <a-card class="general-card">
        <template #title>
          {{ '基本信息' }}
        </template>
        <a-row :gutter="80">
          <a-col :span="24">
            <a-form-item label="可编辑">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item label="客户端ID" aria-labelledby="ClientID">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item label="客户端显示名称" aria-labelledby="ClientName">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item label="描述" aria-labelledby="Description">
              <a-textarea
                placeholder="Please enter something"
                :auto-size="{ minRows: 2, maxRows: 5 }"
                allow-clear
              />
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item label="客户端URI" aria-labelledby="ClientUri">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item label="商标URI" aria-labelledby="LogoUri">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item label="需要同意弹窗" aria-labelledby="RequireConsent">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="允许记住同意"
              aria-labelledby="AllowRememberConsent"
            >
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item label="同意有效期" aria-labelledby="ConsentLifetime">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="需要请求对象"
              aria-labelledby="RequireRequestObject"
            >
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item label="需要PKCE" aria-labelledby="RequirePkce">
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="允许明文PKCE"
              aria-labelledby="AllowPlainTextPkce"
            >
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="ID令牌中始终包含用户声明"
              aria-labelledby="AlwaysIncludeUserClaimsInIdToken"
            >
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="允许通过浏览器接收访问令牌"
              aria-labelledby="AllowAccessTokensViaBrowser"
            >
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="允许离线访问"
              aria-labelledby="AllowOfflineAccess"
            >
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="刷新令牌时更新访问令牌声明"
              aria-labelledby="UpdateAccessTokenClaimsOnRefresh"
            >
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="始终发送客户端声明"
              aria-labelledby="AlwaysSendClientClaims"
            >
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="配对主题盐"
              aria-labelledby="PairWiseSubjectSalt"
            >
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="单点登录有效期"
              aria-labelledby="UserSsoLifetime"
            >
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item label="用户代码类型" aria-labelledby="User Code Type">
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
        </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ '身份验证/注销' }}
        </template>
        <a-row :gutter="80">
          <a-col :span="24">
            <a-form-item
              label="前端通道注销URI"
              aria-labelledby="FrontChannelLogoutUri"
            >
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="允许前端通道注销会话"
              aria-labelledby="FrontChannelLogoutSessionRequired"
            >
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="后端通道注销URI"
              aria-labelledby="BackChannelLogoutUri"
            >
              <a-input v-model="form.value"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item
              label="允许后端通道注销会话"
              aria-labelledby="BackChannelLogoutSessionRequired"
            >
              <a-checkbox v-model="form.isTrue"></a-checkbox>
            </a-form-item>
          </a-col>
        </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ '客户端密钥' }}
        </template>
        <a-row :gutter="80">
          <a-col :span="20">
            <a-row :gutter="80">
              <a-col :span="12">
                <a-form-item label="密钥值">
                  <a-input v-model="form.value"></a-input>
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item label="密钥过期时间">
                  <a-date-picker v-model="form.value" style="width: 100%" />
                </a-form-item>
              </a-col>
              <a-col :span="24">
                <a-form-item label="密钥描述">
                  <a-textarea
                    placeholder="请输入密钥描述"
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
                  <icon-plus />
                </template>
                删除
              </a-button>
            </div>
          </a-col>
          <a-col :span="24" style="text-align: center">
            <a-button type="primary">
              <template #icon>
                <icon-plus /> </template
              >添加
            </a-button>
          </a-col>
        </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ '资源' }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ 'Tokens' }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ 'Claims' }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ 'Grant Types' }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ 'Restrictions' }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card class="general-card">
        <template #title>
          {{ '属性' }}
        </template>
        <a-row :gutter="80"> </a-row>
      </a-card>
      <a-card :style="{ textAlign: 'center' }">
        <a-space>
          <a-button type="primary">Submit</a-button>
          <a-button type="text">Reset</a-button>
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
