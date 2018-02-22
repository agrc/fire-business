define([
    './config',

    'dojo/topic',

    'esri/geometry/Multipoint',
    'esri/geometry/Point',
    'esri/graphic'
], function (
    config,

    topic,

    Multipoint,
    Point,
    Graphic
) {
    return {
        // description:
        //      Handles interaction between app widgets and the map

        version: '1.1.0',

        // handles: Object[]
        //      container to track handles for this object
        handles: null,

        // Properties to be sent into initialize
        map: null,

        // the active layer
        activeLayer: null,

        zoomLevel: 18,


        initialize(map) {
            // summary:
            //      set map
            console.info('app/MapController::initialize', arguments);

            this.map = map;
            this.handles = [];
            this.activeLayer = [];
        },
        activateLayer(layer) {
            // summary:
            //      activates the layer for other functions
            // none
            console.info('app/MapController:activateLayer', arguments);

            if (!layer) {
                return;
            }

            this.activeLayer.push(layer);
        },
        deactivateLayer(layer) {
            // summary:
            //      activates the layer for other functions
            // none
            console.info('app/MapController:deactivateLayer', arguments);

            if (!layer) {
                return;
            }

            this.activeLayer.pop(layer);
        },
        zoom(graphic) {
            // summary:
            //      zooms to things
            // graphic - esri/Graphic, esri/FeatureSet
            console.info('app/MapController::zoom', arguments);

            if (!graphic) {
                return;
            }

            if (Array.isArray(graphic)) {
                if (graphic.length === 0) {
                    return;
                }

                if (graphic[0].geometry.type === 'point') {
                    var multiPoint = new Multipoint(graphic[0].geometry.spatialReference);

                    graphic.forEach(function (point) {
                        multiPoint.addPoint(point.geometry);
                    });

                    return this._setExtent(new Graphic(multiPoint));
                }

                graphic = graphic.reduce(function (a, b) {
                    return a.concat(b);
                });

                if (graphic.length === 0) {
                    return;
                }
            }

            return this._setExtent(graphic);
        },
        _setExtent(graphic) {
            // summary:
            //      sets the map extent
            // graphic: esri graphic.
            console.info('app/MapController::_setExtent', arguments);

            if (!graphic) {
                return;
            }

            if (graphic.geometry.type === 'point') {
                // graphic is a point geometry
                this.map.centerAndZoom(graphic.geometry, this.zoomLevel);

                return;
            }

            var extent;
            if (graphic.geometry.type === 'multipoint') {
                // graphic is a point geometry
                extent = graphic.geometry.getExtent();
            } else {
                extent = graphic.geometry.getExtent();
                // extent = graphicsUtils.graphicsExtent(graphic);
            }

            if (!extent.getWidth() && !extent.getHeight()) {
                // we are looking at the extent of a point
                this.map.centerAndZoom(new Point(extent.xmin, extent.ymin, this.map.spatialReference),
                    this.zoomLevel);
            } else {
                this.map.setExtent(extent, true);
            }

            return extent;
        },
        destroy() {
            // summary:
            //      destroys all handles
            console.info('app/MapController::destroy', arguments);

            this.handles.forEach(function (hand) {
                hand.remove();
            });
        }
    };
});
