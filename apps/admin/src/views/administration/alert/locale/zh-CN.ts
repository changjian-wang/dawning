export default {
  'menu.administration.alertManagement': '告警管理',

  // Stat cards
  'alert.stats.alertRules': '告警规则',
  'alert.stats.enabled': '启用',
  'alert.stats.todayAlerts': '今日告警',
  'alert.stats.unresolvedAlerts': '未解决告警',
  'alert.stats.criticalAlerts': '严重告警',

  // Management card
  'alert.management': '告警管理',
  'alert.tab.rules': '告警规则',
  'alert.tab.history': '告警历史',

  // Buttons
  'alert.addRule': '添加规则',
  'alert.manualCheck': '手动检查',

  // Table columns - Rules
  'alert.column.ruleName': '规则名称',
  'alert.column.metricType': '指标类型',
  'alert.column.condition': '条件',
  'alert.column.duration': '持续时间',
  'alert.column.severity': '严重程度',
  'alert.column.enabled': '启用',
  'alert.column.lastTriggered': '上次触发',
  'alert.column.operations': '操作',

  // Table columns - History
  'alert.column.status': '状态',
  'alert.column.metricValue': '指标值',
  'alert.column.message': '消息',
  'alert.column.triggeredAt': '触发时间',
  'alert.column.threshold': '阈值',

  // Filters
  'alert.filter.severity': '严重程度',
  'alert.filter.status': '状态',

  // Actions
  'alert.action.acknowledge': '确认',
  'alert.action.resolve': '解决',

  // Modal
  'alert.modal.addRule': '添加告警规则',
  'alert.modal.editRule': '编辑告警规则',

  // Form labels
  'alert.form.ruleName': '规则名称',
  'alert.form.description': '描述',
  'alert.form.metricType': '指标类型',
  'alert.form.severity': '严重程度',
  'alert.form.operator': '比较操作符',
  'alert.form.threshold': '阈值',
  'alert.form.duration': '持续时间(秒)',
  'alert.form.cooldown': '冷却时间(分钟)',
  'alert.form.enabled': '启用状态',
  'alert.form.notifySettings': '通知设置',
  'alert.form.notifyChannels': '通知渠道',
  'alert.form.notifyEmails': '通知邮箱',
  'alert.form.webhookUrl': 'Webhook URL',

  // Placeholders
  'alert.placeholder.ruleName': '请输入规则名称',
  'alert.placeholder.description': '请输入规则描述',
  'alert.placeholder.metricType': '请选择指标',
  'alert.placeholder.severity': '请选择严重程度',
  'alert.placeholder.operator': '请选择操作符',
  'alert.placeholder.emails': '多个邮箱用逗号分隔',
  'alert.placeholder.webhookUrl': '请输入 Webhook URL',

  // Validation
  'alert.validation.ruleName': '请输入规则名称',
  'alert.validation.metricType': '请选择指标类型',
  'alert.validation.operator': '请选择操作符',
  'alert.validation.threshold': '请输入阈值',
  'alert.validation.severity': '请选择严重程度',

  // Metric types
  'alert.metricType.cpu': 'CPU 使用率',
  'alert.metricType.memory': '内存使用率',
  'alert.metricType.responseTime': '响应时间',
  'alert.metricType.errorRate': '错误率',
  'alert.metricType.requestCount': '请求数量',

  // Operators
  'alert.operator.gt': '大于 (>)',
  'alert.operator.gte': '大于等于 (>=)',
  'alert.operator.lt': '小于 (<)',
  'alert.operator.lte': '小于等于 (<=)',
  'alert.operator.eq': '等于 (=)',

  // Severity
  'alert.severity.info': '信息',
  'alert.severity.warning': '警告',
  'alert.severity.error': '错误',
  'alert.severity.critical': '严重',

  // Status
  'alert.status.triggered': '已触发',
  'alert.status.acknowledged': '已确认',
  'alert.status.resolved': '已解决',

  // Notify channels
  'alert.notify.email': '邮件',
  'alert.notify.webhook': 'Webhook',

  // Duration suffix
  'alert.duration.seconds': '秒',

  // Messages
  'alert.message.checkComplete': '告警检查完成',
  'alert.message.ruleCreated': '规则创建成功',
  'alert.message.ruleUpdated': '规则更新成功',
  'alert.message.ruleDeleted': '规则删除成功',
  'alert.message.acknowledged': '告警已确认',
  'alert.message.resolved': '告警已解决',
};
