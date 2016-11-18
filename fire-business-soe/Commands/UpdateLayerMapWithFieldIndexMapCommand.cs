using System.Collections.ObjectModel;
using fire_business_soe.Models;

namespace fire_business_soe.Commands
{
    public class UpdateLayerMapWithFieldIndexMapCommand
    {
        private readonly Collection<FeatureClassIndexMap> _map;

        public UpdateLayerMapWithFieldIndexMapCommand(Collection<FeatureClassIndexMap> map)
        {
            _map = map;
        }

        public void Execute()
        {
            foreach (var item in _map)
            {
                item.FieldMap = new FindIndexByFieldNameCommand(item.FeatureClass).Execute();
            }
        }
    }
}