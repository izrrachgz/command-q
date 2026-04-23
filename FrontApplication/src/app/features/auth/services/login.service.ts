import { Injectable } from '@angular/core';
import { AuthService } from '@core/auth';

@Injectable({
  providedIn: 'root',
})
export class LoginService {
  constructor(private authService: AuthService) {}

  login(username: string, password: string) {
    return this.authService.login({
      username,
      password,
    });
  }
}
