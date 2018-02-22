define([
    'dojo/topic',
    'dojo/_base/lang',

    'esri/graphic',
    'esri/symbols/SimpleFillSymbol',
    'esri/symbols/SimpleLineSymbol',
    'esri/symbols/SimpleMarkerSymbol'
], (
    topic,
    lang,

    Graphic,
    SimpleFillSymbol,
    SimpleLineSymbol,
    SimpleMarkerSymbol
) => {
    return {
        // description:
        //      Handles interaction between app widgets and the  Mostly Layerthrough pub/sub

        version: '1.1.0',

        // handles: Object[]
        //      container to track handles for this object
        handles: null,

        graphic: null,

        symbols: null,

        // Properties to be sent into constructor

        // esri/GraphicsLayer - the graphics layer for the map
        graphicsLayer: null,

        initialize(config, symbols) {
            // summary:
            //      set up default highlights and topics
            // topics: {
            //     graphics: {
            //         highlight:,
            //         clear:
            //     }
            // },
            // symbols: {
            //     point: {},
            //     line: {},
            //     poly: {}
            // }
            console.info('app/GraphicsController::initialize', arguments);

            if (symbols || config.symbols) {
                this.symbols = symbols || config.symbols;
            } else {
                this.symbols = {
                    point: null,
                    line: null,
                    poly: null
                };

                this.symbols.point = new SimpleMarkerSymbol({
                    color: [240, 28, 190, 200], // eslint-disable-line no-magic-numbers
                    size: 7.5,
                    angle: 0,
                    xoffset: 0,
                    yoffset: 0,
                    type: 'esriSMS',
                    style: 'esriSMSCircle',
                    outline: {
                        color: [0, 31, 63, 255], // eslint-disable-line no-magic-numbers
                        width: 0.5,
                        type: 'esriSLS',
                        style: 'esriSLSSolid'
                    }
                });
                this.symbols.line = new SimpleLineSymbol({
                    color: [255, 0, 197, 255], // eslint-disable-line no-magic-numbers
                    width: 1.5,
                    type: 'esriSLS',
                    style: 'esriSLSDashDot'
                });
                this.symbols.poly = new SimpleFillSymbol({
                    color: [240, 28, 190, 200], // eslint-disable-line no-magic-numbers
                    outline: {
                        color: [0, 31, 63, 255], // eslint-disable-line no-magic-numbers
                        width: 0.5,
                        type: 'esriSLS',
                        style: 'esriSLSSolid'
                    },
                    type: 'esriSFS',
                    style: 'esriSFSSolid'
                });
            }

            this.handles = [];
            this.handles.push(
                topic.subscribe(config.topics.graphics.highlight,
                    lang.hitch(this, 'highlight')),
                topic.subscribe(config.topics.graphics.clear,
                    lang.hitch(this, 'removeGraphic'))
            );
        },
        highlight(graphic, symbol, additionalProps) {
            // summary:
            //      adds the clicked shape geometry to the graphics layer
            //      highlighting it
            // graphic - esri/Graphic
            // symbol - esri/symbol/* to overwrite the default
            console.info('app/GraphicsController::highlight', arguments);

            if (!graphic) {
                return;
            }

            this.removeGraphic(this.graphic);

            if (!symbol) {
                switch (lang.getObject('geometry.type', false, graphic) || graphic[0].geometry.type) {
                    case 'extent':
                    case 'polygon':
                        symbol = this.symbols.poly;
                        break;
                    case 'polyline':
                        symbol = this.symbols.line;
                        break;
                    default:
                        symbol = this.symbols.point;
                }
            }

            if (Array.isArray(graphic)) {
                this.graphic = [];

                graphic.forEach((item) => {
                    var g = new Graphic(item.geometry, symbol, item.attributes);
                    lang.mixin(g, additionalProps);

                    this.graphic.push(g);
                    this.graphicsLayer.add(g);
                }, this);
            } else {
                this.graphic = new Graphic(graphic.geometry, symbol);
                lang.mixin(this.graphic, additionalProps);

                this.graphicsLayer.add(this.graphic);
            }
        },
        removeGraphic(graphic) {
            // summary:
            //      removes the graphic from the map
            // graphic - esri/Graphic
            console.info('app/GraphicsController::removeGraphic', arguments);

            graphic = lang.getObject('graphic', false, graphic) || graphic || this.graphic;
            if (!graphic) {
                return;
            }

            if (Array.isArray(graphic)) {
                graphic.forEach((item) => {
                    this.graphicsLayer.remove(item);
                }, this);
            } else {
                this.graphicsLayer.remove(graphic);
            }

            this.graphic = null;
        },
        destroy() {
            // summary:
            //      destroys all handles
            console.info('app/GraphicsController::destroy', arguments);

            this.handles.forEach((hand) => {
                hand.remove();
            });

            this.removeGraphic();
        }
    };
});
