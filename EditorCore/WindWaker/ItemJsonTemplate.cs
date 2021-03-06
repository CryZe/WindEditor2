﻿using Newtonsoft.Json;
using System.Collections.Generic;
using WEditor.Maps;

namespace WEditor.WindWaker.Maps
{
    [System.Obsolete]
    public class ItemJsonTemplate
    {
        [System.Obsolete]
        public class Property
        {
            public string Name;

            public string Type;

            /// <summary> Used if Type is set to "enum" </summary>
            public string EnumType;

            /// <summary> Used if Type is set to "objectReference" </summary>
            public string ReferenceType;

            /// <summary> Used if <see cref="FieldType"/> is set to a type that supports a fixed length variable. </summary>
            public int Length;

            /// <summary> Used if ReferenceType is set to "FourCC" </summary>
            public string ReferenceFourCCType;

            public override string ToString()
            {
                return string.Format("{0} [{1}]", Name, Type);
            }
        }

        [JsonProperty("FourCC")]
        public string FourCC;

        [JsonProperty("Properties")]
        public List<Property> Properties;

        /// <summary>Only used if this references another template instead of defining a unique set of properties. </summary>
        [JsonProperty("Template")][System.Obsolete]
        public string Template;

        public override string ToString()
        {
            return string.Format("Template: {0} Property Count: {1}", FourCC, Properties.Count);
        }
    }
}
