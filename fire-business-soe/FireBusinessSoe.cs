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

            var whole = (ITopologicalOperator4) inputGeometry;
            whole.Simplify();

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
                new Criteria
                {
                    LayerIndex = 1,
                    Attributes = new[] {"NAME"},
                    JsonPropertyName = "muni",
                    CalculationCommand = new CalculateIntersectionCommand(whole, _logger)
                },
                new Criteria
                {
                    LayerIndex = 1,
                    FeatureClassIndexMap = _featureClassIndexMap,
                    Attributes = new[] {"NAME"},
                    JsonPropertyName = "muniPrivate",
                    CalculationCommand = new CalculateMuniPrivateCommand(whole, _logger)
                    {
                        LandOwnership = _featureClassIndexMap.Single(x => x.Index == 3)
                    }
                },
                new Criteria
                {
                    LayerIndex = 2,
                    Attributes = new[] {"NAME"},
                    JsonPropertyName = "county",
                    CalculationCommand = new CalculateIntersectionCommand(whole, _logger)
                },
                new Criteria
                {
                    LayerIndex = 2,
                    Attributes = new[] {"NAME"},
                    JsonPropertyName = "countyPrivate",
                    CalculationCommand = new CalculateCountyPrivateCommand(whole, _logger)
                    {
                        LandOwnership = _featureClassIndexMap.Single(x => x.Index == 3),
                        Municipalities = _featureClassIndexMap.Single(x => x.Index == 1)
                    }
                },
                new Criteria
                {
                    LayerIndex = 3,
                    Attributes = new[] {"OWNER"},
                    JsonPropertyName = "owner",
                    CalculationCommand = new CalculateIntersectionCommand(whole, _logger)
                },
                new Criteria
                {
                    LayerIndex = 3,
                    Attributes = new[] {"ADMIN"},
                    JsonPropertyName = "admin",
                    CalculationCommand = new CalculateIntersectionCommand(whole, _logger)
                },
                new Criteria
                {
                    LayerIndex = 4,
                    Attributes = new[] {"STATE_NAME"},
                    JsonPropertyName = "state",
                    CalculationCommand = new CalculateIntersectionCommand(whole, _logger)
                }
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
                    IntersectionPart intersectionPart;
                    try
                    {
                        intersectionPart = criteria.GetIntersectionWith(feature);
                    }
                    catch (Exception ex)
                    {
                        return Json(new ResponseContainer(HttpStatusCode.InternalServerError, ex.Message));
                    }

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

            var response = new IntersectResult(searchResults);

#if !DEBUG
            _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Returning results {0}", searchResults.Count));
#endif

            return Json(new ResponseContainer<IntersectResult>(response));
        }
    }
}