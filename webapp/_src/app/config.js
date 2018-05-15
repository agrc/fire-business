define([
    'dojo/has',

    'esri/config'
], (
    has,

    esriConfig
) => {
    const gisServerBaseUrl = 'https://maps.ffsl.utah.gov';

    const config = {
        // app: app.App
        //      global reference to App
        app: null,

        // cache: app.cache
        //      a spot to cache some data
        cache: {},

        // version.: String
        //      The version number.
        version: '1.0.1',

        urls: {
            origin: gisServerBaseUrl + '/arcgis/rest/services/${env}/FireOrigin/MapServer/0',
            perimeter: gisServerBaseUrl + '/arcgis/rest/services/${env}/BurnArea/MapServer/0',
            landown: 'https://gis.trustlands.utah.gov/server/' +
                     '/rest/services/Ownership/UT_SITLA_Ownership_LandOwnership_WM/FeatureServer/0'
        },

        topics: {
            toast: '1'
        },

        renderers: {
            point: {
                type: 'simple',
                symbol: {
                    type: 'esriPMS',
                    url: 'd903ef1a7b82ca33817bc92c2e28131b',
                    imageData: 'iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAAXNSR0IB2cksfwAAAAlwSF' +
                    'lzAAAOxAAADsQBlSsOGwAAAwdJREFUOI2t1F1IU2EYB/D/u7Nztk7bmWdq0mrhZy5FyYmDpIgk+yAI+tA' +
                    'ihTCIiLLSIojULQProoKMLuwiCLRSqS5LN+qimygmp8hac1pYmmj5Pee2zk4XteFxW0T0vzzP8/zOed+X' +
                    '8yrxn6OMV3gF0MqS7XuZrNR9UDHLCUNL0sxMIDg87Ap9/txhFgTHX4O9R47sIpzuWkp5WTKXk7NUybKRW' +
                    'mB6uvjb8+fFb+60fg8O9B0rdDrf/hF0na9vUmdmVBsrKzQKZfT7GI6jDDt25KaUlAQ+nDnb2SOKJ8yCYI8' +
                    'JupsuHyUUOeN776JFvx+xwHD84+NMKBAwqYrXNzspqiz8pZGJ3qoqHaGVTRm1NfSEIMBju4BMawNojSYKm' +
                    'xsawoCtEem2Bqj0+vR3Bw/dhNO5UQaqiyzXkrdtTSAKBfRmM0AIPFYrMm020FptTIxdsQIAmMSDlYk9fa7' +
                    'NZkFwREDRO7tNk5oaGdQXFIBQFDxWKzJsNjAcFwsDACRt2GAaMxrLsRCkeZ4nhMiWxufng1RVwVPfAMPhw/' +
                    'hyvTkKCx8UbTCYZEtWqFTqWJufkJcH/86d+Li/Emn3W6OwcIhWy8hASRRDAKjFjXNDQxi73w5D81UMt9wC' +
                    'e7ERTIIuCpSCQSIDRa93DoB2YdPiPVMvN8BTV4eMi41Q8Xyk74fPB8kXGJGBFMe5faOjhUuWLYuJAYBuj' +
                    'Qmk+jj66+p/oXo9AGCqt9f7w/PpngzUZGefG7M7ulZVHCDxThMAuOxsrDx1Ep7fKKPTYbSjcyz49PEDGZh' +
                    'ksdgH29tffH3Ste77g4cxsQialQXj6Vr0n68HW1jglaanaouAoAwEAOOWLZs+tncOJO7ZbYiHhaNNSwNbs' +
                    'NY/73bfzm1peRR+LgMJz/uliYn0Lw7Hs8G2u+uSSzcjvKeRSBJmBwcx0tE5GQoEG3KuXLmxsBz19xOe9wMo' +
                    '/vbyZelod/clcWZmNcWyLKGUilAwMC9OTo4ThnH4Xgs1+W1tE4vn414nSRaLHYA9Xj1e4t9P/5ifVloqXS' +
                    '6Jgk8AAAAASUVORK5CYII=',
                    contentType: 'image/png',
                    width: 15,
                    height: 15,
                    angle: 0,
                    xoffset: 0,
                    yoffset: 0
                }
            },
            poly: {
                type: 'simple',
                symbol: {
                    type: 'esriSFS',
                    style: 'esriSFSSolid',
                    color: [0, 0, 0, 0],
                    outline: {
                        type: 'esriSLS',
                        style: 'esriSLSSolid',
                        color: [242, 20, 69, 255], // eslint-disable-line no-magic-numbers
                        width: 3
                    }
                }
            }
        }
    };

    esriConfig.defaults.io.corsEnabledServers.push('maps.ffsl.utah.gov');
    esriConfig.defaults.io.corsEnabledServers.push('gis.trustlands.utah.gov');
    esriConfig.defaults.io.corsEnabledServers.push('mapserv.utah.gov');
    esriConfig.defaults.io.corsEnabledServers.push('api.mapserv.utah.gov');
    esriConfig.defaults.io.corsEnabledServers.push('discover.agrc.utah.gov');
    esriConfig.defaults.map.zoomSymbol.outline.color = [77, 137, 134, 255]; // eslint-disable-line no-magic-numbers

    return config;
});
