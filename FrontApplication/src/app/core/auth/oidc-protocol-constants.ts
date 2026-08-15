export const OidcLocalProxy = {
  segment: 'identity',
} as const;

export const OidcProtocolPaths = {
  authorization: 'connect/authorize',
  endSession: 'connect/logout',
  jwks: '.well-known/jwks',
  revocation: 'connect/revoke',
  token: 'connect/token',
  userInfo: 'connect/userinfo',
} as const;

export const OidcResponseTypes = {
  code: 'code',
} as const;
