export default {
  'menu.administration.application': '应用程序',
  'menu.administration.openiddict.application': '应用程序',
  'application.title': '应用程序管理',

  // Search form
  'application.form.clientId': '客户端ID',
  'application.form.clientId.placeholder': '请输入客户端ID',
  'application.form.displayName': '显示名称',
  'application.form.displayName.placeholder': '请输入显示名称',
  'application.form.type': '类型',
  'application.form.type.placeholder': '请选择类型',
  'application.form.type.all': '全部',
  'application.form.type.confidential': '机密',
  'application.form.type.public': '公共',

  // Buttons
  'application.button.search': '查询',
  'application.button.reset': '重置',
  'application.button.add': '新增',

  // Table columns
  'application.column.clientId': '客户端ID',
  'application.column.displayName': '显示名称',
  'application.column.type': '类型',
  'application.column.consentType': '同意类型',
  'application.column.permissions': '权限',
  'application.column.createdAt': '创建时间',
  'application.column.action': '操作',

  // Consent types
  'application.consentType.explicit': '显式',
  'application.consentType.implicit': '隐式',
  'application.consentType.systematic': '系统',

  // Modal
  'application.modal.add': '新增应用程序',
  'application.modal.edit': '编辑应用程序',
  'application.modal.detail': '应用程序详情',

  // Form fields
  'application.form.clientSecret': '客户端密钥',
  'application.form.clientSecret.placeholder': '请输入客户端密钥',
  'application.form.clientType': '客户端类型',
  'application.form.clientType.placeholder': '请选择',
  'application.form.consentType': '同意类型',
  'application.form.consentType.placeholder': '请选择',
  'application.form.grantTypes': '授权流程',
  'application.form.grantTypes.password': '密码模式',
  'application.form.grantTypes.clientCredentials': '客户端凭证',
  'application.form.grantTypes.authorizationCode': '授权码模式',
  'application.form.grantTypes.refreshToken': '刷新令牌',
  'application.form.scopes': '作用域',
  'application.form.redirectUris': '重定向URI',
  'application.form.redirectUris.placeholder': '每行一个URI，例如：\nhttp://localhost:5173/callback',
  'application.form.postLogoutRedirectUris': '登出重定向URI',
  'application.form.postLogoutRedirectUris.placeholder': '每行一个URI，例如：\nhttp://localhost:5173/login',

  // Validation
  'application.form.clientId.required': '请输入客户端ID',
  'application.form.displayName.required': '请输入显示名称',
  'application.form.type.required': '请选择客户端类型',

  // Detail labels
  'application.detail.clientId': '客户端ID',
  'application.detail.displayName': '显示名称',
  'application.detail.clientType': '客户端类型',
  'application.detail.consentType': '同意类型',
  'application.detail.permissions': '权限列表',
  'application.detail.redirectUris': '重定向URI',
  'application.detail.postLogoutRedirectUris': '登出URI',
  'application.detail.createdAt': '创建时间',

  // Messages
  'application.message.createSuccess': '应用程序创建成功',
  'application.message.updateSuccess': '应用程序更新成功',
  'application.message.deleteSuccess': '应用程序删除成功',
  'application.message.loadFailed': '加载应用程序失败',

  // Permission prefixes
  'application.permission.grantType': '授权',
  'application.permission.endpoint': '端点',
  'application.permission.scope': '作用域',
};
