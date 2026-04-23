import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler } from '@angular/common/http';

import { BehaviorSubject, catchError, throwError, switchMap, filter, take } from 'rxjs';

import { AuthService } from './auth.service';

@Injectable()
export class JwtRefreshInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshSubject = new BehaviorSubject<string | null>(null);

  constructor(private auth: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    return next.handle(req).pipe(
      catchError((error) => {
        if (error.status !== 401) {
          return throwError(() => error);
        }

        // prevent infinite loop
        if (req.url.includes('/account/refresh')) {
          return throwError(() => error);
        }

        if (!this.isRefreshing) {
          this.isRefreshing = true;
          this.refreshSubject.next(null);

          return this.auth.refresh().pipe(
            switchMap((res: any) => {
              this.isRefreshing = false;

              const newToken = res.accessToken;
              this.auth.setAccessToken(newToken);
              this.refreshSubject.next(newToken);

              return next.handle(this.addToken(req, newToken));
            }),
            catchError((err) => {
              this.isRefreshing = false;
              // logout here
              return throwError(() => err);
            }),
          );
        }

        // wait for ongoing refresh
        return this.refreshSubject.pipe(
          filter((token) => token !== null),
          take(1),
          switchMap((token) => next.handle(this.addToken(req, token!))),
        );
      }),
    );
  }

  private addToken(req: HttpRequest<any>, token: string) {
    return req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }
}
