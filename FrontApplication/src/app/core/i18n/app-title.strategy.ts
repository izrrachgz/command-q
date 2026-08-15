import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRouteSnapshot, RouterStateSnapshot, TitleStrategy } from '@angular/router';

import { AppRouteDataKeys } from '../routing/app-route-constants';
import { I18nService } from './i18n.service';
import { TextKeys } from './text-keys';

@Injectable()
export class AppTitleStrategy extends TitleStrategy {
  private currentTitleKey = '';

  constructor(
    private readonly title: Title,
    private readonly i18n: I18nService,
  ) {
    super();
    this.i18n.culture$.subscribe(() => this.setDocumentTitle(this.currentTitleKey));
  }

  override updateTitle(snapshot: RouterStateSnapshot): void {
    this.currentTitleKey = findDeepestTitleKey(snapshot.root) ?? '';
    this.setDocumentTitle(this.currentTitleKey);
  }

  private setDocumentTitle(titleKey: string): void {
    if (!titleKey) {
      return;
    }

    this.title.setTitle(this.i18n.text(TextKeys.app.documentTitle, {
      app: this.i18n.text(TextKeys.app.brand),
      title: this.i18n.text(titleKey),
    }));
  }
}

const findDeepestTitleKey = (route: ActivatedRouteSnapshot): string | undefined => {
  let current: ActivatedRouteSnapshot | null = route;
  let titleKey: string | undefined;

  while (current) {
    titleKey = (current.data[AppRouteDataKeys.titleKey] as string | undefined) ?? titleKey;
    current = current.firstChild;
  }

  return titleKey;
};
