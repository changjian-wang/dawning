<template>
  <a-modal
    v-model:visible="visible"
    title="键盘快捷键"
    :footer="false"
    width="600px"
    :mask-closable="true"
  >
    <div class="keyboard-shortcuts">
      <div class="shortcut-section">
        <h3 class="section-title">
          <icon-compass /> 全局导航
        </h3>
        <div class="shortcut-list">
          <div class="shortcut-item">
            <span class="shortcut-key">Alt + H</span>
            <span class="shortcut-desc">返回首页</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Alt + U</span>
            <span class="shortcut-desc">用户管理</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Alt + R</span>
            <span class="shortcut-desc">角色管理</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Alt + P</span>
            <span class="shortcut-desc">权限管理</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Alt + L</span>
            <span class="shortcut-desc">审计日志</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Alt + S</span>
            <span class="shortcut-desc">系统配置</span>
          </div>
        </div>
      </div>

      <div class="shortcut-section">
        <h3 class="section-title">
          <icon-edit /> 表格操作
        </h3>
        <div class="shortcut-list">
          <div class="shortcut-item">
            <span class="shortcut-key">Ctrl + N</span>
            <span class="shortcut-desc">新增记录</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Ctrl + E</span>
            <span class="shortcut-desc">编辑选中记录</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Delete</span>
            <span class="shortcut-desc">删除选中记录</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Ctrl + F</span>
            <span class="shortcut-desc">搜索/筛选</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Ctrl + R</span>
            <span class="shortcut-desc">刷新数据</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Ctrl + Shift + E</span>
            <span class="shortcut-desc">导出数据</span>
          </div>
        </div>
      </div>

      <div class="shortcut-section">
        <h3 class="section-title">
          <icon-info-circle /> 其他
        </h3>
        <div class="shortcut-list">
          <div class="shortcut-item">
            <span class="shortcut-key">Shift + ?</span>
            <span class="shortcut-desc">显示此帮助</span>
          </div>
          <div class="shortcut-item">
            <span class="shortcut-key">Esc</span>
            <span class="shortcut-desc">关闭弹窗/取消操作</span>
          </div>
        </div>
      </div>
    </div>
  </a-modal>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

const props = defineProps<{
  modelValue: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
}>();

const visible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
});
</script>

<style lang="less" scoped>
.keyboard-shortcuts {
  .shortcut-section {
    margin-bottom: 24px;

    &:last-child {
      margin-bottom: 0;
    }

    .section-title {
      font-size: 14px;
      font-weight: 600;
      color: var(--color-text-1);
      margin-bottom: 12px;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .shortcut-list {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 8px 24px;

      .shortcut-item {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 8px 12px;
        background: var(--color-fill-1);
        border-radius: 4px;

        .shortcut-key {
          font-family: monospace;
          font-size: 12px;
          font-weight: 500;
          padding: 4px 8px;
          background: var(--color-bg-white);
          border: 1px solid var(--color-border);
          border-radius: 4px;
          box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
        }

        .shortcut-desc {
          color: var(--color-text-2);
          font-size: 13px;
        }
      }
    }
  }
}
</style>
