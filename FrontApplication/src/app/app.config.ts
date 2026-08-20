import {
  ApplicationConfig,
  ErrorHandler,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import {
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withInterceptors,
  withInterceptorsFromDi,
} from '@angular/common/http';
import {
  provideRouter,
  TitleStrategy,
  withComponentInputBinding,
  withInMemoryScrolling,
} from '@angular/router';
import {
  AbstractLoggerService,
  AuthInterceptor,
  LogLevel,
  provideAuth,
} from 'angular-auth-oidc-client';
import {
  LucideActivity,
  LucideBuilding2,
  LucideCalendar,
  LucideCheck,
  LucideChevronDown,
  LucideCircleAlert,
  LucideClock,
  LucideEye,
  LucideEyeOff,
  LucideFileSpreadsheet,
  LucideHouse,
  LucideInbox,
  LucideLanguages,
  LucideLayoutDashboard,
  LucideLock,
  LucideLockKeyhole,
  LucideLogOut,
  LucideMail,
  LucideMenu,
  LucidePanelLeftClose,
  LucidePanelLeftOpen,
  LucidePencil,
  LucidePlus,
  LucideRefreshCw,
  LucideSave,
  LucideSearch,
  LucideSend,
  LucideSettings,
  LucideShield,
  LucideShieldCheck,
  LucideUnlockKeyhole,
  LucideUser,
  LucideUserPlus,
  LucideUserRoundSearch,
  LucideUsersRound,
  LucideX,
  provideLucideConfig,
  provideLucideIcons,
} from '@lucide/angular';

import { APP_ROUTES } from './app.routes';
import { apiContextInterceptor } from './core/api/api-context.interceptor';
import { AuthFacade } from './core/auth/auth.facade';
import { OidcConsoleLoggerService } from './core/auth/oidc-console-logger.service';
import {
  OidcLocalProxy,
  OidcProtocolPaths,
  OidcResponseTypes,
} from './core/auth/oidc-protocol-constants';
import { combineUrl } from './core/config/url-builder';
import { AppTitleStrategy } from './core/i18n/app-title.strategy';
import { FrontendErrorHandler } from './core/logging/frontend-error.handler';
import { FrontendLoggerService } from './core/logging/frontend-logger.service';
import { PreferencesService } from './core/preferences/preferences.service';
import { AppRoutePaths } from './core/routing/app-route-paths';
import { environment } from '../environments/environment';

const oidcLogLevel = environment.production ? LogLevel.None : LogLevel.Debug;
const identityAuthority = environment.identity.authority;
const useLocalIdentityProxy =
  !environment.production && identityAuthority.startsWith('https://localhost:');
const oidcProtocolBase = useLocalIdentityProxy
  ? `${trimTrailingSlash(environment.applicationUrl)}/${OidcLocalProxy.segment}`
  : trimTrailingSlash(identityAuthority);

function trimTrailingSlash(value: string): string {
  return value.replace(/\/+$/, '');
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(
      APP_ROUTES,
      withComponentInputBinding(),
      withInMemoryScrolling({ scrollPositionRestoration: 'top' }),
    ),
    provideHttpClient(withInterceptors([apiContextInterceptor]), withInterceptorsFromDi()),
    provideAuth({
      config: {
        authority: identityAuthority,
        authWellknownEndpointUrl: oidcProtocolBase,
        authWellknownEndpoints: {
          issuer: identityAuthority,
          jwksUri: combineUrl(oidcProtocolBase, OidcProtocolPaths.jwks),
          authorizationEndpoint: combineUrl(identityAuthority, OidcProtocolPaths.authorization),
          tokenEndpoint: combineUrl(oidcProtocolBase, OidcProtocolPaths.token),
          userInfoEndpoint: combineUrl(oidcProtocolBase, OidcProtocolPaths.userInfo),
          endSessionEndpoint: combineUrl(identityAuthority, OidcProtocolPaths.endSession),
          revocationEndpoint: combineUrl(oidcProtocolBase, OidcProtocolPaths.revocation),
        },
        redirectUrl: environment.identity.redirectUrl,
        postLogoutRedirectUri: environment.identity.postLogoutRedirectUri,
        clientId: environment.identity.clientId,
        scope: environment.identity.scope,
        responseType: OidcResponseTypes.code,
        silentRenew: true,
        useRefreshToken: true,
        renewTimeBeforeTokenExpiresInSeconds: 30,
        triggerAuthorizationResultEvent: true,
        postLoginRoute: AppRoutePaths.auth.callback,
        unauthorizedRoute: AppRoutePaths.auth.login,
        strictIssuerValidationOnWellKnownRetrievalOff: useLocalIdentityProxy,
        secureRoutes: [environment.api.baseUrl],
        logLevel: oidcLogLevel,
      },
    }),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
    {
      provide: AbstractLoggerService,
      useClass: OidcConsoleLoggerService,
    },
    {
      provide: ErrorHandler,
      useClass: FrontendErrorHandler,
    },
    {
      provide: TitleStrategy,
      useClass: AppTitleStrategy,
    },
    provideLucideConfig({ strokeWidth: 1.8 }),
    provideLucideIcons(
      LucideActivity,
      LucideBuilding2,
      LucideCalendar,
      LucideCheck,
      LucideChevronDown,
      LucideCircleAlert,
      LucideClock,
      LucideEye,
      LucideEyeOff,
      LucideFileSpreadsheet,
      LucideHouse,
      LucideInbox,
      LucideLanguages,
      LucideLayoutDashboard,
      LucideLock,
      LucideLockKeyhole,
      LucideLogOut,
      LucideMail,
      LucideMenu,
      LucidePanelLeftClose,
      LucidePanelLeftOpen,
      LucidePencil,
      LucidePlus,
      LucideRefreshCw,
      LucideSave,
      LucideSearch,
      LucideSend,
      LucideSettings,
      LucideShield,
      LucideShieldCheck,
      LucideUnlockKeyhole,
      LucideUser,
      LucideUserPlus,
      LucideUserRoundSearch,
      LucideUsersRound,
      LucideX,
    ),
    provideAppInitializer(() => inject(PreferencesService).initialize()),
    provideAppInitializer(() => inject(FrontendLoggerService).initialize()),
    provideAppInitializer(() => inject(AuthFacade).initialize()),
  ],
};
