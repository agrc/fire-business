﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.SOESupport;
using fire_business_soe.Commands;
using fire_business_soe.Comparers;
using fire_business_soe.Encoding;
using fire_business_soe.Models;
using Newtonsoft.Json;

//TODO: sign the project (project properties > signing tab > sign the assembly)
//      this is strongly suggested if the dll will be registered using regasm.exe <your>.dll /codebase

namespace fire_business_soe
{
    [ComVisible(true)]
    [Guid("d0fec625-d8c6-42b8-9f47-b356c76619c8")]
    [ClassInterface(ClassInterfaceType.None)]
    [ServerObjectExtension("MapServer", //use "MapServer" if SOE extends a Map service and "ImageServer" if it extends an Image service.
        AllCapabilities = "",
        DefaultCapabilities = "",
        Description = "Return intersection statistics from input",
        DisplayName = "fbs.soe",
        Properties = "",
        SupportsREST = true,
        SupportsSOAP = false)]
    public class FireBusinessSoe : JsonEndpoint, IServerObjectExtension, IObjectConstruct, IRESTRequestHandler
    {
        private const string Version = "1.0.0";
        private static ServerLogger _logger;
        private static Collection<FeatureClassIndexMap> _featureClassIndexMap;
        private readonly IRESTRequestHandler _reqHandler;
        private readonly string _soeName;
        private IServerObjectHelper _serverObjectHelper;

        public FireBusinessSoe()
        {
            _soeName = GetType().Name;
            _logger = new ServerLogger();
            _reqHandler = new SoeRestImpl(_soeName, CreateRestSchema());
        }

        public static int MessageCode { get; set; }

        public void Construct(IPropertySet props)
        {
            _featureClassIndexMap = new CreateLayerMapCommand(_serverObjectHelper).Execute();
        }

        private RestResource CreateRestSchema()
        {
            var rootRes = new RestResource(_soeName, false, RootResHandler);

            var sampleOper = new RestOperation("ExtractIntersections",
                new[] {"geometry", "criteria"},
                new[] {"json"},
                ExtractHandler);

            rootRes.operations.Add(sampleOper);

            return rootRes;
        }

        private static byte[] RootResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
            {
                Description = "Extract intersection information",
                CreatedBy = "AGRC - Steve Gourley @steveAGRC",
                Version
            }));
        }

        private static byte[] ExtractHandler(NameValueCollection boundVariables,
            JsonObject operationInput,
            string outputFormat,
            string requestProperties,
            out string responseProperties)
        {
            responseProperties = null;
            const string methodName = "ExtractIntersection";
            var errors = new ResponseContainer(HttpStatusCode.BadRequest, "");
            JsonObject jsonGeometry;
            JsonObject jsonCriteria;

            if (!operationInput.TryGetJsonObject("geometry", out jsonGeometry))
            {
                errors.AddMessage("geometry parameter is required.");
            }

            if (!operationInput.TryGetJsonObject("criteria", out jsonCriteria))
            {
                errors.AddMessage("criteria parameter is required.");
            }

            if (errors.HasErrors)
            {
                return Json(errors);
            }

#if !DEBUG
            _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, "Params received");
#endif
            var inputGeometry = Conversion.ToGeometry(jsonGeometry, esriGeometryType.esriGeometryPolygon) as IPolygon;

            if (inputGeometry == null)
            {
                errors.AddMessage("geometry json is invalid.");
            }

            var queryCriteria = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jsonCriteria.ToJson());

            Dictionary<int, IEnumerable<string>> criteria;

            try
            {
                criteria = queryCriteria.ToDictionary(key => int.Parse(key.Key), values => values.Value.Select(x => x.ToUpper()));
            }
            catch (Exception ex)
            {
                errors.AddMessage("could not parse criteria. json is invalid.");
                errors.AddMessage(ex.Message);

                return Json(errors);
            }

            if (criteria.Keys.Count > 1)
            {
                errors.AddMessage("criteria json is empty and must have values.");
            }

            if (errors.HasErrors)
            {
                return Json(errors);
            }

#if !DEBUG
            _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, "Params valid");
