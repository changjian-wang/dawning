import localeMessageBox from '@/components/message-box/locale/en-US';
import localeLogin from '@/views/login/locale/en-US';

import localeWorkplace from '@/views/dashboard/workplace/locale/en-US';

import localeClaimType from '@/views/administration/claim-type/locale/en-US';

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
  'menu.administration.openiddict': 'Authorization and Authentication Service',
  'menu.administration.openiddict.application': 'Applications',
  'menu.administration.openiddict.scope': 'Scopes',
  'menu.administration.openiddict.client': 'Clients',
  'menu.administration.openiddict.api.resource': 'API Resources',
  'menu.administration.openiddict.identity.resource': 'Identity Resources',
  'menu.data.list': 'Data List',
  'menu.add': 'Add',
  'menu.info': 'Info',
  'page.title.search.box': 'Search Box',
  'page.title.data.list': 'Data List',
  'page.button.search': 'Search',
  // --end--
  ...localeClaimType,

  ...localeSettings,
  ...localeMessageBox,
  ...localeLogin,
  ...localeWorkplace,
};
