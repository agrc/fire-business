define([
    './config',
    './MapController',

    'dijit/_TemplatedMixin',
    'dijit/_WidgetBase',

    'dojo/dom-construct',
    'dojo/text!./templates/App.html',
    'dojo/_base/declare',
    'dojo/request',
    'dojo/promise/all',

    'esri/map',
    'esri/graphic',
    'esri/graphicsUtils',
    'esri/SpatialReference',
    'esri/geometry/Extent',
    'esri/geometry/Polygon',
    'esri/layers/FeatureLayer',
    'esri/layers/GraphicsLayer',
    'esri/renderers/jsonUtils',

    'layer-selector'
], (
    config,
    mapController,

    _TemplatedMixin,
    _WidgetBase,

    domConstruct,
    template,
    declare,
    request,
    all,

    Map,
    Graphic,
    graphicsUtils,
    SpatialReference,
    Extent,
    Polygon,
    FeatureLayer,
    GraphicsLayer,
    rendererJsonUtils,

    LayerSelector,
) => {
    return declare([_WidgetBase, _TemplatedMixin], {
        // summary:
        //      The main widget for the app

        widgetsInTemplate: true,
        templateString: template,
        baseClass: 'app',
        incidentId: null,
        quadWord: null,
        env: null,

        // childWidgets: Object[]
        //      container for holding custom child widgets
        childWidgets: null,

        constructor() {
            // summary:
            //      first function to fire after page loads
            console.info('app/App::constructor', arguments);

            config.app = this;
            this.childWidgets = [];

            this.inherited(arguments);
        },
        postCreate() {
            // summary:
            //      Fires when
            console.info('app/App::postCreate', arguments);

            this.createMap();

            this.setupConnections();

            this.inherited(arguments);
        },
        setupConnections() {
            // summary:
            //      Fires when
            console.info('app/App::setupConnections', arguments);
        },
        startup() {
            // summary:
            //      Fires after postCreate when all of the child widgets are finished laying out.
            console.info('app/App::startup', arguments);

            this.childWidgets.forEach((widget) => {
                console.log(widget.declaredClass);
                this.own(widget);
                widget.startup();
            }, this);

            this.inherited(arguments);
        },
        createMap() {
            // summary:
            //      Sets up the map
            console.info('app/App::createMap', arguments);

            mapController.initialize(new Map(this.mapNode, {
                useDefaultBaseMap: false,
                extent: new Extent({
                    xmax: -12010849.397533866,
                    xmin: -12898741.918094235,
                    ymax: 5224652.298632992,
                    ymin: 4422369.249751998,
                    spatialReference: {
                        wkid: 3857
                    }
                })
            }));

            this.childWidgets.push(
                new LayerSelector({
                    map: mapController.map,
                    right: false,
                    quadWord: this.quadWord,
                    baseLayers: ['Hybrid', 'Lite', 'Terrain', {
                        token: 'Topo',
                        selected: true
                    }, 'Color IR'],
                    overlays: ['Address Points', {
                        Factory: FeatureLayer,
                        url: config.urls.landown,
                        id: 'Land Ownership',
                        opacity: 0.5
                    }]
                })
            );

            if (mapController.map.loaded) {
                this.getFireFeatures();
            } else {
                mapController.map.on('load', () => this.getFireFeatures());
            }
        },
        getFireFeatures() {
            // summary:
            //      description
            // param or return
            console.info('app/App:getFireFeatures', arguments);

            const wm = 3857;
            this.spatialReference = new SpatialReference(wm);

            this.perimeterGraphics = new GraphicsLayer({
                id: 'perimeter'
            });
            this.originGraphics = new GraphicsLayer({
                id: 'origin'
            });
            mapController.map.addLayer(this.perimeterGraphics);
            mapController.map.addLayer(this.originGraphics);

            all([
                this.queryLayerFor(config.urls.origin.replace('${env}', this.env), `id=${this.incidentId}`),
                this.queryLayerFor(config.urls.perimeter.replace('${env}', this.env), `id=${this.incidentId}`)
            ]).then((items) => this.displayFireGeometries(items));
        },
        queryLayerFor(url, expression) {
            // summary:
            //      queries a url for a FeatureLayer
            // returns graphics?
            console.info('app/App:queryLayerFor', arguments);

            return request.get(`${url}/query`, {
                method: 'get',
                timeout: 15000,
                handleAs: 'json',
                headers: {
                    'X-Requested-With': null
                },
                query: {
                    where: expression,
                    f: 'json',
                    outFields: 'id',
                    returnGeometry: true,
                    outSR: 3857
                }
            });
        },
        displayFireGeometries(response) {
            // summary:
            //      takes the responses from the feature layers and shows them as graphics
            // places graphics on the map
            console.info('app/App:displayFireGeometries', arguments);

            if (!response || !response.length) {
                return;
            }

            const pointGraphics = [];
            const polyGraphics = [];
            let extent;

            response.forEach((featuresSet) => {
                if (!featuresSet || !featuresSet.features || !featuresSet.features.length) {
                    return;
                }

                let addTo;
                let renderer;
                let graphics;
                const type = featuresSet.geometryType;

                if (type === 'esriGeometryPolygon') {
                    renderer = rendererJsonUtils.fromJson(config.renderers.poly);
                    addTo = polyGraphics;
                    graphics = featuresSet.features.map((feature) => {
                        const poly = new Polygon(feature.geometry);
                        poly.setSpatialReference(this.spatialReference);

                        return new Graphic(poly);
                    });
                } else {
                    renderer = rendererJsonUtils.fromJson(config.renderers.point);
                    addTo = pointGraphics;
                    graphics = featuresSet.features.map((feature) => {
                        const geometry = feature.geometry;
                        const coords = [
                            [geometry.x, geometry.y],
                            [geometry.x - 1, geometry.y - 1],
                            [geometry.x + 1, geometry.y + 1],
                            [geometry.x, geometry.y]
                        ];

                        const poly = new Polygon(coords);
                        poly.setSpatialReference(this.spatialReference);

                        return new Graphic(poly);
                    });
                }

                const featureExtent = graphicsUtils.graphicsExtent(graphics);
                if (extent) {
                    extent = extent.union(featureExtent);
                } else {
                    extent = featureExtent;
                }

                featuresSet.features.forEach((feature) => {
                    const graphic = new Graphic(feature);
                    graphic.geometry.setSpatialReference(this.spatialReference);
                    graphic.setSymbol(renderer.symbol);

                    addTo.push(graphic);
                });
            });

            mapController.map.setExtent(extent, true);

            pointGraphics.forEach((graphic) => this.originGraphics.add(graphic));
            polyGraphics.forEach((graphic) => this.perimeterGraphics.add(graphic));
        }
    });
});
