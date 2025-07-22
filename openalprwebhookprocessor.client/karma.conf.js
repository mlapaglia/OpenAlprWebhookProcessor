// Karma configuration file, see link for more information
// https://karma-runner.github.io/1.0/config/configuration-file.html

const puppeteer = require('puppeteer');

module.exports = function (config) {
  // Get Puppeteer's bundled Chromium path
  const chromiumPath = puppeteer.executablePath();
  
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      
    ],
    client: {
      jasmine: {
        random: false
      },
      clearContext: false // leave Jasmine Spec Runner output visible in browser
    },
    jasmineHtmlReporter: {
      suppressAll: true // removes the duplicated traces
    },
    coverageReporter: {
      dir: require('path').join(__dirname, './coverage'),
      subdir: '.',
      reporters: [
        { type: 'lcov' }
      ]
    },
    preprocessors: {
      'src/**/*.ts': ['coverage']
    },
    reporters: ['progress', 'coverage', 'kjhtml'],
    customLaunchers: {
      ChromePuppeteer: {
        base: 'Chrome',
        flags: [
          '--no-sandbox',
          '--disable-web-security'
        ]
      },
      ChromeHeadlessPuppeteer: {
        base: 'ChromeHeadless',
        flags: [
          '--no-sandbox',
          '--disable-web-security',
          '--disable-features=VizDisplayCompositor'
        ]
      }
    },
    browsers: ['Chrome'],
    restartOnFileChange: true
  });
  
  // Set the Chrome binary path to Puppeteer's Chromium
  process.env.CHROME_BIN = chromiumPath;
};
