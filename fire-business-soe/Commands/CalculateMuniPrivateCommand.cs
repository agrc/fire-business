using System;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SOESupport;
using fire_business_soe.Models;

namespace fire_business_soe.Commands
{
    public class CalculateMuniPrivateCommand : ICalculateIntersectionCommand
    {
        private const int MessageCode = 1337;
        private readonly ServerLogger _logger;
        private readonly ITopologicalOperator4 _whole;
        // line over polygon = 1D
        // polygon over polygon = 2D
        public CalculateMuniPrivateCommand(ITopologicalOperator4 whole, ServerLogger logger)
        {
            _whole = whole;
            _logger = logger;
        }

        public FeatureClassIndexMap LandOwnership { get; set; }

        public IntersectionPart Execute(IFeature other)
        {
            const string methodName = "CalculateMuniPrivateCommand";

            var municipleIntersection = new CalculateIntersectionCommand(_whole, _logger).Execute(other);

            if (municipleIntersection.Intersection == null)
            {
                return new IntersectionPart();
            }

            var wholeMuni = (ITopologicalOperator4) municipleIntersection.Intersection;
            var privateFilter = new SpatialFilter
            {
                Geometry = municipleIntersection.Intersection,
                SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects,
                WhereClause = "OWNER = 'Private'"
            };

            var cursor = LandOwnership.FeatureClass.Search(privateFilter, true);
            IFeature privateLand;
            IGeometry privateGeometry = null;
            while ((privateLand = cursor.NextFeature()) != null)
            {
                var intersection = wholeMuni.Intersect(privateLand.ShapeCopy, esriGeometryDimension.esriGeometry2Dimension);

                if (privateGeometry == null)
                {
                    privateGeometry = intersection;
                }
                else
                {
                    privateGeometry = ((ITopologicalOperator4) privateGeometry).Union(intersection);
                }
            }

            if (privateGeometry == null)
            {
                return new IntersectionPart();
            }

            var part = new IntersectionPart();
            var area = (IArea) privateGeometry;
#if !DEBUG
            _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Area: {0}", area.Area));
#endif
            part.Size = Math.Abs(area.Area);
            part.Intersection = privateGeometry;

            return part;
        }
    }
}