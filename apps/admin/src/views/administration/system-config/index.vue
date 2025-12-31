<template>
  <div class="container">
    <a-card class="general-card" :title="$t('menu.systemConfig')">
      <template #extra>
        <a-space>
          <a-button type="primary" size="small" @click="handleInitDefaults">
            <template #icon><icon-refresh /></template>
            {{ $t('systemConfig.initDefaults') }}
          </a-button>
          <a-button type="primary" size="small" status="success" @click="handleAddConfig">
            <template #icon><icon-plus /></template>
            {{ $t('systemConfig.addConfig') }}
          </a-button>
        </a-space>
      </template>

      <div class="config-layout">
        <!-- 左侧分组列表 -->
        <div class="group-list">
          <a-menu
            v-model:selected-keys="selectedGroupKeys"
            @menu-item-click="handleGroupClick"
          >
            <a-menu-item
              v-for="group in groups"
              :key="group"
              class="group-item"
            >
              <template #icon><icon-folder /></template>
              {{ group }}
              <span class="config-count">({{ getGroupCount(group) }})</span>
            </a-menu-item>
          </a-menu>
          <div v-if="groups.length === 0" class="no-groups">
            <a-empty :description="$t('systemConfig.noGroups')" />
          </div>
        </div>

        <!-- 右侧配置列表 -->
        <div class="config-panel">
          <div v-if="selectedGroup" class="config-header">
            <h3>{{ selectedGroup }}</h3>
            <a-button
              size="small"
              type="outline"
              :loading="saving"
              @click="handleSaveAll"
            >
              <template #icon><icon-save /></template>
              {{ $t('systemConfig.saveAll') }}
            </a-button>
          </div>

          <a-spin :loading="loading" style="width: 100%">
            <div v-if="currentConfigs.length > 0" class="config-list">
              <div
                v-for="config in currentConfigs"
                :key="config.key"
                class="config-item"
              >
                <div class="config-info">
                  <div class="config-key">
                    <a-tag size="small" color="arcoblue">{{
                      config.key
                    }}</a-tag>
                    <span v-if="config.isReadonly" class="readonly-badge">
                      <icon-lock />
                    </span>
                  </div>
                  <div class="config-desc">{{ config.description || '-' }}</div>
                </div>
                <div class="config-value">
                  <a-input
                    v-if="!config.isReadonly"
                    v-model="config.value"
                    :placeholder="$t('systemConfig.inputValue')"
                    allow-clear
                    @change="handleValueChange(config)"
                  />
                  <a-input
                    v-else
                    :model-value="config.value"
                    readonly
                    disabled
                  />
                </div>
                <div class="config-actions">
                  <a-button
                    v-if="!config.isReadonly"
                    type="text"
                    size="small"
                    status="danger"
                    @click="handleDeleteConfig(config)"
                  >
                    <template #icon><icon-delete /></template>
                  </a-button>
                </div>
              </div>
            </div>
            <div v-else class="no-configs">
              <a-empty :description="$t('systemConfig.noConfigs')">
                <template #image>
                  <icon-settings
                    :size="60"
                    style="color: var(--color-text-4)"
                  />
                </template>
              </a-empty>
            </div>
          </a-spin>
        </div>
      </div>
    </a-card>

    <!-- 添加配置弹窗 -->
    <a-modal
      v-model:visible="addModalVisible"
      :title="$t('systemConfig.addConfig')"
      :ok-loading="addSubmitLoading"
      @before-ok="handleAddBeforeOk"
      @cancel="addModalVisible = false"
    >
      <a-form :model="addForm" layout="vertical">
        <a-form-item field="group" :label="$t('systemConfig.group')" required>
          <a-select
            v-model="addForm.group"
            allow-create
            :placeholder="$t('systemConfig.selectOrInputGroup')"
          >
            <a-option v-for="g in groups" :key="g" :value="g">{{ g }}</a-option>
          </a-select>
        </a-form-item>
        <a-form-item field="key" :label="$t('systemConfig.key')" required>
          <a-input
            v-model="addForm.key"
            :placeholder="$t('systemConfig.inputKey')"
          />
        </a-form-item>
        <a-form-item field="value" :label="$t('systemConfig.value')">
          <a-input
            v-model="addForm.value"
            :placeholder="$t('systemConfig.inputValue')"
          />
        </a-form-item>
        <a-form-item
          field="description"
          :label="$t('systemConfig.description')"
        >
          <a-textarea
            v-model="addForm.description"
            :placeholder="$t('systemConfig.inputDescription')"
            :auto-size="{ minRows: 2, maxRows: 4 }"
          />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { ref, computed, onMounted } from 'vue';
  import { Message, Modal } from '@arco-design/web-vue';
  import { useI18n } from 'vue-i18n';
  import {
    getConfigGroups,
    getConfigsByGroup,
    setConfigValue,
    batchUpdateConfigs,
    deleteConfig,
    initDefaultConfigs,
    SystemConfigItem,
  } from '@/api/system-config';

  const { t } = useI18n();

  const loading = ref(false);
  const saving = ref(false);
  const groups = ref<string[]>([]);
  const selectedGroupKeys = ref<string[]>([]);
  const configsByGroup = ref<Record<string, SystemConfigItem[]>>({});
  const modifiedConfigs = ref<Set<string>>(new Set());

  // Add configuration modal
  const addModalVisible = ref(false);
  const addForm = ref({
    group: '',
    key: '',
    value: '',
    description: '',
  });

  const selectedGroup = computed(() => selectedGroupKeys.value[0] || '');

  const currentConfigs = computed(() => {
    if (!selectedGroup.value) return [];
    return configsByGroup.value[selectedGroup.value] || [];
  });

  const getGroupCount = (group: string) => {
    return configsByGroup.value[group]?.length || 0;
  };

  // Load group configurations
  const loadGroupConfigs = async (group: string) => {
    loading.value = true;
    try {
      const res = await getConfigsByGroup(group);
      if (res.data.success) {
        configsByGroup.value[group] = res.data.data;
      }
    } finally {
      loading.value = false;
    }
  };

  // Load groups
  const loadGroups = async () => {
    try {
      const res = await getConfigGroups();
      if (res.data.success) {
        groups.value = res.data.data;
        // Preload all group configurations to display correct counts
        await Promise.all(groups.value.map((group) => loadGroupConfigs(group)));
        // Auto-select first group
        if (groups.value.length > 0 && !selectedGroup.value) {
          selectedGroupKeys.value = [groups.value[0]];
        }
      }
    } catch {
      // Failed to load groups
    }
  };

  // Handle group click
  const handleGroupClick = async (key: string) => {
    if (!configsByGroup.value[key]) {
      await loadGroupConfigs(key);
    }
  };

  // Handle value change
  const handleValueChange = (config: SystemConfigItem) => {
    modifiedConfigs.value.add(`${config.group}:${config.key}`);
  };

  // Save all changes
  const handleSaveAll = async () => {
    if (modifiedConfigs.value.size === 0) {
      Message.info(t('systemConfig.noChanges'));
      return;
    }

    saving.value = true;
    try {
      const configsToSave = currentConfigs.value.filter((c) =>
        modifiedConfigs.value.has(`${c.group}:${c.key}`)
      );
      await batchUpdateConfigs(configsToSave);
      modifiedConfigs.value.clear();
      Message.success(t('systemConfig.saveSuccess'));
    } catch (error) {
      Message.error(t('systemConfig.saveFailed'));
    } finally {
      saving.value = false;
    }
  };

  // Delete configuration
  const handleDeleteConfig = (config: SystemConfigItem) => {
    Modal.confirm({
      title: t('systemConfig.deleteConfirm'),
      content: t('systemConfig.deleteConfirmContent', { key: config.key }),
      okText: t('systemConfig.modal.ok'),
      cancelText: t('systemConfig.modal.cancel'),
      okButtonProps: { status: 'danger' },
      onOk: async () => {
        try {
          await deleteConfig(config.group, config.key);
          // Refresh current group
          await loadGroupConfigs(config.group);
          Message.success(t('systemConfig.deleteSuccess'));
        } catch (error) {
          Message.error(t('systemConfig.deleteFailed'));
        }
      },
    });
  };

  // Add configuration
  const handleAddConfig = () => {
    addForm.value = {
      group: selectedGroup.value || '',
      key: '',
      value: '',
      description: '',
    };
    addModalVisible.value = true;
  };

  const addSubmitLoading = ref(false);

  const handleAddBeforeOk = async (done: (closed: boolean) => void) => {
    if (!addForm.value.group || !addForm.value.key) {
      Message.warning(t('systemConfig.fillRequired'));
      done(false);
      return;
    }

    try {
      addSubmitLoading.value = true;
      await setConfigValue(
        addForm.value.group,
        addForm.value.key,
        addForm.value.value,
        addForm.value.description
      );
      // Refresh group list and configurations
      await loadGroups();
      if (addForm.value.group) {
        selectedGroupKeys.value = [addForm.value.group];
        await loadGroupConfigs(addForm.value.group);
      }
      Message.success(t('systemConfig.addSuccess'));
      done(true);
    } catch (error) {
      Message.error(t('systemConfig.addFailed'));
      done(false);
    } finally {
      addSubmitLoading.value = false;
    }
  };

  // Initialize default configurations
  const handleInitDefaults = () => {
    Modal.confirm({
      title: t('systemConfig.initDefaultsConfirm'),
      content: t('systemConfig.initDefaultsContent'),
      okText: t('systemConfig.modal.ok'),
      cancelText: t('systemConfig.modal.cancel'),
      onOk: async () => {
        try {
          await initDefaultConfigs();
          await loadGroups();
          // Reload all loaded groups
          const loadedGroups = Object.keys(configsByGroup.value);
          await Promise.all(loadedGroups.map((group) => loadGroupConfigs(group)));
          Message.success(t('systemConfig.initDefaultsSuccess'));
        } catch {
          Message.error(t('systemConfig.initDefaultsFailed'));
        }
      },
    });
  };

  onMounted(() => {
    loadGroups();
  });
