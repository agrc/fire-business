using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;
using fire_business_soe.Models;

namespace fire_business_soe.Commands
{
    public class GetValueAtIndexCommand
    {
        private readonly IEnumerable<IndexFieldMap> _indexes;
        private readonly IObject _row;

        public GetValueAtIndexCommand(IEnumerable<IndexFieldMap> indexes, IObject row)
        {
            _indexes = indexes;
            _row = row;
        }

        public bool IncludeShape { get; set; }

        public IEnumerable<KeyValuePair<string, object>> Execute()
        {
            var results = new Dictionary<string, object>();

            foreach (var map in _indexes)
            {
                if (map.Index < 0)
                {
                    results.Add(map.Field, null);
                    continue;
                }

                // ReSharper disable RedundantCast
                results.Add(map.Field, (object) _row.Value[map.Index]);
                // ReSharper restore RedundantCast
            }

            return results;
        }
    }
}