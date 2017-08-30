using System.Collections.Generic;

namespace fire_business_soe.Models
{
    public class Criteria
    {
        public readonly CriteriaFilter Filter;
        public int LayerIndex { get; set; }
        public IEnumerable<string> Attributes { get; set; }
        public string JsonPropertyName { get; set; }

        public Criteria(int layerIndex, IEnumerable<string> attributes, string jsonPropertyName, CriteriaFilter filter=null)
        {
            Filter = filter;
            LayerIndex = layerIndex;
            Attributes = attributes;
            JsonPropertyName = jsonPropertyName;
        }
    }
}