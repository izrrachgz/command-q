import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  importProvidersFrom,
  provideAppInitializer,
  inject,
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { APP_ROUTES } from './app.routes';

import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpApiInterceptor } from '@core/http/http.interceptor';
import { JwtInterceptor } from '@core/auth/jwt.interceptor';

import { CoreModule } from './core/core.module';
import { JwtRefreshInterceptor } from '@core/auth/jwt.refresh.interceptor';
import { AuthService } from '@core/auth/auth.service';
import { catchError, firstValueFrom, of, tap } from 'rxjs';

export const appConfig: ApplicationConfig = {
  providers: [
    importProvidersFrom(CoreModule),
    provideBrowserGlobalErrorListeners(),
    provideRouter(APP_ROUTES),
    provideHttpClient(withInterceptorsFromDi()),
    // Interceptors order matters, the first registered will run last
    provideAppInitializer(() => {
      const auth = inject(AuthService);

      return firstValueFrom(
        auth.refresh().pipe(
          tap((res) => auth.setAccessToken(res.accessToken)),
          catchError(() => {
            auth.setAccessToken(null);
            return of(null);
          }),
        ),
      );
    }),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtRefreshInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpApiInterceptor,
      multi: true,
    },
  ],
};
