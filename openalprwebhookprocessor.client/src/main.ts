/// <reference types="@angular/localize" />

import { enableProdMode, isDevMode, importProvidersFrom, inject, provideAppInitializer } from '@angular/core';
import { environment } from './environments/environment';
import { AppComponent } from './app/app.component';
import { ServiceWorkerModule } from '@angular/service-worker';
import { Chart, registerables } from 'chart.js';
import { MatExpansionModule } from '@angular/material/expansion';

Chart.register(...registerables);
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDividerModule } from '@angular/material/divider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { LightboxModule } from 'ngx-lightbox';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { provideAnimations } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppRoutingModule } from './app/app-routing.module';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { HIGHLIGHT_OPTIONS, HighlightModule } from 'ngx-highlightjs';
import { JwtInterceptor, ErrorInterceptor } from './app/_helpers';
import { HTTP_INTERCEPTORS, withInterceptorsFromDi, provideHttpClient } from '@angular/common/http';
import { AccountService } from './app/_services';
import { appInitializer } from './app/_helpers/app.initializer';
import { DatePipe } from '@angular/common';

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    importProvidersFrom(
      BrowserModule,
      AppRoutingModule,
      FormsModule,
      ReactiveFormsModule,
      MatAutocompleteModule,
      LightboxModule,
      MatButtonModule,
      MatTabsModule,
      MatIconModule,
      MatCardModule,
      MatDatepickerModule,
      MatInputModule,
      MatFormFieldModule,
      MatCheckboxModule,
      MatDividerModule,
      MatSlideToggleModule,
      MatSnackBarModule,
      MatProgressSpinnerModule,
      MatExpansionModule,
      HighlightModule,
      ServiceWorkerModule.register('ngsw-worker.js', {
        enabled: !isDevMode(),
        registrationStrategy: 'registerWhenStable:30',
      }),
    ),
    DatePipe,
    provideAppInitializer(() => {
      const initializerFn = appInitializer(inject(AccountService));
      return initializerFn();
    }),
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    {
      provide: HIGHLIGHT_OPTIONS,
      useValue: {
        coreLibraryLoader: () => import('highlight.js/lib/core'),
        languages: {
          plaintext: () => import('highlight.js/lib/languages/plaintext'),
          shell: () => import('highlight.js/lib/languages/shell'),
          accesslog: () => import('highlight.js/lib/languages/accesslog'),
          dotnetlogs: () => import('./assets/highlight/dotnet-logs.language'),
        },
      },
    },
    provideHttpClient(withInterceptorsFromDi()),
    provideAnimations(),
  ],
})
  .catch(err => console.error(err));
