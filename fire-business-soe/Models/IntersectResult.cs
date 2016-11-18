using System.Collections.Generic;

namespace fire_business_soe.Models
{
    public class IntersectResult
    {
        public IntersectResult(Dictionary<string, IList<IntersectAttributes>> attributes)
        {
            Attributes = attributes;
        }

        public Dictionary<string, IList<IntersectAttributes>> Attributes { get; set; } 
    }
}