// This file is required by karma.conf.js and loads recursively all the .spec and framework files

import '@angular/localize/init';
import 'zone.js/testing';
import { getTestBed } from '@angular/core/testing';
import {
  BrowserTestingModule,
  platformBrowserTesting,
} from '@angular/platform-browser/testing';

declare const require: {
  context(path: string, deep?: boolean, filter?: RegExp): {
    keys(): string[]
    <T>(id: string): T
  }
};

getTestBed().initTestEnvironment(
  BrowserTestingModule,
  platformBrowserTesting(),
);

const context = require.context('./', true, /\.spec\.ts$/);
context.keys().map(context);
