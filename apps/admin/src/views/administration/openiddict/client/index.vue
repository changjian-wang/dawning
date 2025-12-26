<template>
  <div class="client">
    <div class="container">
      <Breadcrumb :items="['menu.administration.openiddict', 'menu.administration.openiddict.client']" />
      <a-card class="general-card">
        <template #title>
          {{ $t('client.title') }}
        </template>
        <a-row :gutter="12">
          <a-col :span="6">
            <a-form-item :label="$t('client.form.name')">
              <a-input :placeholder="$t('client.form.name.placeholder')"></a-input>
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item :label="$t('client.form.type')">
              <a-select :placeholder="$t('client.form.type.placeholder')">
                <a-option>String</a-option>
                <a-option>Int</a-option>
                <a-option>DateTime</a-option>
                <a-option>Boolean</a-option>
                <a-option>Enum</a-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :flex="30" style="text-align: right">
            <a-space direction="horizontal" :size="18">
              <a-button type="primary" size="small" @click="() => {}">
                <template #icon>
                  <icon-search />
                </template>
                {{ $t('common.search') }}
              </a-button>
              <a-button size="small" @click="() => {}">
                <template #icon>
                  <icon-refresh />
                </template>
                {{ $t('common.reset') }}
              </a-button>
              <a-button
                type="primary"
                size="small"
                status="success"
                @click="
                  () => {
                    $router.push('client/add');
                  }
                "
              >
                <template #icon>
                  <icon-plus />
                </template>
                {{ $t('common.add') }}
              </a-button>
            </a-space>
          </a-col>
        </a-row>
        <a-divider style="margin-top: 0"></a-divider>
        <a-table :columns="columns" :data="data" :bordered="false">
          <template #actions="{ record }">
            <a-space>
              <a-tooltip :content="$t('client.action.view')">
                <a-button type="text" size="small" @click="() => {}">
                  <template #icon><icon-eye /></template>
                </a-button>
              </a-tooltip>
              <a-tooltip :content="$t('client.action.edit')">
                <a-button
                  type="text"
                  size="small"
                  status="warning"
                  @click="() => {}"
                >
                  <template #icon><icon-edit /></template>
                </a-button>
              </a-tooltip>
              <a-popconfirm
                :content="$t('client.action.deleteConfirm')"
                @ok="() => {}"
              >
                <a-tooltip :content="$t('client.action.delete')">
                  <a-button type="text" size="small" status="danger">
                    <template #icon><icon-delete /></template>
                  </a-button>
                </a-tooltip>
              </a-popconfirm>
            </a-space>
          </template>
        </a-table>
      </a-card>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref } from 'vue';
  import { useI18n } from 'vue-i18n';

  const { t } = useI18n();

  const columns = [
    {
      title: t('client.column.clientId'),
      dataIndex: 'clientId',
    },
    {
      title: t('client.column.clientName'),
      dataIndex: 'clientName',
    },
    {
      title: t('client.column.actions'),
      slotName: 'actions',
    },
  ];
  const data = ref([]);
</script>

<style scoped lang="less"></style>
