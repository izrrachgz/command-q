import { registerLocaleData } from '@angular/common';
import localeEn from '@angular/common/locales/en';
import localeEs from '@angular/common/locales/es';
import { bootstrapApplication } from '@angular/platform-browser';

import { appConfig } from './app/app.config';
import { App } from './app/app';

registerLocaleData(localeEn, 'en-US');
registerLocaleData(localeEs, 'es-ES');

bootstrapApplication(App, appConfig).catch((err) => console.error(err));
