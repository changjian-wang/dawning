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
              <a-form-item field="name" :label="$t('claimType.search.name')" class="form-item-block">
                <a-input
                  v-model="model.name"
                  :placeholder="$t('claimType.placeholder.name')"
                  allow-clear
                >
                  <template #prefix>
                    <icon-tag />
                  </template>
                </a-input>
              </a-form-item>
            </a-col>
            <a-col :xs="24" :sm="12" :md="8" :lg="6" :xl="6">
              <a-form-item field="type" :label="$t('claimType.search.type')" class="form-item-block">
                <a-select
                  v-model="model.type"
                  :placeholder="$t('claimType.placeholder.type')"
                  allow-clear
                >
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
            <a-col
              :xs="24"
              :sm="12"
              :md="24"
              :lg="12"
              :xl="12"
              class="action-col"
            >
              <a-space :size="12">
                <a-button type="primary" @click="handleSearch">
                  <template #icon><icon-search /></template>
                  {{ $t('claimType.button.search') }}
                </a-button>
                <a-button @click="handleReset">
                  <template #icon><icon-refresh /></template>
                  {{ $t('claimType.button.reset') }}
                </a-button>
                <a-button type="primary" status="success" @click="handleAdd">
                  <template #icon><icon-plus /></template>
                  {{ $t('claimType.button.add') }}
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
          :stripe="true"
        >
          <template #required="{ record }">
            <a-tag v-if="record.required" color="green" size="small">
              <template #icon><icon-check-circle-fill /></template>
              {{ $t('claimType.tag.required') }}
            </a-tag>
            <a-tag v-else color="gray" size="small">
              <template #icon><icon-minus-circle-fill /></template>
              {{ $t('claimType.tag.optional') }}
            </a-tag>
          </template>
          <template #nonEditable="{ record }">
            <a-tag v-if="!record.nonEditable" color="arcoblue" size="small">
              <template #icon><icon-edit /></template>
              {{ $t('claimType.tag.editable') }}
            </a-tag>
            <a-tag v-else color="orange" size="small">
              <template #icon><icon-lock /></template>
              {{ $t('claimType.tag.locked') }}
            </a-tag>
          </template>
          <template #optional="{ record }">
            <a-space>
              <a-tooltip :content="$t('claimType.action.view')">
                <a-button type="text" size="small" @click="handleView(record)">
                  <template #icon><icon-eye /></template>
                </a-button>
              </a-tooltip>
              <a-tooltip :content="$t('claimType.action.edit')">
                <a-button
                  type="text"
                  size="small"
                  status="warning"
                  @click="handleEdit(record)"
                >
                  <template #icon><icon-edit /></template>
                </a-button>
              </a-tooltip>
              <a-popconfirm
                :content="$t('claimType.delete.confirm')"
                @ok="handleDel(record.id)"
              >
                <a-tooltip :content="$t('claimType.action.delete')">
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
    <div class="add">
      <a-modal
        v-model:visible="visible"
        width="auto"
        :title="isEditMode ? $t('claimType.modal.edit') : $t('claimType.modal.add')"
        :ok-loading="submitLoading"
        @before-ok="handleBeforeOk"
      >
        <a-form ref="formRef" :rules="rules" :model="form" layout="vertical">
          <a-card class="general-card">
            <a-row :gutter="80">
              <a-col :span="12">
                <a-form-item field="name" :label="$t('claimType.form.name')" validate-trigger="blur">
                  <a-input
                    v-model="form.name"
                    :placeholder="$t('claimType.placeholder.input')"
                  ></a-input>
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item :label="$t('claimType.form.displayName')">
                  <a-input
                    v-model="form.displayName"
                    :placeholder="$t('claimType.placeholder.input')"
                  ></a-input>
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item :label="$t('claimType.form.type')">
                  <a-select v-model="form.type" :placeholder="$t('claimType.placeholder.select')">
                    <a-option>String</a-option>
                    <a-option>Int</a-option>
                    <a-option>DateTime</a-option>
                    <a-option>Boolean</a-option>
                    <a-option>Enum</a-option>
                  </a-select>
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item :label="$t('claimType.form.required')">
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
                <a-form-item :label="$t('claimType.form.description')">
                  <a-textarea
                    v-model="form.description"
                    :placeholder="$t('claimType.placeholder.input')"
                  />
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item :label="$t('claimType.form.nonEditable')">
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

    <!-- 查看详情弹窗 -->
    <a-modal
      v-model:visible="viewVisible"
      :title="$t('claimType.modal.detail')"
      width="560px"
      :footer="false"
    >
      <div class="detail-content">
        <div class="detail-row">
          <span class="label">{{ $t('claimType.detail.name') }}</span>
          <span class="value">{{ currentRecord?.name || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('claimType.detail.displayName') }}</span>
          <span class="value">{{ currentRecord?.displayName || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('claimType.detail.type') }}</span>
          <span class="value">{{ currentRecord?.type || '-' }}</span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('claimType.detail.required') }}</span>
          <span class="value">
            <a-tag v-if="currentRecord?.required" color="green" size="small">{{ $t('claimType.tag.required') }}</a-tag>
            <a-tag v-else color="gray" size="small">{{ $t('claimType.tag.optional') }}</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('claimType.detail.editable') }}</span>
          <span class="value">
            <a-tag
              v-if="!currentRecord?.nonEditable"
              color="arcoblue"
              size="small"
              >{{ $t('claimType.tag.editable') }}</a-tag
            >
            <a-tag v-else color="orange" size="small">{{ $t('claimType.tag.locked') }}</a-tag>
          </span>
        </div>

        <div class="detail-row">
          <span class="label">{{ $t('claimType.detail.description') }}</span>
          <span class="value">{{ currentRecord?.description || '-' }}</span>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
  import { reactive, ref, computed, onMounted, onUnmounted } from 'vue';
  import { useI18n } from 'vue-i18n';
  import {
    IClaimType,
    IClaimTypeModel,
    claimType,
  } from '@/api/administration/claim-type';
  import { FieldRule, PaginationProps } from '@arco-design/web-vue';

  const { t } = useI18n();

  const visible = ref(false);
  const viewVisible = ref(false);
  const currentRecord = ref<IClaimType | null>(null);
  const formRef = ref<any>(null);
  const isEditMode = ref(false);

  const columns = computed(() => [
    {
      title: t('claimType.column.name'),
      dataIndex: 'name',
    },
    {
      title: t('claimType.column.displayName'),
      dataIndex: 'displayName',
    },
    {
      title: t('claimType.column.type'),
      dataIndex: 'type',
    },
    {
      title: t('claimType.column.description'),
      dataIndex: 'description',
    },
    {
      title: t('claimType.column.required'),
      dataIndex: 'required',
      slotName: 'required',
    },
    {
      title: t('claimType.column.nonEditable'),
      dataIndex: 'nonEditable',
      slotName: 'nonEditable',
    },
    {
      title: t('common.actions'),
      slotName: 'optional',
      width: 120,
      align: 'center',
    },
  ]);
  const data = ref<IClaimType[]>([]);
  const form = reactive<IClaimType>({ ...claimType.form.create() });
  const rules = computed<Record<string, FieldRule<any> | FieldRule<any>[]>>(() => ({
    name: [
      {
        required: true,
        message: t('claimType.validation.nameRequired'),
      },
    ],
  }));
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

  const submitLoading = ref(false);

  const handleBeforeOk = async (done: (closed: boolean) => void) => {
    const errors = await formRef.value?.validate();
    if (errors) {
      done(false);
      return;
    }

    try {
      submitLoading.value = true;
      const result: number = await claimType.api.create(form);
      if (result) {
        claimType.form.reset(form);
        fetchData();
        done(true);
      } else {
        done(false);
      }
    } catch (error) {
      done(false);
    } finally {
      submitLoading.value = false;
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
    isEditMode.value = false;
    claimType.form.reset(form);
    visible.value = true;
  };

  const handleView = (record: IClaimType) => {
    currentRecord.value = record;
    viewVisible.value = true;
  };

  const handleEdit = (record: IClaimType) => {
    isEditMode.value = true;
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

  .detail-content {
    .detail-row {
      display: flex;
      padding: 14px 0;
      border-bottom: 1px solid var(--color-border-1);
      align-items: flex-start;

      &:last-child {
        border-bottom: none;
      }

      .label {
        width: 90px;
        flex-shrink: 0;
        font-size: 14px;
        color: var(--color-text-3);
        line-height: 1.5;
      }

      .value {
        flex: 1;
        font-size: 14px;
        color: var(--color-text-1);
        line-height: 1.5;
        word-break: break-word;
      }
    }
  }
</style>
