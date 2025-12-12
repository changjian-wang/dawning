<template>
  <div class="claim-type">
    <div class="container">
      <Breadcrumb
        :items="['menu.administration', 'menu.administration.claim.type']"
      />
      <a-card class="general-card search-card">
        <a-form :model="model" layout="inline" class="search-form">
          <a-row :gutter="[16, 16]" style="width: 100%">
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item field="name" label="名称" class="form-item-block">
                <a-input
                  v-model="model.name"
                  placeholder="请输入名称"
                  allow-clear
                >
                  <template #prefix>
                    <icon-tag />
                  </template>
                </a-input>
              </a-form-item>
            </a-col>
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item field="type" label="类型" class="form-item-block">
                <a-select v-model="model.type" placeholder="请选择类型" allow-clear>
                  <template #prefix>
                    <icon-code />
                  </template>
                  <a-option value="String">String</a-option>
                  <a-option value="Int">Int</a-option>
                  <a-option value="DateTime">DateTime</a-option>
                  <a-option value="Boolean">Boolean</a-option>
                  <a-option value="Enum">Enum</a-option>
                </a-select>
              </a-form-item>
            </a-col>
            <a-col :xs="24" :sm="12" :md="24" :lg="12" :xl="12" class="action-col">
              <a-space :size="12">
                <a-button type="primary" @click="handleSearch">
                  <template #icon><icon-search /></template>
                  查询
                </a-button>
                <a-button @click="handleReset">
                  <template #icon><icon-refresh /></template>
                  重置
                </a-button>
                <a-button type="primary" status="success" @click="handleAdd">
                  <template #icon><icon-plus /></template>
                  新增
                </a-button>
              </a-space>
            </a-col>
          </a-row>
        </a-form>
      </a-card>
      <a-card class="general-card table-card">
        <a-table
          :columns="columns"
          :data="data"
          :pagination="{ ...pagination, onChange: handlePaginationChange }"
          :bordered="false"
        >
          <template #required="{ record }">
            <a-tag v-if="record.required" color="green" size="small">
              <template #icon><icon-check-circle-fill /></template>
              必需
            </a-tag>
            <a-tag v-else color="gray" size="small">
              <template #icon><icon-minus-circle-fill /></template>
              可选
            </a-tag>
          </template>
          <template #nonEditable="{ record }">
            <a-tag v-if="!record.nonEditable" color="arcoblue" size="small">
              <template #icon><icon-edit /></template>
              可编辑
            </a-tag>
            <a-tag v-else color="orange" size="small">
              <template #icon><icon-lock /></template>
              锁定
            </a-tag>
          </template>
          <template #optional="{ record }">
            <a-space>
              <a-button type="text" size="medium" @click="handleView(record)">
                <template #icon><icon-eye :size="18" /></template>
              </a-button>
              <a-button
                type="text"
                size="medium"
                status="warning"
                @click="handleEdit(record)"
              >
                <template #icon><icon-edit :size="18" /></template>
              </a-button>
              <a-popconfirm content="确定要删除此声明类型吗？" @ok="handleDel(record.id)">
                <a-button type="text" size="medium" status="danger">
                  <template #icon><icon-delete :size="18" /></template>
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
  import { FieldRule, Modal, PaginationProps } from '@arco-design/web-vue';

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
  const data = ref<IClaimType[]>([]);
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
    try {
      const result = await claimType.api.getPagedList(
        model,
        pagination.current,
        pagination.pageSize
      );

      pagination.total = result.totalCount;
      pagination.current = result.pageIndex;
      pagination.pageSize = result.pageSize;

      data.value = result.items || [];
    } catch (error) {
      console.error('加载声明类型数据失败:', error);
      // 不显示错误消息，因为后端API可能还未实现
    }
  };

  const handlePaginationChange = (page: number) => {
    pagination.current = page;
    fetchData();
  };

  onMounted(async () => {
    fetchData();
  });

  onUnmounted(() => {
    data.value = [];
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

  const handleReset = () => {
    Object.assign(model, {
      name: '',
      displayName: '',
      type: '',
      description: '',
    });
    fetchData();
  };

  const handleAdd = () => {
    visible.value = true;
  };

  const handleView = (record: IClaimType) => {
    Modal.info({
      title: '查看声明类型',
      content: `名称: ${record.name}\n显示名称: ${record.displayName}\n类型: ${record.type}\n描述: ${record.description}`,
    });
  };

  const handleEdit = (record: IClaimType) => {
    // 跳转到编辑页面
    // router.push(`/administration/claim-type/${record.id}/info`);
    // 或者打开弹窗编辑
    Object.assign(form, record);
    visible.value = true;
  };

  const handleDel = async (id: string) => {
    const result: boolean = await claimType.api.delete(id);
    if (result) {
      fetchData();
    }
  };
</script>

<style scoped lang="less">
  .claim-type {
    .search-card {
      margin-bottom: 16px;
    }

    .search-form {
      :deep(.arco-form-item) {
        margin-bottom: 0;
      }

      .form-item-block {
        width: 100%;
        
        :deep(.arco-form-item-wrapper-col) {
          width: 100%;
        }
      }

      .action-col {
        display: flex;
        align-items: flex-end;
        justify-content: flex-end;

        :deep(.arco-btn) {
          font-weight: 500;
          border-radius: 4px;
          transition: all 0.3s ease;

          &.arco-btn-primary {
            &:not(.arco-btn-status-success) {
              background-color: #165dff;
              border-color: #165dff;

              &:hover {
                background-color: #4080ff;
                border-color: #4080ff;
              }
            }

            &.arco-btn-status-success {
              background-color: #00b42a;
              border-color: #00b42a;

              &:hover {
                background-color: #23c343;
                border-color: #23c343;
              }
            }
          }

          &.arco-btn-secondary {
            &:hover {
              background-color: #f2f3f5;
            }
          }
        }
      }
    }

    .table-card {
      box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
      border-radius: 8px;
    }

    :deep(.arco-table-th) {
      background-color: #f7f8fa;
      font-weight: 600;
    }

    :deep(.arco-table-tr:hover) {
      background-color: #f7f8fa;
    }
  }
</style>
