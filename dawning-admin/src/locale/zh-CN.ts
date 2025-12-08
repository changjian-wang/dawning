import localeMessageBox from '@/components/message-box/locale/zh-CN';
import localeLogin from '@/views/login/locale/zh-CN';

import localeWorkplace from '@/views/dashboard/workplace/locale/zh-CN';

import localeClaimType from '@/views/administration/claim-type/locale/zh-CN';

import localeSettings from './zh-CN/settings';

export default {
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
  'menu.administration': '管理',
  'menu.administration.user': '用户管理',
  'menu.administration.openiddict': '认证授权服务',
  'menu.administration.openiddict.application': '应用程序',
  'menu.administration.openiddict.client': '客户端',
  'menu.administration.openiddict.api.resource': 'API资源',
  'menu.administration.openiddict.identity.resource': '身份资源',
  'menu.data.list': '数据列表',
  'menu.add': '新增信息',
  'menu.info': '基本信息',
  'page.title.search.box': '查询条件',
  'page.title.data.list': '数据列表',
  'page.button.search': '搜索',
  // --end--
  ...localeClaimType,

  ...localeSettings,
  ...localeMessageBox,
  ...localeLogin,
  ...localeWorkplace,
};
