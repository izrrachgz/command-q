import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';

import { Observable } from 'rxjs';
import { environment } from '@env/environment.prod';

@Injectable()
export class HttpApiInterceptor implements HttpInterceptor {
  private readonly API_CLIENT_ID_NAME = environment.client.name;
  private readonly API_CLIENT_ID = environment.client.id;

  constructor() {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const commonRequest = req.clone({
      setHeaders: {
        [this.API_CLIENT_ID_NAME]: this.API_CLIENT_ID,
      },
    });

    return next.handle(commonRequest);
  }
}
