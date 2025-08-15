// Karma configuration file, see link for more information
// https://karma-runner.github.io/1.0/config/configuration-file.html
const puppeteer = require('puppeteer');

module.exports = function (config) {
  const chromiumPath = puppeteer.executablePath();
 
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      // Custom middleware plugin to handle CSS 404s
      {'middleware:css-mock': ['factory', function() {
        return function(req, res, next) {
          // Mock any CSS requests to prevent 404 warnings
          if (req.url.endsWith('.css')) {
            res.writeHead(200, {'Content-Type': 'text/css'});
            res.end('/* mock css for testing */');
            return;
          }
          next();
        };
      }]}
    ],
    client: {
      jasmine: {
        random: false
      },
      clearContext: false
    },
    jasmineHtmlReporter: {
      suppressAll: true
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
    // Add the custom middleware to handle CSS requests
    middleware: ['css-mock'],
    // Reduce log level to only show errors, not warnings
    logLevel: config.LOG_WARNING,
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
 
  process.env.CHROME_BIN = chromiumPath;
};