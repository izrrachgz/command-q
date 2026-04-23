import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';

import { LoginRequest, LoginResponse, JwtPayload, RegisterRequest } from './auth.model';
import { environment } from '@env/environment.staging';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly API_URL = environment.apiUrl;

  private loggedInSubject = new BehaviorSubject<boolean>(this.hasValidToken());
  loggedIn$ = this.loggedInSubject.asObservable();

  private accessToken: string | null = null;

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {}

  login(data: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.API_URL}account/sign-in`, data, {
        withCredentials: true, // for cookies
      })
      .pipe(
        tap((res) => {
          this.setAccessToken(res.accessToken);
          this.loggedInSubject.next(true);
        }),
      );
  }

  refresh(): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${this.API_URL}account/refresh`,
      {},
      {
        withCredentials: true,
      },
    );
  }

  register(data: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.API_URL}account/create`, data).pipe(
      tap((res) => {
        this.setAccessToken(res.accessToken);
        this.loggedInSubject.next(true);
      }),
    );
  }

  logout(): void {
    this.setAccessToken(null);
    this.loggedInSubject.next(false);

    this.router.navigate(['/auth/login']);
  }

  setAccessToken(token: string | null) {
    this.accessToken = token;
  }

  getAccessToken(): string | null {
    return this.accessToken;
  }

  isAuthenticated(): boolean {
    return this.hasValidToken();
  }

  private hasValidToken(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;

    try {
      const payload = this.decodeToken(token);

      return payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  private decodeToken(token: string): JwtPayload {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload));
  }
}
