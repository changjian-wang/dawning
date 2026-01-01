import localeMessageBox from '@/components/message-box/locale/zh-CN';
import localeLogin from '@/views/login/locale/zh-CN';
import localeUserCenter from '@/views/user/locale/zh-CN';

import localeWorkplace from '@/views/dashboard/workplace/locale/zh-CN';

import localeUser from '@/views/administration/user/locale/zh-CN';
import localeRole from '@/views/administration/role/locale/zh-CN';
import localePermission from '@/views/administration/permission/locale/zh-CN';
import localeApplication from '@/views/administration/openiddict/application/locale/zh-CN';
import localeScope from '@/views/administration/openiddict/scope/locale/zh-CN';
import localeApiResource from '@/views/administration/openiddict/api-resource/locale/zh-CN';
import localeIdentityResource from '@/views/administration/openiddict/identity-resource/locale/zh-CN';
import localeAuthorization from '@/views/administration/openiddict/authorization/locale/zh-CN';
import localeOpenIddict from '@/views/administration/openiddict/locale/zh-CN';
import localeClient from '@/views/administration/openiddict/client/locale/zh-CN';
import localeClaimType from '@/views/administration/claim-type/locale/zh-CN';
import localeAuditLog from '@/views/administration/audit-log/locale/zh-CN';
import localeSystemLog from '@/views/administration/system-log/locale/zh-CN';
import localeGateway from '@/views/administration/gateway/locale/zh-CN';
import localeSystemConfig from '@/views/administration/system-config/locale/zh-CN';
import localeSystemMonitor from '@/views/administration/system-monitor/locale/zh-CN';
import localeAlert from '@/views/administration/alert/locale/zh-CN';
import localeTenant from '@/views/administration/tenant/locale/zh-CN';

import localeSettings from './zh-CN/settings';

export default {
  // 通用翻译
  'common.search': '搜索',
  'common.reset': '重置',
  'common.create': '新建',
  'common.add': '新增',
  'common.edit': '编辑',
  'common.delete': '删除',
  'common.operations': '操作',
  'common.confirm': '确认',
  'common.cancel': '取消',
  'common.save': '保存',
  'common.detail': '详情',
  'common.deleteConfirm': '确定要删除吗？',
  'common.all': '全部',
  'common.advancedSearch': '高级搜索',
  'common.createdAt': '创建时间',
  'common.updatedAt': '更新时间',
  'common.actions': '操作',
  'common.system': '系统',
  'common.operationFailed': '操作失败',
  'common.operationSuccess': '操作成功',
  'common.error': '请求失败',
  'common.loadFailed': '加载数据失败',
  'common.createSuccess': '创建成功',
  'common.createFailed': '创建失败',
  'common.updateSuccess': '更新成功',
  'common.updateFailed': '更新失败',
  'common.deleteSuccess': '删除成功',
  'common.deleteFailed': '删除失败',

  'menu.dashboard': '仪表盘',
  'menu.server.dashboard': '仪表盘-服务端',
  'menu.server.workplace': '工作台-服务端',
  'menu.server.monitor': '实时监控-服务端',
  'menu.list': '列表页',
  'menu.result': '结果页',
  'menu.exception': '异常页',
  'menu.form': '表单页',
  'menu.profile': '详情页',
  'menu.visualization': '数据可视化',
  'menu.user': '个人中心',
  'menu.arcoWebsite': 'Arco Design',
  'menu.faq': '常见问题',
  'navbar.docs': '文档中心',
  'navbar.action.locale': '切换为中文',
  // --begin--
  'menu.administration': '系统管理',
  'menu.administration.tenant': '租户管理',
  'menu.administration.userPermission': '用户权限',
  'menu.administration.openiddict': '认证授权',
  'menu.administration.monitoring': '监控告警',
  'menu.administration.settings': '系统设置',
  'menu.data.list': '数据列表',
  'menu.add': '新增信息',
  'menu.info': '基本信息',
  'page.title.search.box': '查询条件',
  'page.title.data.list': '数据列表',
  'page.button.search': '搜索',
  'searchTable.form.search': '查询',
  'searchTable.form.reset': '重置',
  // --end--
  ...localeUser,
  ...localeRole,
  ...localePermission,
  ...localeApplication,
  ...localeScope,
  ...localeApiResource,
  ...localeIdentityResource,
  ...localeAuthorization,
  ...localeClient,
  ...localeClaimType,
  ...localeAuditLog,
  ...localeSystemLog,
  ...localeGateway,
  ...localeSystemConfig,
  ...localeSystemMonitor,
  ...localeAlert,
  ...localeTenant,
  ...localeOpenIddict,

  ...localeSettings,
  ...localeMessageBox,
  ...localeLogin,
  ...localeWorkplace,
  ...localeUserCenter,
};
