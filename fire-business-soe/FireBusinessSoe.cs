using System;
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

        public void Init(IServerObjectHelper pSoh)
        {
            _serverObjectHelper = pSoh;
        }

        public void Shutdown()
        {
        }

        private RestResource CreateRestSchema()
        {
            var rootRes = new RestResource(_soeName, false, RootResHandler);

            var sampleOper = new RestOperation("ExtractIntersections",
                new[] {"id"},
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
            double? featureId;

            if (!operationInput.TryGetAsDouble("id", out featureId) && featureId.HasValue)
            {
                errors.AddMessage("The id of the shape is required.");
            }

            if (errors.HasErrors)
            {
                return Json(errors);
            }

#if !DEBUG
            _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, "Params received");
#endif

            var fireLayerMap = _featureClassIndexMap.First(x => x.LayerName == "Fire Perimeters");
            var fireLayer = fireLayerMap.FeatureClass;

            var perimeterFeature = fireLayer.GetFeature(Convert.ToInt32(featureId.Value));
            var inputGeometry = perimeterFeature.ShapeCopy;

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

            var totalArea = ((IArea) inputGeometry).Area;
            if (totalArea < 0)
            {
                ((ICurve) inputGeometry).ReverseOrientation();
            }

            var filter = new SpatialFilter
            {
                Geometry = inputGeometry,
                SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
            };

            var searchResults = new Dictionary<string, IList<IntersectAttributes>>
            {
                {
                    "fire", new[]
                    {
                        new IntersectAttributes(new[]
                        {
                            new KeyValuePair<string, object>("total", "total")
                        })
                        {
                            Intersect = totalArea
                        }
                    }
                }
            };

            var criterias = new List<Criteria>
            {
                new Criteria(1, new[] {"NAME"}, "muni"),
                new Criteria(1, new[] {"NAME"}, "muniPrivate", new CriteriaFilter(3, "OWNER = 'Private'")),
                new Criteria(2, new[] {"NAME"}, "county"),
                new Criteria(2, new[] {"NAME"}, "countyPrivate", new CriteriaFilter(3, "OWNER = 'Private'")),
                new Criteria(3, new[] {"OWNER"}, "owner"),
                new Criteria(3, new[] {"ADMIN"}, "admin"),
                new Criteria(4, new[] {"STATE_NAME"}, "state")
            };

            foreach (var criteria in criterias)
            {
                // get the IFeatureClass
                var container = _featureClassIndexMap.Single(x => x.Index == criteria.LayerIndex);
                // get the index of the fields to calculate intersections for
                var fieldMap = container.FieldMap.Select(x => x.Value)
                    .Where(y => criteria.Attributes.Contains(y.Field.ToUpper()))
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

#if !DEBUG
                    _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode,
                        "intersecting " + container.LayerName);
#endif
                    var gis = (ITopologicalOperator4) inputGeometry;
                    gis.Simplify();

                    IntersectionPart intersectionPart;
                    try
                    {
                        intersectionPart = GetIntersectionAndSize(feature, gis);
                    }
                    catch (Exception ex)
                    {
                        return Json(new ResponseContainer(HttpStatusCode.InternalServerError, ex.Message));
                    }

                    // need to double filter
                    if (criteria.Filter != null)
                    {
                        var subFilter = new SpatialFilter
                        {
                            Geometry = intersectionPart.Intersection,
                            SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects,
                            WhereClause = criteria.Filter.WhereClause
                        };
                        var subGis = (ITopologicalOperator4)intersectionPart.Intersection;
                        subGis.Simplify();

                        var filterContainer = _featureClassIndexMap.Single(x => x.Index == criteria.Filter.LayerId);
                        var filterCursor = filterContainer.FeatureClass.Search(subFilter, true);
                        IFeature filterFeature;
                        while ((filterFeature = filterCursor.NextFeature()) != null)
                        {
                            var subAttributes = new IntersectAttributes(values);
                            IntersectionPart subIntersectionPart;
                            try
                            {
                                subIntersectionPart = GetIntersectionAndSize(filterFeature, subGis);
                            }
                            catch (Exception ex)
                            {
                                return Json(new ResponseContainer(HttpStatusCode.InternalServerError, ex.Message));
                            }

                            subAttributes.Intersect = subIntersectionPart.Size;

                            if (searchResults.ContainsKey(criteria.JsonPropertyName))
                            {
                                if (searchResults[criteria.JsonPropertyName].Any(x => new MultiSetComparer<object>().Equals(x.Attributes, subAttributes.Attributes)))
                                {
                                    var duplicate = searchResults[criteria.JsonPropertyName]
                                        .Single(x => new MultiSetComparer<object>().Equals(x.Attributes, subAttributes.Attributes));

                                    duplicate.Intersect += subAttributes.Intersect;
                                }
                                else
                                {
                                    searchResults[criteria.JsonPropertyName].Add(subAttributes);
                                }
                            }
                            else
                            {
                                searchResults[criteria.JsonPropertyName] = new Collection<IntersectAttributes> { subAttributes };
                            }
                        }
                    }
                    else
                    {
                        attributes.Intersect = intersectionPart.Size;

                        if (searchResults.ContainsKey(criteria.JsonPropertyName))
                        {
                            if (searchResults[criteria.JsonPropertyName].Any(x => new MultiSetComparer<object>().Equals(x.Attributes, attributes.Attributes)))
                            {
                                var duplicate = searchResults[criteria.JsonPropertyName]
                                    .Single(x => new MultiSetComparer<object>().Equals(x.Attributes, attributes.Attributes));

                                duplicate.Intersect += attributes.Intersect;
                            }
                            else
                            {
                                searchResults[criteria.JsonPropertyName].Add(attributes);
                            }
                        }
                        else
                        {
                            searchResults[criteria.JsonPropertyName] = new Collection<IntersectAttributes> {attributes};
                        }
                    }
                }
            }

            var response = new IntersectResult(searchResults);

#if !DEBUG
            _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Returning results {0}", searchResults.Count));
#endif

            return Json(new ResponseContainer<IntersectResult>(response));
        }

        private static IntersectionPart GetIntersectionAndSize(IFeature feature, ITopologicalOperator4 gis)
        {
            // line over polygon = 1D
            // polygon over polygon = 2D

            const string methodName = "GetIntersectionAndSize";

            IGeometry intersection;
            var part = new IntersectionPart();
            switch (feature.ShapeCopy.GeometryType)
            {
                case esriGeometryType.esriGeometryPolygon:
                    intersection = gis.Intersect(feature.ShapeCopy, esriGeometryDimension.esriGeometry2Dimension);

                    var area = (IArea) intersection;
                    part.Size = Math.Abs(area.Area);
                    part.Intersection = intersection;
#if !DEBUG
                    _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Area: {0}", area.Area));
#endif
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    intersection = gis.Intersect(feature.ShapeCopy, esriGeometryDimension.esriGeometry1Dimension);

                    var length = (IPolyline5) intersection;
                    part.Size = Math.Abs(length.Length);
                    part.Intersection = intersection;
#if !DEBUG
                    _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Length: {0}", length.Length));
#endif
                    break;
            }

            return part;
        }
    }
}