import localeMessageBox from '@/components/message-box/locale/en-US';
import localeLogin from '@/views/login/locale/en-US';
import localeUserCenter from '@/views/user/locale/en-US';

import localeWorkplace from '@/views/dashboard/workplace/locale/en-US';

import localeUser from '@/views/administration/user/locale/en-US';
import localeRole from '@/views/administration/role/locale/en-US';
import localePermission from '@/views/administration/permission/locale/en-US';
import localeApplication from '@/views/administration/openiddict/application/locale/en-US';
import localeScope from '@/views/administration/openiddict/scope/locale/en-US';
import localeOpenIddict from '@/views/administration/openiddict/locale/en-US';
import localeClaimType from '@/views/administration/claim-type/locale/en-US';
import localeSystemMetadata from '@/views/administration/system-metadata/locale/en-US';
import localeAuditLog from '@/views/administration/audit-log/locale/en-US';
import localeSystemLog from '@/views/administration/system-log/locale/en-US';
import localeGateway from '@/views/administration/gateway/locale/en-US';
import localeSystemConfig from '@/views/administration/system-config/locale/en-US';
import localeSystemMonitor from '@/views/administration/system-monitor/locale/en-US';
import localeAlert from '@/views/administration/alert/locale/en-US';

import localeSettings from './en-US/settings';

export default {
  'menu.dashboard': 'Dashboard',
  'menu.server.dashboard': 'Dashboard-Server',
  'menu.server.workplace': 'Workplace-Server',
  'menu.server.monitor': 'Monitor-Server',
  'menu.list': 'List',
  'menu.result': 'Result',
  'menu.exception': 'Exception',
  'menu.form': 'Form',
  'menu.profile': 'Profile',
  'menu.visualization': 'Data Visualization',
  'menu.user': 'User Center',
  'menu.arcoWebsite': 'Arco Design',
  'menu.faq': 'FAQ',
  'navbar.docs': 'Docs',
  'navbar.action.locale': 'Switch to English',
  // --begin--
  'menu.administration': 'Administration',
  'menu.data.list': 'Data List',
  'menu.add': 'Add',
  'menu.info': 'Info',
  'page.title.search.box': 'Search Box',
  'page.title.data.list': 'Data List',
  'page.button.search': 'Search',
  'searchTable.form.search': 'Search',
  'searchTable.form.reset': 'Reset',
  // --end--
  ...localeUser,
  ...localeRole,
  ...localePermission,
  ...localeApplication,
  ...localeScope,
  ...localeClaimType,
  ...localeSystemMetadata,
  ...localeAuditLog,
  ...localeSystemLog,
  ...localeGateway,
  ...localeSystemConfig,
  ...localeSystemMonitor,
  ...localeAlert,
  ...localeOpenIddict,

  ...localeSettings,
  ...localeMessageBox,
  ...localeLogin,
  ...localeWorkplace,
  ...localeUserCenter,
};
