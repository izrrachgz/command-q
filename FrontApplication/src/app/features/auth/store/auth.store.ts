import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { AuthSnapshot } from './auth.snapshot';

@Injectable({
  providedIn: 'root',
})
export class AuthStore {
  private state = new BehaviorSubject<AuthSnapshot>({
    userId: null,
    username: null,
    authenticated: false,
  });

  state$ = this.state.asObservable();

  get snapshot(): AuthSnapshot {
    return this.state.value;
  }

  setAuthenticated(userId: string, username: string) {
    this.state.next({
      userId,
      username,
      authenticated: true,
    });
  }

  clear() {
    this.state.next({
      userId: null,
      username: null,
      authenticated: false,
    });
  }
}
