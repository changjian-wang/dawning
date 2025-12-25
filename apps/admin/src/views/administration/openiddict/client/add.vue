<template>
  <div class="client-add">
    <div class="container">
      <Breadcrumb :items="['认证授权', '客户端', '新增信息']" />
      <a-form
        ref="formRef"
        class="form"
        layout="horizontal"
        auto-label-width
        :model="form"
        :rules="rules"
        @submit="handleSubmit"
      >
        <a-tabs v-model:active-key="activeKey" :default-active-key="1">
          <a-tab-pane :key="1" title="基本信息" @click="handleTabClick(1)">
            <a-card class="general-card">
              <a-row :gutter="80">
                <a-col :span="24">
                  <a-form-item
                    label="客户端ID"
                    field="clientId"
                    aria-labelledby="Client ID"
                    required
                  >
                    <a-input
                      ref="clientIdInputRef"
                      v-model="form.clientId"
                      validate-trigger="blur"
                    ></a-input>
                    <a-button
                      :style="{ marginLeft: '10px' }"
                      @click="handleGenerateClientId"
                    >
                      <template #icon>
                        <icon-sync />
                      </template>
                    </a-button>
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item
                    label="显示名称"
                    field="clientName"
                    aria-labelledby="Client Name"
                    required
                  >
                    <a-input
                      v-model="form.clientName"
                      validate-trigger="blur"
                    ></a-input>
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="客户端URI" aria-labelledby="Client URI">
                    <a-input v-model="form.clientUri"></a-input>
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="LogoURI" aria-labelledby="LOGO URI">
                    <a-input v-model="form.logoUri"></a-input>
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item
                    label="登录回调URI"
                    field="loginUri"
                    required
                    aria-labelledby="Login URI"
                  >
                    <a-input
                      v-model="form.loginUri"
                      placeholder="输入登录后回调的URI"
                      validate-trigger="blur"
                    />
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
              </a-row>
            </a-card>
          </a-tab-pane>
          <a-tab-pane
            :key="2"
            title="安全凭证与认证"
            @click="handleTabClick(2)"
          >
            <a-card class="general-card">
              <a-row :gutter="80">
                <a-col :span="24">
                  <a-form-item label="需要密钥">
                    <a-switch v-model="form.requireClientSecret">
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
                  <a-form-item label="强制要求PKCE">
                    <a-switch v-model="form.requirePkce">
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
                  <a-form-item label="允许明文PKCE">
                    <a-switch v-model="form.allowPlainTextPkce">
                      <template #checked-icon>
                        <a-tooltip content="不推荐">
                          <icon-check />
                        </a-tooltip>
                      </template>
                      <template #unchecked-icon>
                        <icon-close />
                      </template>
                    </a-switch>
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="授权类型">
                    <a-select
                      v-model="form.allowedGrantTypes"
                      default-value="hybrid"
                    >
                      <a-option value="hybrid">Hybrid</a-option>
                      <a-option value="implicit" disabled>Implicit</a-option>
                      <a-option value="authorization_code" disabled
                        >AuthorizationCode</a-option
                      >
                      <a-option value="client_credentials" disabled
                        >ClientCredentials</a-option
                      >
                      <a-option value="password" disabled
                        >ResourceOwnerPassword</a-option
                      >
                      <a-option
                        value="urn:ietf:params:oauth:grant-type:device_code"
                        disabled
                        >DeviceFlow</a-option
                      >
                    </a-select>
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="身份令牌签名算法">
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
                </a-col>
                <a-col :span="24">
                  <a-form-item label="CORS">
                    <a-space>
                      <a-tag
                        v-for="cors of corsOrigins"
                        :key="cors"
                        :color="'arcoblue'"
                        :closable="true"
                        @close="handleCorsRemove(cors)"
                      >
                        {{ cors }}
                      </a-tag>
                      <a-input
                        v-if="showCorsInput"
                        ref="corsInputRef"
                        v-model.trim="corsInputVal"
                        :style="{ width: '90px' }"
                        size="mini"
                        @keyup.enter="handleCorsAdd"
                        @blur="handleCorsAdd"
                      />
                      <a-tag
                        v-else
                        :style="{
                          width: '90px',
                          backgroundColor: 'var(--color-fill-2)',
                          border: '1px dashed var(--color-fill-3)',
                          cursor: 'pointer',
                        }"
                        @click="handleCorsEdit"
                      >
                        <template #icon>
                          <icon-plus />
                        </template>
                        添加CORS
                      </a-tag>
                    </a-space>
                  </a-form-item>
                </a-col>
              </a-row>
              <a-divider orientation="center">密钥</a-divider>
              <a-form
                ref="secretFormRef"
                class="form"
                layout="horizontal"
                auto-label-width
                :model="secretForm"
              >
                <a-row :gutter="80">
                  <a-col :span="12">
                    <a-form-item label="类型">
                      <a-select v-model="secretForm.type" placeholder="请选择">
                        <a-option>Shared Secret</a-option>
                        <a-option disabled>X509 Certificate Base64</a-option>
                        <a-option disabled>X509 Name</a-option>
                        <a-option disabled>X509 Thumbprint</a-option>
                      </a-select>
                    </a-form-item>
                  </a-col>
                  <a-col :span="12">
                    <a-form-item label="密钥过期时间">
                      <a-date-picker
                        v-model="secretForm.expiration"
                        :disabled-date="
                          (current: any) => dayjs(current).isBefore(dayjs())
                        "
                      />
                    </a-form-item>
                  </a-col>
                  <a-col :span="24">
                    <a-form-item label="密钥值">
                      <a-input
                        v-model="secretForm.value"
                        placeholder="密钥值"
                      ></a-input>
                      <a-button
                        :style="{ marginLeft: '10px' }"
                        @click="handleGenerateSecretValue"
                      >
                        <template #icon>
                          <icon-sync />
                        </template>
                      </a-button>
                    </a-form-item>
                  </a-col>
                  <a-col :span="24">
                    <a-form-item label="密钥描述">
                      <a-textarea
                        v-model="secretForm.description"
                        placeholder="请输入密钥描述"
                        :auto-size="{ minRows: 2, maxRows: 5 }"
                        allow-clear
                      />
                    </a-form-item>
                  </a-col>
                </a-row>
              </a-form>
              <a-space
                style="width: 100%; justify-content: right; margin-bottom: 1em"
              >
                <a-button
                  type="primary"
                  :disabled="!secretForm.value"
                  @click="!!secretForm.value && handleAddSecret()"
                >
                  <template #icon>
                    <icon-plus />
                  </template>
                  <span>添加</span>
                </a-button>
              </a-space>
              <a-table :columns="secretColumns" :data="form.clientSecrets">
                <template #optional="{ rowIndex }">
                  <a-space>
                    <a-popconfirm
                      content="确定删除?"
                      @ok="handleDeleteSecret(rowIndex)"
                    >
                      <a-button>
                        <template #icon>
                          <icon-delete />
                        </template>
                      </a-button>
                    </a-popconfirm>
                  </a-space>
                </template>
              </a-table>
            </a-card>
          </a-tab-pane>
          <a-tab-pane
            :key="3"
            title="令牌生命周期与策略"
            @click="handleTabClick(3)"
          >
            <a-card class="general-card">
              <a-row :gutter="80">
                <a-col :span="12">
                  <a-form-item label="身份令牌有效期">
                    <a-space>
                      <a-input-number
                        v-model="form.identityTokenLifetime"
                        placeholder="请输入有效时间"
                        :default-value="300"
                        :min="0"
                        :step="30"
                      />
                      <a-button-group>
                        <a-button
                          @click="
                            () => {
                              form.identityTokenLifetime = 300;
                            }
                          "
                          >5分钟</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.identityTokenLifetime = 600;
                            }
                          "
                          >10分钟</a-button
                        >
                      </a-button-group>
                    </a-space>
                  </a-form-item>
                </a-col>
                <a-col :span="12">
                  <a-form-item label="访问令牌有效期">
                    <a-space>
                      <a-input-number
                        v-model="form.accessTokenLifetime"
                        placeholder="请输入有效时间"
                        :default-value="900"
                        :min="0"
                        :step="60"
                      />
                      <a-button-group>
                        <a-button
                          @click="
                            () => {
                              form.accessTokenLifetime = 900;
                            }
                          "
                          >15分钟</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.accessTokenLifetime = 1800;
                            }
                          "
                          >30分钟</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.accessTokenLifetime = 3600;
                            }
                          "
                          >1小时</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.accessTokenLifetime = 18000;
                            }
                          "
                          >5小时</a-button
                        >
                      </a-button-group>
                    </a-space>
                  </a-form-item>
                </a-col>
                <a-col :span="12">
                  <a-form-item label="授权码有效期">
                    <a-space>
                      <a-input-number
                        v-model="form.authorizationCodeLifetime"
                        placeholder="请输入有效时间"
                        :default-value="300"
                        :min="0"
                        :step="60"
                      />
                      <a-button-group>
                        <a-button
                          @click="
                            () => {
                              form.authorizationCodeLifetime = 300;
                            }
                          "
                          >5分钟</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.authorizationCodeLifetime = 600;
                            }
                          "
                          >10分钟</a-button
                        >
                      </a-button-group>
                    </a-space>
                  </a-form-item>
                </a-col>
                <a-col :span="12">
                  <a-form-item label="刷新令牌绝对有效期">
                    <a-space>
                      <a-input-number
                        v-model="form.absoluteRefreshTokenLifetime"
                        placeholder="请输入有效时间"
                        :default-value="86400"
                        :min="0"
                        :step="86400"
                      />
                      <a-button-group>
                        <a-button
                          @click="
                            () => {
                              form.absoluteRefreshTokenLifetime = 86400;
                            }
                          "
                          >1天</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.absoluteRefreshTokenLifetime = 1296000;
                            }
                          "
                          >15天</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.absoluteRefreshTokenLifetime = 2592000;
                            }
                          "
                          >30天</a-button
                        >
                      </a-button-group>
                    </a-space>
                  </a-form-item>
                </a-col>
                <a-col :span="12">
                  <a-form-item label="开启令牌滑动过期模式">
                    <a-switch
                      v-model="form.refreshTokenExpiration"
                      checked-value="0"
                      unchecked-value="1"
                    >
                      <template #checked-icon>
                        <icon-check />
                      </template>
                      <template #unchecked-icon>
                        <icon-close />
                      </template>
                    </a-switch>
                  </a-form-item>
                </a-col>
                <a-col :span="12">
                  <a-form-item
                    v-if="form.refreshTokenExpiration == 0"
                    label="刷新令牌滑动有效期"
                  >
                    <a-space>
                      <a-input-number
                        v-model="form.slidingRefreshTokenLifetime"
                        placeholder="请输入有效时间"
                        :default-value="900"
                        :min="0"
                        :step="60"
                      />
                      <a-button-group>
                        <a-button
                          @click="
                            () => {
                              form.slidingRefreshTokenLifetime = 900;
                            }
                          "
                          >15分钟</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.slidingRefreshTokenLifetime = 1800;
                            }
                          "
                          >30分钟</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.slidingRefreshTokenLifetime = 3600;
                            }
                          "
                          >1小时</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.slidingRefreshTokenLifetime = 18000;
                            }
                          "
                          >5小时</a-button
                        >
                      </a-button-group>
                    </a-space>
                  </a-form-item>
                </a-col>
                <a-col :span="12">
                  <a-form-item label="刷新令牌使用方式">
                    <a-space>
                      <a-radio-group
                        v-model="form.refreshTokenUsage"
                        default-value="1"
                      >
                        <a-radio :value="0">ReUse</a-radio>
                        <a-radio :value="1">One Time Only</a-radio>
                      </a-radio-group>
                    </a-space>
                  </a-form-item>
                </a-col>
                <a-col :span="12">
                  <a-form-item label="访问令牌类型">
                    <a-space>
                      <a-radio-group
                        v-model="form.accessTokenType"
                        default-value="0"
                      >
                        <a-radio :value="0">Jwt</a-radio>
                        <a-radio :value="1">Reference</a-radio>
                      </a-radio-group>
                    </a-space>
                  </a-form-item>
                </a-col>
              </a-row>
            </a-card>
          </a-tab-pane>
          <a-tab-pane
            :key="4"
            title="会话管理与登录注销"
            @click="handleTabClick(4)"
          >
            <a-card class="general-card">
              <a-row :gutter="80">
                <a-col :span="24">
                  <a-form-item label="前端通道注销URI">
                    <a-input
                      v-model="form.frontChannelLogoutUri"
                      placeholder="前端通道注销URI"
                    />
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="开启前端注销会话">
                    <a-switch v-model="form.frontChannelLogoutSessionRequired">
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
                  <a-form-item label="后端通道注销URI">
                    <a-input
                      v-model="form.backChannelLogoutUri"
                      placeholder="后端通道注销URI"
                    />
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="开启后端注销会话">
                    <a-switch v-model="form.backChannelLogoutSessionRequired">
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
              <a-divider orientation="center">登录/注销URIs</a-divider>
              <a-space
                style="width: 100%; justify-content: right; margin-bottom: 1em"
              >
                <a-button
                  type="primary"
                  @click="
                    () => {
                      visibleWithAddURI = true;
                    }
                  "
                  >新增URI</a-button
                >
              </a-space>
              <a-table
                :columns="redirectUriColumns"
                :data="redirectUriData"
                :pagination="false"
              >
                <template #uri="{ record, rowIndex }">
                  <a-input
                    v-if="record.isEditing"
                    :ref="(el: any) => setEditInputRef(el, rowIndex)"
                    v-model="redirectUriData[rowIndex].uri"
                    @press-enter="
                      (event: any) => {
                        event.preventDefault();
                        redirectUriData[rowIndex].isEditing = false;
                      }
                    "
                  />
                  <span v-else>{{ record.uri }}</span>
                </template>
                <template #callback="{ record }">
                  <a-space>
                    <icon-check v-if="record.type === RedirectTypes.Callback" />
                    <icon-close v-else />
                  </a-space>
                </template>
                <template #signout="{ record }">
                  <a-space>
                    <icon-check v-if="record.type === RedirectTypes.Signout" />
                    <icon-close v-else />
                  </a-space>
                </template>
                <template #optional="{ rowIndex }">
                  <a-space>
                    <a-button @click="handleEditRedirectUri(rowIndex)">
                      <template #icon>
                        <icon-edit />
                      </template>
                    </a-button>
                    <a-popconfirm
                      content="确定删除?"
                      @ok="handleRemoveRedirectUri(rowIndex)"
                    >
                      <a-button>
                        <template #icon>
                          <icon-delete />
                        </template>
                      </a-button>
                    </a-popconfirm>
                  </a-space>
                </template>
              </a-table>
            </a-card>
          </a-tab-pane>
          <a-tab-pane
            :key="5"
            title="用户声明与数据控制"
            @click="handleTabClick(5)"
          >
            <a-card class="general-card">
              <a-row :gutter="80">
                <a-col :span="24">
                  <a-form-item label="始终发送客户端声明">
                    <a-switch v-model="form.alwaysSendClientClaims">
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
                  <a-form-item label="始终包含用户声明于ID Token">
                    <a-switch v-model="form.alwaysIncludeUserClaimsInIdToken">
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
                  <a-form-item label="声明前缀">
                    <a-input
                      v-model="form.clientClaimsPrefix"
                      placeholder="输入前缀"
                    />
                  </a-form-item>
                </a-col>
                <a-col :span="24">
                  <a-form-item label="配对主体盐值">
                    <a-input
                      v-model="form.pairWiseSubjectSalt"
                      placeholder="输入客户端-服务端配对盐值"
                    />
                    <a-button
                      :style="{ marginLeft: '10px' }"
                      @click="handleGeneratePairWiseSubjectSalt"
                    >
                      <template #icon>
                        <icon-sync />
                      </template>
                    </a-button>
                  </a-form-item>
                </a-col>
              </a-row>
              <a-divider orientation="center">声明</a-divider>
              <a-space
                style="width: 100%; justify-content: right; margin-bottom: 1em"
              >
                <a-button type="primary" @click="visibleWithAddClaim = true"
                  >新增</a-button
                >
              </a-space>
              <a-table :columns="claimColumns" :data="form.claims">
                <template #optional="{ rowIndex }">
                  <a-space>
                    <a-popconfirm
                      content="确定删除?"
                      @ok="handleDeleteClaim(rowIndex)"
                    >
                      <a-button>
                        <template #icon>
                          <icon-delete />
                        </template>
                      </a-button>
                    </a-popconfirm>
                  </a-space>
                </template>
              </a-table>
            </a-card>
          </a-tab-pane>
          <a-tab-pane
            :key="6"
            title="用户交互与同意"
            @click="handleTabClick(6)"
          >
            <a-card class="general-card">
              <a-row :gutter="80">
                <a-col :span="24">
                  <a-form-item label="强制用户授权同意">
                    <a-switch v-model="form.requireConsent">
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
                  <a-form-item label="允许记住同意状态">
                    <a-switch
                      v-model="form.allowRememberConsent"
                      :disabled="!form.requireConsent"
                    >
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
                  <a-form-item label="同意状态有效期">
                    <a-space>
                      <a-input-number
                        v-model="form.consentLifetime"
                        placeholder="请输入有效时间"
                        :default-value="0"
                        :min="0"
                        :step="604800"
                        :disabled="!form.requireConsent"
                      />
                      <a-button-group>
                        <a-button
                          @click="
                            () => {
                              form.consentLifetime = 604800;
                            }
                          "
                          >7天</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.consentLifetime = 2629743;
                            }
                          "
                          >30天</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.consentLifetime = 2592000;
                            }
                          "
                          >365天</a-button
                        >
                        <a-button
                          @click="
                            () => {
                              form.consentLifetime = 0;
                            }
                          "
                          >无限期</a-button
                        >
                      </a-button-group>
                    </a-space>
                  </a-form-item>
                </a-col>
              </a-row>
            </a-card>
          </a-tab-pane>
        </a-tabs>
        <a-divider></a-divider>
        <a-card class="general-card">
          <a-row :gutter="80">
            <a-col :span="24">
              <a-space style="width: 100%; justify-content: center">
                <a-button html-type="submit" type="primary" :loading="submitting" :disabled="submitting">提交</a-button>
                <a-button
                  :disabled="submitting"
                  @click="() => ($refs.formRef as FormInstance).resetFields()"
                  >重置</a-button
                >
              </a-space>
            </a-col>
          </a-row>
        </a-card>
      </a-form>
    </div>

    <!-- Modals -->
    <div class="add-uri">
      <a-modal v-model:visible="visibleWithAddURI" width="800px">
        <template #title> 新增URI </template>
        <a-form
          class="form"
          layout="horizontal"
          auto-label-width
          :model="redirectUriForm"
        >
          <a-card class="general-card">
            <a-row :gutter="80">
              <a-col :span="24">
                <a-form-item label="URI">
                  <a-input
                    v-model="redirectUriForm.uri"
                    placeholder="输入URI, 如https://docs.aluneth.com/signin-oidc"
                  ></a-input>
                </a-form-item>
              </a-col>
              <a-col :span="24">
                <a-form-item label="登录/注销">
                  <a-switch
                    v-model="redirectUriForm.type"
                    checked-color="#14C9C9"
                    unchecked-color="#F53F3F"
                    type="round"
                    :checked-value="RedirectTypes.Callback"
                    :unchecked-value="RedirectTypes.Signout"
                  >
                    <template #checked>IN</template>
                    <template #unchecked>OUT</template>
                  </a-switch>
                </a-form-item>
              </a-col>
            </a-row>
          </a-card>
        </a-form>

        <template #footer>
          <a-space>
            <a-button @click="visibleWithAddURI = false">取消</a-button>
            <a-button
              type="primary"
              :disabled="
                !(
                  !!redirectUriForm.uri &&
                  !_.some(redirectUriData, {
                    uri: redirectUriForm.uri,
                    type: redirectUriForm.type,
                  })
                )
              "
              @click="handleAddRedirectUri"
            >
              提交
            </a-button>
          </a-space>
        </template>
      </a-modal>
    </div>
    <div class="add-claim">
      <a-modal
        v-model:visible="visibleWithAddClaim"
        width="800px"
        @ok="handleAddClaim"
      >
        <a-form
          class="form"
          layout="horizontal"
          auto-label-width
          :model="claimForm"
        >
          <a-card class="general-card">
            <a-row :gutter="80">
              <a-col :span="24">
                <a-form-item label="声明类型">
                  <a-select v-model="claimForm.type" placeholder="请选择类型">
                    <a-option
                      v-for="(item, index) in claimTypeOptions"
                      :key="index"
                      :value="item.name"
                      >{{ item.displayName }}({{ item.name }})</a-option
                    >
                  </a-select>
                </a-form-item>
              </a-col>
              <a-col :span="24">
                <a-form-item label="声明值">
                  <a-input
                    v-model="claimForm.value"
                    placeholder="输入声明的值"
                  />
                </a-form-item>
              </a-col>
            </a-row>
          </a-card>
        </a-form>
      </a-modal>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { nextTick, onMounted, reactive, ref } from 'vue';
  import { useRouter } from 'vue-router';
  import { Message } from '@arco-design/web-vue';
  import dayjs from 'dayjs';
  import { v7 as uuidv7 } from 'uuid';
  import { FormInstance } from '@arco-design/web-vue';
  import {
    client,
    IClientWithLoginURI,
    IRedirectUri,
  } from '@/api/openiddict/client';
  import { ClientSecret, IClientSecret } from '@/api/openiddict/client-secret';
  import { IClientRedirectUri } from '@/api/openiddict/client-redirect-uri';
  import { IClientPostLogoutRedirectUri } from '@/api/openiddict/client-post-logout-redirect-uri';
  import { IClientClaim } from '@/api/openiddict/client-claim';
  import { claimType, IClaimType } from '@/api/administration/claim-type';
  import _, { forEach } from 'lodash';
  import CryptoJS from 'crypto-js';
  import { RedirectType, RedirectTypes } from '@/api/constants';

  const router = useRouter();
  const activeKey = ref(1);
  const visibleWithAddURI = ref(false);
  const visibleWithAddClaim = ref(false);
  const submitting = ref(false);
  const corsOrigins = ref<string[]>([
    'https://oa.aluneth.com',
    'https://m.oa.aluneth.com',
    'https://api.oa.aluneth.com',
  ]);
  const corsInputRef = ref<HTMLInputElement | null>(null);
  const showCorsInput = ref(false);
  const corsInputVal = ref('');
  const form = reactive<IClientWithLoginURI>({
    ...client.form.createWithLoginURI(),
  });
  const formRef = ref<FormInstance | null>(null); // 明确表单引用类型
  const initClaimForm: IClientClaim = {
    id: '',
    type: '',
    value: '',
    clientId: '',
  };
  const claimForm = reactive<IClientClaim>({ ...initClaimForm });
  const clientIdInputRef = ref<HTMLInputElement | null>(null);
  const rules = {
    clientId: [
      {
        required: true,
        message: '客户端ID不能为空',
      },
    ],
    displayName: [
      {
        required: true,
        message: '客户端显示名称不能为空',
      },
    ],
    loginUri: [
      {
        required: true,
        message: '登录回调URI',
      },
    ],
  };
  const initSecretForm: IClientSecret = new ClientSecret();
  const secretForm = reactive<IClientSecret>({ ...initSecretForm });
  const secretColumns = reactive([
    {
      title: '类型',
      dataIndex: 'type',
    },
    {
      title: '值',
      dataIndex: 'value',
    },
    {
      title: '描述',
      dataIndex: 'description',
    },
    {
      title: '创建时间',
      dataIndex: 'created',
    },
    {
      title: '过期时间',
      dataIndex: 'expiration',
    },
    {
      title: '操作',
      slotName: 'optional',
    },
  ]);
  const claimColumns = reactive([
    {
      title: '类型',
      dataIndex: 'type',
    },
    {
      title: '值',
      dataIndex: 'value',
    },
    {
      title: '操作',
      slotName: 'optional',
    },
  ]);
  type InputRef = HTMLInputElement | null;
  const editInputRefs = ref<Record<number, InputRef>>({});

  const setEditInputRef = (el: InputRef, index: number) => {
    editInputRefs.value[index] = el;
  };
  const handleEditUriFocusInput = async (index: number) => {
    await nextTick();
    editInputRefs.value[index]?.focus(); // ✅ 类型安全
  };
  const redirectUriForm = reactive<IRedirectUri>({
    uri: '',
    type: RedirectTypes.Callback,
    isEditing: false,
  });
  const redirectUriColumns = reactive([
    {
      title: '链接',
      dataIndex: 'uri',
      slotName: 'uri',
    },
    {
      title: '回调URI',
      dataIndex: 'type',
      slotName: 'callback',
    },
    {
      title: '注销URI',
      dataIndex: 'type',
      slotName: 'signout',
    },
    {
      title: '操作',
      width: 200,
      slotName: 'optional',
    },
  ]);
  const redirectUriData = reactive<IRedirectUri[]>([]);
  let claimTypeOptions = reactive<IClaimType[]>([]);

  const handleGenerateClientId = () => {
    form.clientId = uuidv7().replace(/-/g, '');

    nextTick(() => {
      if (clientIdInputRef.value) {
        clientIdInputRef.value.focus();
        formRef.value?.clearValidate('clientId');
      }
    });
  };
  const handleGeneratePairWiseSubjectSalt = () => {
    form.pairWiseSubjectSalt = CryptoJS.lib.WordArray.random(8).toString(
      CryptoJS.enc.Hex
    );
  };
  const handleGenerateSecretValue = () => {
    secretForm.value = uuidv7().replace(/-/g, '');
  };
  const handleAddSecret = () => {
    form.clientSecrets.push({ ...secretForm });
    Object.assign(secretForm, initSecretForm);
  };
  const handleDeleteSecret = (index: number) => {
    _.pullAt(form.clientSecrets, index);
  };
  const handleAddClaim = () => {
    form.claims.push({ ...claimForm });
    Object.assign(claimForm, initClaimForm);
  };
  const handleDeleteClaim = (index: number) => {
    form.claims.splice(index, 1);
  };
  const handleCorsEdit = () => {
    showCorsInput.value = true;

    nextTick(() => {
      if (corsInputRef.value) {
        corsInputRef.value.focus();
      }
    });
  };
  const handleCorsAdd = () => {
    if (corsInputVal.value) {
      corsOrigins.value.push(corsInputVal.value);
      corsInputVal.value = '';
    }
    showCorsInput.value = false;
  };
  const handleCorsRemove = (key: any) => {
    corsOrigins.value = corsOrigins.value.filter(
      (item: string) => item !== key
    );
  };
  const handleAddRedirectUri = () => {
    redirectUriData.push({ ...redirectUriForm });
    redirectUriForm.uri = '';
    redirectUriForm.type = RedirectTypes.Callback;
  };
  const handleEditRedirectUri = (index: number) => {
    redirectUriData[index].isEditing = !redirectUriData[index].isEditing;
    handleEditUriFocusInput(index);
  };
  const handleRemoveRedirectUri = (index: number) => {
    _.pullAt(redirectUriData, index);
  };
  const handleGetAllClaimType = async () => {
    claimTypeOptions = await claimType.api.getAll();
  };

  const handleTabClick = (key: number) => {
    activeKey.value = key;
  };

  const handleSubmit = async () => {
    if (submitting.value) return;
    submitting.value = true;
    try {
      const result = await formRef.value?.validate();

      if (result === undefined) {
        console.log('验证通过');
        const redirectUris: IClientRedirectUri[] = [
          {
            redirectUri: form.loginUri!,
            id: '',
            clientId: '',
          },
        ];
        form.redirectUris = redirectUris.concat(form.redirectUris);
        forEach(redirectUriData, (item: IRedirectUri) => {
          const redirectUri: IClientRedirectUri = {
            redirectUri: item.uri,
            id: '',
            clientId: '',
          };
          form.redirectUris.push(redirectUri);
        });

        // 调用 API 创建客户端
        await client.create(form);
        Message.success('客户端创建成功');
        router.push({ name: 'OpenIddictClient' });
      } else {
        activeKey.value = 1;
        console.log('验证失败');
      }
    } catch (errors) {
      activeKey.value = 1;
      console.error('验证失败:', errors);
    } finally {
      submitting.value = false;
    }
  };

  onMounted(async () => {
    await handleGetAllClaimType();
  });
</script>

<style lang="less" scoped></style>
