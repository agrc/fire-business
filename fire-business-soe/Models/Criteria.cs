using System.Collections.Generic;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Geodatabase;
using fire_business_soe.Commands;

namespace fire_business_soe.Models
{
    public class Criteria
    {
        public ICalculateIntersectionCommand CalculationCommand { get; set; }
        public int LayerIndex { get; set; }
        public IEnumerable<string> Attributes { get; set; }
        public string JsonPropertyName { get; set; }
        public Collection<FeatureClassIndexMap> FeatureClassIndexMap { get; set; }

        public IntersectionPart GetIntersectionWith(IFeature other)
        {
            return CalculationCommand.Execute(other);
        }
    }
}