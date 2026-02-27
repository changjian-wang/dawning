---
description: "Create Vue 3 admin page with Arco Design: list, form, i18n, CRUD operations. Trigger: 创建页面, create page, Vue页面, 新建页面, 前端页面, vue page, 管理页面"
---

# Create Vue Page Skill

## 目标

创建完整的 Vue 3 管理后台页面（列表 + CRUD + 国际化）。

## 触发条件

- **关键词**：创建页面, create page, Vue页面, 新建页面, 前端页面, vue page, 管理页面, 列表页
- **文件模式**：`*.vue`, `src/views/**`, `src/api/*.ts`
- **用户意图**：创建新的管理后台页面

## 编排

- **前置**：`create-api`（先有后端接口）
- **后续**：无

## Skill 使用日志

使用本 skill 后，在 `/memories/repo/skill-usage.md` 追加一行：`- {日期} create-vue-page — {触发原因}`

---

## 目录结构

```
views/{module}/{feature}/
├── index.vue              # 主页面
├── components/            # 页面私有组件
│   ├── EditModal.vue      # 编辑弹窗
│   └── DetailDrawer.vue   # 详情抽屉
└── locale/                # 国际化
    ├── zh-CN.ts
    └── en-US.ts
```

## 创建流程

### 1. 创建 API 接口

```typescript
// src/api/{feature}.ts
import axios from '@/api/interceptor';
import type { IPagedData } from './paged-data';

export interface {Feature}Info {
  id: string;
  // 业务字段
}

export function get{Feature}List(params: {Feature}QueryParams) {
  return axios.get<IPagedData<{Feature}Info>>('/api/{feature}', { params });
}

export function get{Feature}ById(id: string) {
  return axios.get<{Feature}Info>(`/api/{feature}/${id}`);
}

export function create{Feature}(data: Create{Feature}Request) {
  return axios.post<{ id: string }>('/api/{feature}', data);
}

export function update{Feature}(id: string, data: Update{Feature}Request) {
  return axios.put(`/api/{feature}/${id}`, data);
}

export function delete{Feature}(id: string) {
  return axios.delete(`/api/{feature}/${id}`);
}
```

### 2. 创建主页面

```vue
<template>
  <div class="container">
    <!-- 搜索区域 -->
    <a-card class="search-card">
      <a-form :model="searchForm" layout="inline" @submit="handleSearch">
        <a-form-item field="keyword" :label="t('{feature}.keyword')">
          <a-input v-model="searchForm.keyword" :placeholder="t('common.pleaseInput')" />
        </a-form-item>
        <a-form-item>
          <a-space>
            <a-button type="primary" html-type="submit">
              <template #icon><icon-search /></template>
              {{ t('common.search') }}
            </a-button>
            <a-button @click="resetSearch">
              <template #icon><icon-refresh /></template>
              {{ t('common.reset') }}
            </a-button>
          </a-space>
        </a-form-item>
      </a-form>
    </a-card>

    <!-- 表格区域 -->
    <a-card class="table-card">
      <template #extra>
        <a-button type="primary" @click="handleAdd">
          <template #icon><icon-plus /></template>
          {{ t('common.add') }}
        </a-button>
      </template>

      <a-table
        :columns="columns"
        :data="tableData"
        :loading="loading"
        :pagination="pagination"
        @page-change="handlePageChange"
      >
        <template #operations="{ record }">
          <a-space>
            <a-button type="text" size="mini" @click="handleEdit(record)">
              {{ t('common.edit') }}
            </a-button>
            <a-popconfirm :content="t('common.deleteConfirm')" @ok="handleDelete(record.id)">
              <a-button type="text" status="danger" size="mini">
                {{ t('common.delete') }}
              </a-button>
            </a-popconfirm>
          </a-space>
        </template>
      </a-table>
    </a-card>

    <EditModal v-model:visible="editModalVisible" :id="editingId" @success="fetchData" />
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { Message } from '@arco-design/web-vue';
import { get{Feature}List, delete{Feature} } from '@/api/{feature}';
import type { {Feature}Info } from '@/api/{feature}';
import EditModal from './components/EditModal.vue';

const { t } = useI18n();

const searchForm = reactive({ keyword: '' });
const pagination = reactive({ current: 1, pageSize: 20, total: 0 });
const loading = ref(false);
const tableData = ref<{Feature}Info[]>([]);
const editModalVisible = ref(false);
const editingId = ref<string | undefined>();

const columns = computed(() => [
  { title: 'ID', dataIndex: 'id', width: 200 },
  // 其他列...
  { title: t('common.operations'), slotName: 'operations', width: 150 },
]);

const fetchData = async () => {
  loading.value = true;
  try {
    const { data } = await get{Feature}List({
      ...searchForm,
      page: pagination.current,
      pageSize: pagination.pageSize,
    });
    tableData.value = data.items;
    pagination.total = data.total;
  } finally {
    loading.value = false;
  }
};

const handleSearch = () => { pagination.current = 1; fetchData(); };
const resetSearch = () => { searchForm.keyword = ''; handleSearch(); };
const handlePageChange = (page: number) => { pagination.current = page; fetchData(); };
const handleAdd = () => { editingId.value = undefined; editModalVisible.value = true; };
const handleEdit = (record: {Feature}Info) => { editingId.value = record.id; editModalVisible.value = true; };
const handleDelete = async (id: string) => {
  await delete{Feature}(id);
  Message.success(t('common.deleteSuccess'));
  fetchData();
};

onMounted(fetchData);
</script>

<style lang="less" scoped>
.container { padding: 20px; }
.search-card { margin-bottom: 16px; }
</style>
```

### 3. 创建国际化文件

```typescript
// locale/zh-CN.ts
export default {
  'menu.{module}.{feature}': '{Feature}管理',
  '{feature}.keyword': '关键词',
};

// locale/en-US.ts
export default {
  'menu.{module}.{feature}': '{Feature} Management',
  '{feature}.keyword': 'Keyword',
};
```

### 4. 添加路由

在路由配置中添加新页面路由。

## 规范要求

- 使用 Composition API + `<script setup>`
- 使用 Arco Design Vue 组件
- 实现搜索、分页、CRUD 功能
- 添加国际化支持（zh-CN, en-US）
- 操作后显示成功/失败消息
- 删除前显示确认提示

## 验收场景

- **输入**："创建一个产品管理页面"
- **预期**：agent 生成 API 接口、列表页、编辑弹窗、国际化文件
- **上次验证**：2026-02-27 ✅
