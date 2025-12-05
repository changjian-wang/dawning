<template>
  <div class="system-metadata">
    <div class="container">
      <Breadcrumb :items="['系统管理', '元数据']" />
      <a-card class="general-card">
        <template #title>
          {{ $t('page.title.search.box') }}
        </template>
        <a-row :gutter="12">
          <a-col :span="6">
            <a-form-item label="名称">
              <a-input v-model="model.name" placeholder="请输入..."></a-input>
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
              <a-button type="primary" @click="handleSearch">
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
                    visible = true;
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
          <template #required="{ record }">
            <div v-if="record.required">
              <icon-check style="color: green" />
            </div>
            <div v-else>
              <icon-close />
            </div>
          </template>
          <template #nonEditable="{ record }">
            <div v-if="record.nonEditable">
              <icon-check style="color: green" />
            </div>
            <div v-else>
              <icon-close />
            </div>
          </template>
          <template #optional="{ record }">
            <a-space>
              <a-button
                type="primary"
                @click="
                  () => {
                    $router.push(`system-metadata/${record.id}/info`);
                  }
                "
              >
                <template #icon>
                  <icon-edit />
                </template>
              </a-button>
              <a-button
                @click="
                  Modal.info({
                    title: '查看声明类型',
                    content: `${record.name}: ${record.displayName}`,
                  })
                "
              >
                <template #icon>
                  <icon-eye />
                </template>
              </a-button>
              <a-popconfirm content="确定删除?" @ok="handleDel(record.id)">
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
    </div>
    <div class="add">
      <a-modal
        v-model:visible="visible"
        width="auto"
        title="新增"
        @before-ok="handleValidateForm"
        @ok="handleSubmit"
      >
        <a-form ref="formRef" :rules="rules" :model="form">
          <a-card class="general-card">
            <a-row :gutter="80">
              <a-col :span="12">
                <a-form-item field="name" label="名称" validate-trigger="blur">
                  <a-input
                    v-model="form.name"
                    placeholder="请输入..."
                  ></a-input>
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item field="key" label="键名">
                  <a-input v-model="form.key" placeholder="请输入..."></a-input>
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item field="value" label="有效值">
                  <a-input
                    v-model="form.value"
                    placeholder="请输入..."
                  ></a-input>
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
        </a-form>
      </a-modal>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { reactive, ref, onMounted, onUnmounted } from 'vue';
  import { Modal, PaginationProps } from '@arco-design/web-vue';
  import { FieldRule } from '@arco-design/web-vue';
  import {
    ISystemMetadata,
    ISystemMetadataModel,
    metadata,
  } from '@/api/administration/system-metadata';

  const visible = ref(false);
  const formRef = ref<any>(null);
  const columns = reactive([
    {
      title: '名称',
      dataIndex: 'name',
    },
    {
      title: '键名',
      dataIndex: 'key',
    },
    {
      title: '有效值',
      dataIndex: 'value',
    },
    {
      title: '描述',
      dataIndex: 'description',
    },
    {
      title: '用户可编辑',
      dataIndex: 'nonEditable',
      slotName: 'nonEditable',
    },
    {
      title: '操作',
      slotName: 'optional',
    },
  ]);
  const data = reactive<ISystemMetadata[]>([]);
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
  const model = reactive<ISystemMetadataModel>({
    name: '',
    displayName: '',
    type: '',
    description: '',
  });

  const pagination = reactive<PaginationProps>({
    current: 1,
    pageSize: 10,
    total: 0,
    showTotal: true,
  });

  const fetchData = async () => {
    const result = await metadata.api.getPagedList(
      model,
      pagination.current,
      pagination.pageSize
    );

    pagination.total = result.totalCount;
    pagination.current = result.pageIndex;
    pagination.pageSize = result.pageSize;

    data.splice(0, data.length, ...result.items);
  };

  onMounted(async () => {
    fetchData();
  });

  onUnmounted(() => {
    data.splice(0, data.length);
    metadata.form.reset(form);
  });

  const handleValidateForm = async () => {
    if (await formRef.value.validate()) {
      return false;
    }

    return true;
  };

  const handleSubmit = async () => {
    const result: number = await metadata.api.create(form);
    if (result) {
      metadata.form.reset(form);
      fetchData();
    }
  };

  const handleSearch = async () => {
    fetchData();
  };

  const handleDel = async (id: string) => {
    const result: boolean = await metadata.api.delete(id);
    if (result) {
      fetchData();
    }
  };
</script>
