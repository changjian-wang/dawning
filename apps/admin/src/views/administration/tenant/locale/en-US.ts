export default {
  'tenant.title': 'Tenant Management',
  'tenant.list.title': 'Tenant List',
  'tenant.search.placeholder': 'Search tenant name or code',
  'tenant.filter.status': 'Status',
  'tenant.filter.status.all': 'All',
  'tenant.filter.status.active': 'Active',
  'tenant.filter.status.inactive': 'Inactive',
  'tenant.button.create': 'Create Tenant',
  'tenant.button.edit': 'Edit',
  'tenant.button.delete': 'Delete',
  'tenant.button.enable': 'Enable',
  'tenant.button.disable': 'Disable',
  'tenant.button.switch': 'Switch',

  // Table columns
  'tenant.column.code': 'Tenant Code',
  'tenant.column.name': 'Tenant Name',
  'tenant.column.domain': 'Domain',
  'tenant.column.email': 'Contact Email',
  'tenant.column.plan': 'Plan',
  'tenant.column.maxUsers': 'Max Users',
  'tenant.column.isActive': 'Status',
  'tenant.column.createdAt': 'Created At',
  'tenant.column.operations': 'Actions',

  // Form
  'tenant.form.code': 'Tenant Code',
  'tenant.form.code.placeholder': 'Enter tenant code (unique identifier)',
  'tenant.form.code.help': 'Tenant code will be used in URL and request headers',
  'tenant.form.name': 'Tenant Name',
  'tenant.form.name.placeholder': 'Enter tenant name',
  'tenant.form.description': 'Description',
  'tenant.form.description.placeholder': 'Enter tenant description',
  'tenant.form.domain': 'Domain',
  'tenant.form.domain.placeholder': 'Enter bound domain (optional)',
  'tenant.form.domain.help': 'Auto-detect tenant via subdomain',
  'tenant.form.email': 'Contact Email',
  'tenant.form.email.placeholder': 'Enter contact email',
  'tenant.form.phone': 'Contact Phone',
  'tenant.form.phone.placeholder': 'Enter contact phone',
  'tenant.form.logoUrl': 'Logo URL',
  'tenant.form.logoUrl.placeholder': 'Enter logo image URL',
  'tenant.form.plan': 'Plan',
  'tenant.form.plan.placeholder': 'Select subscription plan',
  'tenant.form.maxUsers': 'Max Users',
  'tenant.form.maxUsers.placeholder': 'Enter max users limit',
  'tenant.form.maxStorageMB': 'Max Storage (MB)',
  'tenant.form.maxStorageMB.placeholder': 'Enter max storage limit',
  'tenant.form.subscriptionExpiresAt': 'Subscription Expires',
  'tenant.form.isActive': 'Active Status',

  // Plan options
  'tenant.plan.free': 'Free',
  'tenant.plan.basic': 'Basic',
  'tenant.plan.pro': 'Professional',
  'tenant.plan.enterprise': 'Enterprise',

  // Modal titles
  'tenant.modal.create': 'Create Tenant',
  'tenant.modal.edit': 'Edit Tenant',
  'tenant.modal.view': 'View Details',
  'tenant.modal.delete.title': 'Confirm Delete',
  'tenant.modal.delete.content':
    'Are you sure you want to delete tenant "{name}"? All data for this tenant will be removed. This action cannot be undone.',

  // Messages
  'tenant.message.createSuccess': 'Tenant created successfully',
  'tenant.message.updateSuccess': 'Tenant updated successfully',
  'tenant.message.deleteSuccess': 'Tenant deleted successfully',
  'tenant.message.enableSuccess': 'Tenant enabled',
  'tenant.message.disableSuccess': 'Tenant disabled',
  'tenant.message.codeExists': 'Tenant code already exists',
  'tenant.message.domainExists': 'Domain is already in use',
  'tenant.message.switchSuccess': 'Switched to tenant: {name}',

  // Current tenant
  'tenant.current': 'Current Tenant',
  'tenant.current.host': 'Host Mode (Super Admin)',
  'tenant.current.none': 'No tenant selected',

  // Actions
  'tenant.action.view': 'View',
  'tenant.action.edit': 'Edit',
  'tenant.action.delete': 'Delete',
  'tenant.action.enable': 'Enable',
  'tenant.action.disable': 'Disable',

  // Status display
  'tenant.status.active': 'Active',
  'tenant.status.inactive': 'Inactive',
  'tenant.status.unlimited': 'Unlimited',

  // Validation messages
  'tenant.validation.codeRequired': 'Please enter tenant code',
  'tenant.validation.codePattern': 'Only lowercase letters, numbers, underscores and hyphens allowed',
  'tenant.validation.codeLength': 'Length must be 2-50 characters',
  'tenant.validation.nameRequired': 'Please enter tenant name',
  'tenant.validation.nameMaxLength': 'Maximum 100 characters',
  'tenant.validation.emailInvalid': 'Please enter a valid email address',

  // Error messages
  'tenant.message.loadFailed': 'Failed to load tenant list',
  'tenant.message.operationFailed': 'Operation failed',
  'tenant.message.deleteFailed': 'Delete failed',
};
