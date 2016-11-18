using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;
using fire_business_soe.Models;

namespace fire_business_soe.Commands
{
    public class FindIndexByFieldNameCommand
    {
        private readonly IFields _fields;
        private readonly string[] _fieldsToMap;
        private readonly Dictionary<string, IndexFieldMap> _propertyValueIndexMap;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FindIndexByFieldNameCommand" /> class.
        /// </summary>
        /// <param name="layer"> The layer. </param>
        /// <param name="fieldsToMap">The fields to map to an index number</param>
        public FindIndexByFieldNameCommand(IFeatureClass layer, string[] fieldsToMap)
        {
            _fieldsToMap = fieldsToMap;
            _fields = layer.Fields;
            _propertyValueIndexMap = new Dictionary<string, IndexFieldMap>();
        }

        public FindIndexByFieldNameCommand(IFeatureClass layer)
        {
            _fields = layer.Fields;
            _propertyValueIndexMap = new Dictionary<string, IndexFieldMap>();
        }

        /// <summary>
        ///     code to execute when command is run. Iterates over every month and finds the index for the field in teh feature
        ///     class
        /// </summary>
        public Dictionary<string, IndexFieldMap> Execute()
        {
            var iterate = _fieldsToMap;

            if (_fieldsToMap == null)
            {
                iterate = new string[_fields.FieldCount];
                for (var i = 0; i < _fields.FieldCount; i++)
                {
                    iterate[i] = _fields.Field[i].Name;
                }
            }

            foreach (var field in iterate)
            {
                _propertyValueIndexMap.Add(field, new IndexFieldMap(GetIndexForField(field, _fields), field));
            }

            return _propertyValueIndexMap;
        }

        /// <summary>
        ///     Gets the index for field.
        /// </summary>
        /// <param name="attributeName"> The attribute name. </param>
        /// <param name="fields"> The fields. </param>
        /// <returns> </returns>
        private static int GetIndexForField(string attributeName, IFields fields)
        {
            var findField = fields.FindField(attributeName.Trim());

            return findField < 0 ? fields.FindFieldByAliasName(attributeName.Trim()) : findField;
        }
    }
}