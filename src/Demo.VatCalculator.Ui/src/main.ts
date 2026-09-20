import { registerLocaleData } from '@angular/common';
import localeDeAt from '@angular/common/locales/de-AT';
import localeDeAtExtra from '@angular/common/locales/extra/de-AT';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

registerLocaleData(localeDeAt, 'de-AT', localeDeAtExtra);

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
