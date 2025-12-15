<template>
  <div class="container">
    <a-card class="general-card" :title="$t('userSettings.title')">
      <a-tabs default-active-key="password">
        <!-- 修改密码 -->
        <a-tab-pane key="password" :title="$t('userSettings.changePassword')">
          <div class="form-wrapper">
            <a-form
              ref="passwordFormRef"
              :model="passwordForm"
              :rules="passwordRules"
              :label-col-props="{ span: 6 }"
              :wrapper-col-props="{ span: 14 }"
              @submit="handleChangePassword"
            >
              <a-form-item
                field="oldPassword"
                :label="$t('userSettings.oldPassword')"
              >
                <a-input-password
                  v-model="passwordForm.oldPassword"
                  :placeholder="$t('userSettings.oldPasswordPlaceholder')"
                  allow-clear
                />
              </a-form-item>
              <a-form-item
                field="newPassword"
                :label="$t('userSettings.newPassword')"
              >
                <a-input-password
                  v-model="passwordForm.newPassword"
                  :placeholder="$t('userSettings.newPasswordPlaceholder')"
                  allow-clear
                />
              </a-form-item>
              <a-form-item
                field="confirmPassword"
                :label="$t('userSettings.confirmPassword')"
              >
                <a-input-password
                  v-model="passwordForm.confirmPassword"
                  :placeholder="$t('userSettings.confirmPasswordPlaceholder')"
                  allow-clear
                />
              </a-form-item>
              <a-form-item :wrapper-col-props="{ offset: 6 }">
                <a-space>
                  <a-button
                    type="primary"
                    html-type="submit"
                    :loading="passwordLoading"
                  >
                    {{ $t('userSettings.submit') }}
                  </a-button>
                  <a-button @click="resetPasswordForm">
                    {{ $t('userSettings.reset') }}
                  </a-button>
                </a-space>
              </a-form-item>
            </a-form>
          </div>
        </a-tab-pane>

        <!-- 个人信息 -->
        <a-tab-pane key="profile" :title="$t('userSettings.profile')">
          <div class="form-wrapper">
            <a-form
              ref="profileFormRef"
              :model="profileForm"
              :label-col-props="{ span: 6 }"
              :wrapper-col-props="{ span: 14 }"
              @submit="handleUpdateProfile"
            >
              <a-form-item :label="$t('userSettings.username')">
                <a-input
                  v-model="profileForm.username"
                  :placeholder="$t('userSettings.usernamePlaceholder')"
                  disabled
                />
              </a-form-item>
              <a-form-item :label="$t('userSettings.email')">
                <a-input
                  v-model="profileForm.email"
                  :placeholder="$t('userSettings.emailPlaceholder')"
                />
              </a-form-item>
              <a-form-item :label="$t('userSettings.phone')">
                <a-input
                  v-model="profileForm.phone"
                  :placeholder="$t('userSettings.phonePlaceholder')"
                />
              </a-form-item>
              <a-form-item :wrapper-col-props="{ offset: 6 }">
                <a-space>
                  <a-button
                    type="primary"
                    html-type="submit"
                    :loading="profileLoading"
                  >
                    {{ $t('userSettings.save') }}
                  </a-button>
                  <a-button @click="goBack">
                    {{ $t('userSettings.cancel') }}
                  </a-button>
                </a-space>
              </a-form-item>
            </a-form>
          </div>
        </a-tab-pane>
      </a-tabs>
    </a-card>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { Message } from '@arco-design/web-vue';
  import { useI18n } from 'vue-i18n';
  import { useUserStore } from '@/store';
  import { user as userApi } from '@/api/administration/user';

  const { t } = useI18n();
  const router = useRouter();
  const userStore = useUserStore();

  // 密码表单
  const passwordFormRef = ref();
  const passwordLoading = ref(false);
  const passwordForm = reactive({
    oldPassword: '',
    newPassword: '',
    confirmPassword: '',
  });

  const validateConfirmPassword = (
    value: string,
    callback: (error?: string) => void
  ) => {
    if (value !== passwordForm.newPassword) {
      callback(t('userSettings.passwordNotMatch'));
    } else {
      callback();
    }
  };

  const passwordRules = computed(() => ({
    oldPassword: [
      { required: true, message: t('userSettings.oldPasswordRequired') },
    ],
    newPassword: [
      { required: true, message: t('userSettings.newPasswordRequired') },
      { minLength: 6, message: t('userSettings.passwordMinLength') },
    ],
    confirmPassword: [
      { required: true, message: t('userSettings.confirmPasswordRequired') },
      { validator: validateConfirmPassword },
    ],
  }));


  const resetPasswordForm = () => {
    passwordForm.oldPassword = '';
    passwordForm.newPassword = '';
    passwordForm.confirmPassword = '';
    passwordFormRef.value?.clearValidate();
  };

  const handleChangePassword = async ({
    errors,
  }: {
    errors: Record<string, any> | undefined;
  }) => {
    if (errors) return;

    passwordLoading.value = true;
    try {
      await userApi.api.changePassword(
        userStore.accountId!,
        passwordForm.oldPassword,
        passwordForm.newPassword
      );
      Message.success(t('userSettings.passwordChangeSuccess'));
      resetPasswordForm();
    } catch (error: any) {
      Message.error(error.message || t('userSettings.passwordChangeFailed'));
    } finally {
      passwordLoading.value = false;
    }
  };

  // 个人信息表单
  const profileFormRef = ref();
  const profileLoading = ref(false);
  const profileForm = reactive({
    username: userStore.name || '',
    email: userStore.email || '',
    phone: userStore.phone || '',
  });

  const handleUpdateProfile = async ({
    errors,
  }: {
    errors: Record<string, any> | undefined;
  }) => {
    if (errors) return;

    profileLoading.value = true;
    try {
      // TODO: 实现更新用户信息的 API
      await userApi.api.update({
        id: userStore.accountId || '',
        username: profileForm.username,
        email: profileForm.email,
        phoneNumber: profileForm.phone,
      });
      userStore.setInfo({
        email: profileForm.email,
        phone: profileForm.phone,
      });
      Message.success(t('userSettings.profileUpdateSuccess'));
    } catch (error: any) {
      Message.error(error.message || t('userSettings.profileUpdateFailed'));
    } finally {
      profileLoading.value = false;
    }
  };

  const goBack = () => {
    router.push({ name: 'Info' });
  };

  // 页面加载时从后端获取最新用户信息
  onMounted(async () => {
    if (userStore.accountId) {
      try {
        const userInfo = await userApi.api.get(userStore.accountId);
        // 更新 store 和表单
        userStore.setInfo({
          name: userInfo.username,
          email: userInfo.email,
          phone: userInfo.phoneNumber,
        });
        profileForm.username = userInfo.username || '';
        profileForm.email = userInfo.email || '';
        profileForm.phone = userInfo.phoneNumber || '';
      } catch (error) {
        console.error('Failed to fetch user info:', error);
      }
    }
  });
</script>

<style scoped lang="less">
  .container {
    padding: 20px;
  }

  .form-wrapper {
    max-width: 600px;
    margin: 30px auto;
    padding: 20px 0;
  }
</style>
