/// <reference types="@angular/localize" />

import { enableProdMode, isDevMode, importProvidersFrom, ENVIRONMENT_INITIALIZER, inject } from '@angular/core';
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
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { LightboxModule } from 'ngx-lightbox';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppRoutingModule } from './app/app-routing.module';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { HIGHLIGHT_OPTIONS, HighlightModule } from 'ngx-highlightjs';
import { CookieInterceptor, ErrorInterceptor } from './app/_helpers';
import { HTTP_INTERCEPTORS, withInterceptorsFromDi, provideHttpClient } from '@angular/common/http';
import { DatePipe } from '@angular/common';

if (environment.production) {
  enableProdMode();
}

// Configure Material Symbols
function configureIcons() {
  return () => {
    const iconRegistry = inject(MatIconRegistry);
    iconRegistry.setDefaultFontSetClass('material-symbols-outlined');
  };
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
    { provide: HTTP_INTERCEPTORS, useClass: CookieInterceptor, multi: true },
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
    provideNativeDateAdapter(),
    // Configure Material Symbols
    {
      provide: ENVIRONMENT_INITIALIZER,
      useFactory: configureIcons,
      multi: true,
    },
  ],
})
  .catch(err => console.error(err));
