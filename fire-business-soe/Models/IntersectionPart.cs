using ESRI.ArcGIS.Geometry;

namespace fire_business_soe.Models
{
    public class IntersectionPart
    {
        public IGeometry Intersection { get; set; }
        public double Size { get; set; }
        public string Message { get; set; }
    }
}