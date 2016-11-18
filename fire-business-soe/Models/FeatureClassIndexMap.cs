﻿using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;

namespace fire_business_soe.Models
{
    public class FeatureClassIndexMap
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FeatureClassIndexMap" />  class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="layerName">The layer name to find</param>
        /// <param name="featureClass">The feature class.</param>
        public FeatureClassIndexMap(int index, string layerName, IFeatureClass featureClass)
        {
            Index = index;
            LayerName = layerName;
            FeatureClass = featureClass;
        }

        /// <summary>
        ///     Gets or sets the index of the layer in the map document.
        /// </summary>
        /// <value>
        ///     The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        ///     Gets or sets the name of the layer in the mxd.
        /// </summary>
        /// <value>
        ///     The name of the layer in the mxd.
        /// </value>
        public string LayerName { get; set; }

        /// <summary>
        ///     Gets or sets the feature class.
        /// </summary>
        /// <value>
        ///     The feature class reference of the layer in the mxd.
        /// </value>
        public IFeatureClass FeatureClass { get; set; }

        /// <summary>
        ///     Gets or sets the field map.
        /// </summary>
        /// <value>
        ///     The field map. Where the key is the field, and the index field map
        ///     is the field and index
        /// </value>
        public Dictionary<string, IndexFieldMap> FieldMap { get; set; }
    }
}