#endif

            var filterGeometry = (ITopologicalOperator4) inputGeometry;
            if (filterGeometry == null)
            {
                errors.Message = "input geometry could not become a topological operator.";

                return Json(errors);
            }

            filterGeometry.IsKnownSimple_2 = false;
            filterGeometry.Simplify();

            if (((IArea) inputGeometry).Area < 0)
            {
                ((ICurve) inputGeometry).ReverseOrientation();
            }

            var filter = new SpatialFilter
            {
                Geometry = inputGeometry,
                SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
            };

            var searchResults = new Dictionary<string, IList<IntersectAttributes>>();

            foreach (var pair in criteria)
            {
                var layerIndex = pair.Key;
                var fields = pair.Value;

                var container = _featureClassIndexMap.Single(x => x.Index == layerIndex);
                var fieldMap = container.FieldMap.Select(x => x.Value)
                    .Where(y => fields.Contains(y.Field.ToUpper()))
                    .ToList();

#if !DEBUG
                _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode,
                    string.Format("Querying {0} at index {1}", container.LayerName, container.Index));
#endif

                var cursor = container.FeatureClass.Search(filter, true);
                IFeature feature;
                while ((feature = cursor.NextFeature()) != null)
                {
                    var values = new GetValueAtIndexCommand(fieldMap, feature).Execute();
                    var attributes = new IntersectAttributes(values);

                    // line over polygon = 1D
                    // polygon over polygon = 2D

#if !DEBUG
                    _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode,
                        "intersecting " + container.LayerName);
#endif
                    var gis = (ITopologicalOperator4) inputGeometry;
                    gis.Simplify();

                    IGeometry intersection;
                    switch (feature.ShapeCopy.GeometryType)
                    {
                        case esriGeometryType.esriGeometryPolygon:
                            try
                            {
                                intersection = gis.Intersect(feature.ShapeCopy, esriGeometryDimension.esriGeometry2Dimension);

                                var area = (IArea) intersection;
                                attributes.Intersect = Math.Abs(area.Area);
#if !DEBUG
                                _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Area: {0}", area.Area));
#endif
                            }
                            catch (Exception ex)
                            {
                                return Json(new ResponseContainer(HttpStatusCode.InternalServerError, ex.Message));
                            }

                            break;
                        case esriGeometryType.esriGeometryPolyline:
                            try
                            {
                                intersection = gis.Intersect(feature.ShapeCopy, esriGeometryDimension.esriGeometry1Dimension);

                                var length = (IPolyline5) intersection;
                                attributes.Intersect = Math.Abs(length.Length);
#if !DEBUG
                                _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Length: {0}", length.Length));
#endif
                            }
                            catch (Exception ex)
                            {
                                return Json(new ResponseContainer(HttpStatusCode.InternalServerError, ex.Message));
                            }

                            break;
                    }

                    if (searchResults.ContainsKey(container.LayerName))
                    {
                        if (searchResults[container.LayerName].Any(x => new MultiSetComparer<object>().Equals(x.Attributes, attributes.Attributes)))
                        {
                            var duplicate = searchResults[container.LayerName]
                                .Single(x => new MultiSetComparer<object>().Equals(x.Attributes, attributes.Attributes));

                            duplicate.Intersect += attributes.Intersect;
                        }
                        else
                        {
                            searchResults[container.LayerName].Add(attributes);
                        }
                    }
                    else
                    {
                        searchResults[container.LayerName] = new Collection<IntersectAttributes> { attributes };
                    }
                }
            }

            var response = new IntersectResult(searchResults);

#if !DEBUG
            _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Returning results {0}", searchResults.Count));
#endif

            return Json(new ResponseContainer<IntersectResult>(response));
        }

        public void Init(IServerObjectHelper pSoh)
        {
            _serverObjectHelper = pSoh;
        }

        public void Shutdown()
        {
        }

        public string GetSchema()
        {
            return _reqHandler.GetSchema();
        }

        public byte[] HandleRESTRequest(string capabilities, string resourceName, string operationName, string operationInput, string outputFormat,
            string requestProperties, out string responseProperties)
        {
            return _reqHandler.HandleRESTRequest(capabilities, resourceName, operationName, operationInput, outputFormat, requestProperties,
                out responseProperties);
        }
    }
}