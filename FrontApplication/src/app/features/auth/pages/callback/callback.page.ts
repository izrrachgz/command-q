import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';

import { AuthFacade } from '../../../../core/auth/auth.facade';
import { TextPipe } from '../../../../core/i18n/text.pipe';
import { AppRoutePaths } from '../../../../core/routing/app-route-paths';

@Component({
  selector: 'app-callback-page',
  standalone: true,
  imports: [LucideDynamicIcon, TextPipe],
  templateUrl: './callback.page.html',
})
export class CallbackPage implements OnInit {
  constructor(
    private readonly auth: AuthFacade,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.auth.finishCallback().subscribe({
      next: (session) => {
        const target = session.authenticated ? this.auth.resolveReturnUrl() : AppRoutePaths.auth.login;
        void this.router.navigateByUrl(target);
      },
      error: () => this.router.navigate([AppRoutePaths.auth.login]),
    });
  }
}
