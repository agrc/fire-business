using System;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SOESupport;
using fire_business_soe.Models;

namespace fire_business_soe.Commands
{
    public class CalculateIntersectionCommand : ICalculateIntersectionCommand
    {
        private const int MessageCode = 1337;
        private readonly ServerLogger _logger;
        private readonly ITopologicalOperator4 _whole;
        // line over polygon = 1D
        // polygon over polygon = 2D
        public CalculateIntersectionCommand(ITopologicalOperator4 whole, ServerLogger logger)
        {
            _whole = whole;
            _logger = logger;
        }

        public Criteria Criteria { get; set; }

        public IntersectionPart Execute(IFeature other)
        {
            const string methodName = "CalculateIntersectionCommand";

            IGeometry intersection;
            var part = new IntersectionPart();
            switch (other.ShapeCopy.GeometryType)
            {
                case esriGeometryType.esriGeometryPolygon:
                    intersection = _whole.Intersect(other.ShapeCopy, esriGeometryDimension.esriGeometry2Dimension);

                    var area = (IArea) intersection;
                    part.Size = Math.Abs(area.Area);
                    part.Intersection = intersection;
#if !DEBUG
                    _logger.LogMessage(ServerLogger.msgType.infoStandard, methodName, MessageCode, string.Format("Area: {0}", area.Area));
#endif
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    intersection = _whole.Intersect(other.ShapeCopy, esriGeometryDimension.esriGeometry1Dimension);

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