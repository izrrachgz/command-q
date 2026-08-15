export const TextKeys = {
  action: {
    create: 'action.create',
    login: 'action.login',
    save: 'action.save',
  },
  admin: {
    accounts: {
      active: 'admin.accounts.active',
      backendPending: 'admin.accounts.backendPending',
      email: 'admin.accounts.email',
      locked: 'admin.accounts.locked',
      title: 'admin.accounts.title',
      username: 'admin.accounts.username',
    },
    dashboard: {
      title: 'admin.dashboard.title',
    },
    email: {
      attempts: 'table.attempts',
      pendingBackend: 'admin.email.pendingBackend',
      recipient: 'admin.email.recipient',
      sheetName: 'admin.email.sheetName',
      status: 'admin.email.status',
      subject: 'admin.email.subject',
      title: 'admin.email.title',
    },
    preferences: {
      title: 'preferences.title',
    },
  },
  app: {
    brand: 'app.brand',
    documentTitle: 'app.documentTitle',
  },
  auth: {
    adminLogin: 'auth.adminLogin',
    callback: 'auth.callback',
    identityUnavailable: 'auth.identityUnavailable',
    register: 'auth.register',
    registrationFailed: 'auth.registrationFailed',
    signInFailed: 'auth.signInFailed',
    tenantLogin: 'auth.tenantLogin',
  },
  provision: {
    title: 'provision.title',
  },
  landing: {
    title: 'landing.title',
  },
  nav: {
    accounts: 'nav.accounts',
    admin: 'nav.admin',
    dashboard: 'nav.dashboard',
    email: 'nav.email',
    preferences: 'nav.preferences',
    profile: 'nav.profile',
    tenant: 'nav.tenant',
  },
  report: {
    accountsSheetName: 'report.accounts.sheetName',
  },
  tenant: {
    dashboard: {
      title: 'tenant.dashboard.title',
    },
    preferences: {
      title: 'preferences.title',
    },
    profile: {
      title: 'tenant.profile.title',
    },
  },
  validation: {
    email: 'validation.email',
    max: 'validation.max',
    maxlength: 'validation.maxlength',
    min: 'validation.min',
    minlength: 'validation.minlength',
    passwordMismatch: 'validation.passwordMismatch',
    pattern: 'validation.pattern',
    required: 'validation.required',
  },
} as const;
