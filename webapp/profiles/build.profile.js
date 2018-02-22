/* eslint-disable no-unused-vars */
var profile = {
    basePath: '../src',
    action: 'release',
    cssOptimize: 'comments',
    mini: true,
    optimize: false,
    layerOptimize: false,
    selectorEngine: 'acme',
    layers: {
        'dojo/dojo': {
            include: [
                'app/App',
                'app/packages',
                'app/run',
                'dojo/domReady',
                'dojo/i18n',
                'dojox/gfx/filters',
                'dojox/gfx/path',
                'dojox/gfx/shape',
                'dojox/gfx/svg',
                'dojox/gfx/svgext',
                'esri/dijit/Attribution',
                'esri/layers/ArcGISDynamicMapServiceLayer',
                'esri/layers/VectorTileLayerImpl'
            ],
            includeLocales: ['en-us'],
            customBase: true,
            boot: true
        }
    },
    packages: [{
        name: 'app',
        trees: [
            // don't bother with .hidden, tests, min, src, and templates
            ['.', '.', /(\/\.)|(~$)|(tests)/]
        ],
        resourceTags: {
            amd: function amd(filename, mid) {
                return /\.js$/.test(filename);
            }
        }
    }, {
        name: 'moment',
        location: 'moment',
        main: 'moment',
        trees: [
            // don't bother with .hidden, tests, min, src, and templates
            ['.', '.', /(\/\.)|(~$)|(test|txt|src|min|templates)/]
        ],
        resourceTags: {
            amd: function amd(filename, mid) {
                return /\.js$/.test(filename);
            }
        }
    }, {
        name: 'proj4',
        trees: [
            // don't bother with .hidden, tests, min, src, and templates
            ['.', '.', /(\/\.)|(~$)|(test|txt|src|min|html)/]
        ],
        resourceTags: {
            amd: function amd() {
                return true;
            },
            copyOnly: function copyOnly() {
                return false;
            }
        }
    }],
    staticHasFeatures: {
        'dojo-trace-api': 0,
        'dojo-log-api': 0,
        'dojo-publish-privates': 0,
        'dojo-sync-loader': 0,
        'dojo-xhr-factory': 0,
        'dojo-test-sniff': 0,
        'extend-esri': 0,
        'config-deferredInstrumentation': 0,
        'config-dojo-loader-catches': 0,
        'config-tlmSiblingOfDojo': 0,
        'dojo-amd-factory-scan': 0,
        'dojo-combo-api': 0,
        'dojo-config-api': 1,
        'dojo-config-require': 0,
        'dojo-debug-messages': 0,
        'dojo-dom-ready-api': 1,
        'dojo-firebug': 0,
        'dojo-guarantee-console': 1,
        'dojo-has-api': 1,
        'dojo-inject-api': 1,
        'dojo-loader': 1,
        'dojo-modulePaths': 0,
        'dojo-moduleUrl': 0,
        'dojo-requirejs-api': 0,
        'dojo-sniff': 1,
        'dojo-timeout-api': 0,
        'dojo-undef-api': 0,
        'dojo-v1x-i18n-Api': 1,
        'dom': 1, // eslint-disable-line
        'host-browser': 1,
        'extend-dojo': 1
    },
    userConfig: {
        packages: ['app', 'dijit', 'dojox', 'agrc', 'esri', 'layer-selector', {
            name: 'toaster',
            location: './toaster/dist'
        }]
    }
};
