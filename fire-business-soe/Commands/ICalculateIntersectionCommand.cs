using ESRI.ArcGIS.Geodatabase;
using fire_business_soe.Models;

namespace fire_business_soe.Commands
{
    public interface ICalculateIntersectionCommand
    {
        IntersectionPart Execute(IFeature other);
    }
}