</script>

<style scoped lang="less">
  .container {
    padding: 16px;
  }

  .config-layout {
    display: flex;
    gap: 16px;
    min-height: 500px;
  }

  .group-list {
    width: 220px;
    flex-shrink: 0;
    border-right: 1px solid var(--color-border);
    padding-right: 16px;

    .group-item {
      .config-count {
        color: var(--color-text-3);
        font-size: 12px;
        margin-left: 4px;
      }
    }

    .no-groups {
      padding: 40px 0;
      text-align: center;
    }
  }

  .config-panel {
    flex: 1;
    min-width: 0;

    .config-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
      padding-bottom: 12px;
      border-bottom: 1px solid var(--color-border);

      h3 {
        margin: 0;
        font-size: 16px;
        font-weight: 500;
      }
    }
  }

  .config-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .config-item {
    display: flex;
    align-items: center;
    gap: 16px;
    padding: 12px 16px;
    background: var(--color-fill-1);
    border-radius: 6px;
    transition: background 0.2s;

    &:hover {
      background: var(--color-fill-2);
    }

    .config-info {
      width: 280px;
      flex-shrink: 0;

      .config-key {
        display: flex;
        align-items: center;
        gap: 6px;
        margin-bottom: 4px;

        .readonly-badge {
          color: var(--color-text-3);
          font-size: 12px;
        }
      }

      .config-desc {
        font-size: 12px;
        color: var(--color-text-3);
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
    }

    .config-value {
      flex: 1;
      min-width: 0;
    }

    .config-actions {
      width: 40px;
      flex-shrink: 0;
      text-align: right;
    }
  }

  .no-configs {
    padding: 60px 0;
    text-align: center;
  }
</style>
