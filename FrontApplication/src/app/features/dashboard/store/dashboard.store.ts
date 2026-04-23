import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { DashboardSnapshot } from './dashboard.snapshot';

@Injectable({
  providedIn: 'root',
})
export class DashboardStore {
  private state = new BehaviorSubject<DashboardSnapshot>({
    open: false,
  });

  state$ = this.state.asObservable();

  get snapshot(): DashboardSnapshot {
    return this.state.value;
  }

  setAuthenticated(userId: string, username: string) {
    this.state.next({
      open: true,
    });
  }

  clear() {
    this.state.next({
      open: false,
    });
  }
}
