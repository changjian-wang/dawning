export default {
  'menu.administration.auditLog': '审计日志',

  'auditLog.title': '审计日志',
  'auditLog.list.title': '审计日志列表',
  'auditLog.detail.title': '日志详情',

  // 搜索表单
  'auditLog.form.username': '操作用户',
  'auditLog.form.username.placeholder': '请输入用户名',
  'auditLog.form.action': '操作类型',
  'auditLog.form.action.placeholder': '请选择操作类型',
  'auditLog.form.entityType': '实体类型',
  'auditLog.form.entityType.placeholder': '请选择实体类型',
  'auditLog.form.ipAddress': 'IP地址',
  'auditLog.form.ipAddress.placeholder': '请输入IP地址',
  'auditLog.form.dateRange': '日期范围',
  'auditLog.form.dateRange.placeholder': ['开始日期', '结束日期'],
  'auditLog.form.search': '查询',
  'auditLog.form.reset': '重置',
  'auditLog.form.export': '导出',

  // 操作类型
  'auditLog.action.Create': '创建',
  'auditLog.action.Update': '更新',
  'auditLog.action.Delete': '删除',
  'auditLog.action.ChangePassword': '修改密码',
  'auditLog.action.ResetPassword': '重置密码',
  'auditLog.action.Login': '登录',
  'auditLog.action.Logout': '登出',
  'auditLog.action.AssignPermissions': '分配权限',
  'auditLog.action.RemovePermissions': '移除权限',

  // 实体类型
  'auditLog.entityType.User': '用户',
  'auditLog.entityType.Role': '角色',
  'auditLog.entityType.Permission': '权限',
  'auditLog.entityType.Application': '应用',
  'auditLog.entityType.Scope': '作用域',
  'auditLog.entityType.ClaimType': '声明类型',
  'auditLog.entityType.SystemConfig': '系统配置',

  // 表格列
  'auditLog.columns.username': '操作用户',
  'auditLog.columns.action': '操作',
  'auditLog.columns.entityType': '实体类型',
  'auditLog.columns.entityId': '实体ID',
  'auditLog.columns.description': '描述',
  'auditLog.columns.ipAddress': 'IP地址',
  'auditLog.columns.statusCode': '状态码',
  'auditLog.columns.createdAt': '操作时间',
  'auditLog.columns.operations': '操作',

  // 操作按钮
  'auditLog.button.view': '查看详情',
  'auditLog.button.cleanup': '清理旧日志',

  // 详情对话框
  'auditLog.detail.basicInfo': '基本信息',
  'auditLog.detail.requestInfo': '请求信息',
  'auditLog.detail.changes': '变更内容',
  'auditLog.detail.userId': '用户ID',
  'auditLog.detail.username': '用户名',
  'auditLog.detail.action': '操作类型',
  'auditLog.detail.entityType': '实体类型',
  'auditLog.detail.entityId': '实体ID',
  'auditLog.detail.description': '描述',
  'auditLog.detail.ipAddress': 'IP地址',
  'auditLog.detail.userAgent': 'User-Agent',
  'auditLog.detail.requestPath': '请求路径',
  'auditLog.detail.requestMethod': '请求方法',
  'auditLog.detail.statusCode': '状态码',
  'auditLog.detail.createdAt': '操作时间',
  'auditLog.detail.oldValues': '变更前',
  'auditLog.detail.newValues': '变更后',
  'auditLog.detail.noChanges': '无变更记录',

  // 清理对话框
  'auditLog.cleanup.title': '清理旧日志',
  'auditLog.cleanup.message': '确定要清理多少天之前的审计日志？',
  'auditLog.cleanup.daysToKeep': '保留天数',
  'auditLog.cleanup.daysToKeep.placeholder': '请输入要保留的天数',
  'auditLog.cleanup.confirm': '确定清理',
  'auditLog.cleanup.cancel': '取消',
  'auditLog.cleanup.success': '清理成功',
  'auditLog.cleanup.error': '清理失败',

  // 提示信息
  'auditLog.message.noData': '暂无审计日志',
  'auditLog.message.error': '加载失败，请稍后重试',
};
