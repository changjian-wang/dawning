<template>
  <div class="claim-type">
    <div class="container">
      <Breadcrumb :items="['认证授权', '声明类型']" />
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
        <a-table
          :columns="columns"
          :data="data"
          :pagination="{ ...pagination, onChange: handlePaginationChange }"
          :bordered="false"
        >
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
                    $router.push(`claim-type/${record.id}/info`);
                  }
                "
              >
                <template #icon>
                  <icon-edit />
                </template>
              </a-button>
              <a-button
                @click="
                  $modal.info({
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
                <a-form-item label="显示名称">
                  <a-input
                    v-model="form.displayName"
                    placeholder="请输入..."
                  ></a-input>
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item label="类型">
                  <a-select v-model="form.type" placeholder="请选择...">
                    <a-option>String</a-option>
                    <a-option>Int</a-option>
                    <a-option>DateTime</a-option>
                    <a-option>Boolean</a-option>
                    <a-option>Enum</a-option>
                  </a-select>
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item label="是否必须项">
                  <a-switch v-model="form.required">
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
                <a-form-item label="说明">
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
  import {
    IClaimType,
    IClaimTypeModel,
    claimType,
  } from '@/api/administration/claim-type';
  import { FieldRule, Pagination, PaginationProps } from '@arco-design/web-vue';

  const visible = ref(false);
  const formRef = ref<any>(null);
  const columns = reactive([
    {
      title: '名称',
      dataIndex: 'name',
    },
    {
      title: '显示名称',
      dataIndex: 'displayName',
    },
    {
      title: '类型',
      dataIndex: 'type',
    },
    {
      title: '描述',
      dataIndex: 'description',
    },
    {
      title: '是否必要',
      dataIndex: 'required',
      slotName: 'required',
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
  const data = reactive<IClaimType[]>([]);
  const form = reactive<IClaimType>({ ...claimType.form.create() });
  const rules: Record<string, FieldRule<any> | FieldRule<any>[]> | undefined = {
    name: [
      {
        required: true,
        message: '不能为空',
      },
    ],
  };
  const model = reactive<IClaimTypeModel>({
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
    const result = await claimType.api.getPagedList(
      model,
      pagination.current,
      pagination.pageSize
    );

    pagination.total = result.totalCount;
    pagination.current = result.pageIndex;
    pagination.pageSize = result.pageSize;

    data.splice(0, data.length, ...result.items);
  };

  const handlePaginationChange = (page: number) => {
    pagination.current = page;
    fetchData();
  };

  onMounted(async () => {
    fetchData();
  });

  onUnmounted(() => {
    data.splice(0, data.length);
    claimType.form.reset(form);
  });

  const handleValidateForm = async () => {
    if (await formRef.value.validate()) {
      return false;
    }

    return true;
  };

  const handleSubmit = async () => {
    const result: number = await claimType.api.create(form);
    if (result) {
      claimType.form.reset(form);
      fetchData();
    }
  };

  const handleSearch = async () => {
    fetchData();
  };

  const handleDel = async (id: string) => {
    const result: boolean = await claimType.api.delete(id);
    if (result) {
      fetchData();
    }
  };
</script>

<style scoped lang="less"></style>
