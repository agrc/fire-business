using System.Collections.Generic;
using System.Linq;
using fire_business_soe.Converters;
using Newtonsoft.Json;

namespace fire_business_soe.Models
{
    public class IntersectAttributes
    {
        public IntersectAttributes(IEnumerable<KeyValuePair<string, object>> values)
        {
            var pairs = values as IList<KeyValuePair<string, object>> ?? values.ToList();

            if (values != null && pairs.Any())
            {
                Attributes = pairs.Select(x => x.Value);
            }
        }

        public IEnumerable<object> Attributes { get; set; }

        [JsonConverter(typeof(ConvertMetersToAcresConverter))]
        public double Intersect { get; set; } 
    }
}