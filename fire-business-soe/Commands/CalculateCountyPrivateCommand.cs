using System;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SOESupport;
using fire_business_soe.Models;

namespace fire_business_soe.Commands
{
    public class CalculateCountyPrivateCommand : ICalculateIntersectionCommand
    {
        // areas people live, but not in a city, e.g. ranch, house, etc.
        // total unincorporated county private acreage
        private const int MessageCode = 1337;
        private readonly ServerLogger _logger;
        private readonly ITopologicalOperator4 _whole;
        // line over polygon = 1D
        // polygon over polygon = 2D
        public CalculateCountyPrivateCommand(ITopologicalOperator4 whole, ServerLogger logger)
        {
            _whole = whole;
            _logger = logger;
        }

        public FeatureClassIndexMap LandOwnership { get; set; }
        public FeatureClassIndexMap Municipalities { get; set; }

        public IntersectionPart Execute(IFeature other)
        {
            const string methodName = "CalculateCountyPrivateCommand";

            var countyIntersection = new CalculateIntersectionCommand(_whole, _logger).Execute(other);

            //  out of utah
            if (countyIntersection.Intersection == null)
            {
                return new IntersectionPart();
            }

            var countyPart = (ITopologicalOperator4) countyIntersection.Intersection;
            var privateFilter = new SpatialFilter
            {
                Geometry = countyIntersection.Intersection,
                SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects,
                WhereClause = "OWNER = 'Private'"
            };
            var muniFilter = new SpatialFilter
            {
                Geometry = countyIntersection.Intersection,
                SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
            };

            var privateGeometry = UnionItemsFor(LandOwnership.FeatureClass, privateFilter, countyPart);
            var municipalGeometry = UnionItemsFor(Municipalities.FeatureClass, muniFilter, countyPart);

            // no private or municipal land
            if (privateGeometry == null && municipalGeometry == null)
            {
                return new IntersectionPart();
            }

            // no municipality - return private
            if (municipalGeometry == null)
            {
                var privateArea = (IArea) privateGeometry;
#if !DEBUG
                _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Area: {0}", privateArea.Area));
#endif
                return new IntersectionPart
                {
                    Size = Math.Abs(privateArea.Area),
                    Intersection = privateGeometry
                };
            }

            // no private - return 0 because it's places people live not in a city
            if (privateGeometry == null)
            {
                return new IntersectionPart();
            }

            // there are both private and muni
            var privatePart = (ITopologicalOperator4) privateGeometry;
            var intersection = privatePart.Difference(municipalGeometry);

            var part = new IntersectionPart();
            var area = (IArea) intersection;
#if !DEBUG
            _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Area: {0}", area.Area));
#endif
            part.Size = Math.Abs(area.Area);
            part.Intersection = intersection;

            return part;
        }

        private static IGeometry UnionItemsFor(IFeatureClass featureClass, SpatialFilter privateFilter, ITopologicalOperator4 countyPart)
        {
            var cursor = featureClass.Search(privateFilter, true);
            IFeature privateLand;
            IGeometry privateGeometry = null;
            while ((privateLand = cursor.NextFeature()) != null)
            {
                var intersect = countyPart.Intersect(privateLand.ShapeCopy, esriGeometryDimension.esriGeometry2Dimension);

                if (privateGeometry == null)
                {
                    privateGeometry = intersect;
                }
                else
                {
                    privateGeometry = ((ITopologicalOperator4) privateGeometry).Union(intersect);
                }
            }
            return privateGeometry;
        }
    }
}