<template>
  <div class="container">
    <Breadcrumb :items="['menu.form', 'menu.administration.ids.client']" />
    <a-card class="general-card">
      <template #title>
        {{ $t('page.title.search.box') }}
      </template>
      <a-row :gutter="12">
        <a-col :span="6">
          <a-form-item label="名称">
            <a-input placeholder="请输入..."></a-input>
          </a-form-item>
        </a-col>
        <a-col :span="6">
          <a-form-item label="类型">
            <a-select placeholder="请选择...">
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
            <a-button type="primary" @click="() => {}">
              <template #icon>
                <icon-search />
              </template>
              {{ '查询' }}
            </a-button>
            <a-button @click="() => {}">
              <template #icon>
                <icon-refresh />
              </template>
              {{ '重置' }}
            </a-button>
            <a-button
              type="primary"
              class="add"
              @click="
                () => {
                  $router.push('api-resource/add');
                }
              "
            >
              <template #icon>
                <icon-plus />
              </template>
            </a-button>
          </a-space>
        </a-col>
      </a-row>
      <a-divider style="margin-top: 0"></a-divider>
      <a-table :columns="columns" :data="data" :bordered="false">
        <template #optional="{ record }">
          <a-space>
            <a-button type="primary" @click="() => {}">
              <template #icon>
                <icon-edit />
              </template>
            </a-button>
            <a-button
              @click="
                $modal.info({
                  title: 'Name',
                  content: `${record.clientId} ${record.clientName}`,
                })
              "
            >
              <template #icon>
                <icon-eye />
              </template>
            </a-button>
          </a-space>
        </template>
      </a-table>
    </a-card>
  </div>
</template>

<script lang="ts" setup>
  import { reactive, ref } from 'vue';

  const formData = ref<any>({});
  const formRef = ref<any>();

  const columns = [
    {
      title: 'Id',
      dataIndex: 'id',
    },
    {
      title: 'API资源名称',
      dataIndex: 'apiResourceName',
    },
    {
      title: '操作',
      slotName: 'optional',
    },
  ];
  const data = reactive([]);
</script>

<style scoped lang="less"></style>
