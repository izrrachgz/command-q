export const TextKeys = {
  accessibility: {
    closeNavigation: 'accessibility.closeNavigation',
    collapseNavigation: 'accessibility.collapseNavigation',
    expandNavigation: 'accessibility.expandNavigation',
    openNavigation: 'accessibility.openNavigation',
    skipToContent: 'accessibility.skipToContent',
  },
  action: {
    cancel: 'action.cancel',
    confirm: 'action.confirm',
    create: 'action.create',
    login: 'action.login',
    logout: 'action.logout',
    no: 'action.no',
    save: 'action.save',
    yes: 'action.yes',
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
    roles: {
      available: 'admin.roles.available',
      backendPending: 'admin.roles.backendPending',
      empty: 'admin.roles.empty',
      selectAccount: 'admin.roles.selectAccount',
      title: 'admin.roles.title',
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
  confirmation: {
    acceptLabel: 'confirmation.acceptLabel',
    signOutMessage: 'confirmation.signOutMessage',
    signOutTitle: 'confirmation.signOutTitle',
    textLabel: 'confirmation.textLabel',
    title: 'confirmation.title',
    typeExpectedText: 'confirmation.typeExpectedText',
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
    roles: 'nav.roles',
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
    dateTimeRangeIncomplete: 'validation.dateTimeRangeIncomplete',
    dateTimeRangeOrder: 'validation.dateTimeRangeOrder',
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
