﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace EditorCore.WindWaker.Templates
{
    public class ItemJsonTemplate
    {
        public class Property
        {
            public string Name;

            public string Type;

            /// <summary> Used if Type is set to "enum" </summary>
            public string EnumType;

            /// <summary> Used if Type is set to "objectReference" </summary>
            public string ReferenceType;

            /// <summary> Used if Type is set to a fixed length type variable. </summary>
            public int Length;
        }

        [JsonProperty("FourCC")]
        public string FourCC;

        [JsonProperty("Properties")]
        public List<Property> Properties;
    }
}
