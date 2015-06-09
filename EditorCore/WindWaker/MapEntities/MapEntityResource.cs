﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EditorCore.WindWaker.MapEntities
{
    public class MapEntityResource : BaseFileResource
    {
        public MapEntityResource(string fileName, string folderName, ZArchive parentArchive) : base (fileName, folderName, parentArchive)
        {

        }

        public override string ToString()
        {
            return string.Format("[MapEntityResource] {0}", base.ToString());
        }
    }

    public enum PropertyType
    {
        None,
        Byte,
        Short,
        Int32,
        Float,
        String,
        Vector2,
        Vector3,
        Enum,
        ObjectReference,
    }

    public class MapEntityObject
    {
        public class Property
        {
            public string Name { get; private set; }
            public object Value;
            public PropertyType Type { get; private set; }

            public Property(string name, PropertyType type, object defaultValue = null)
            {
                Name = name;
                Type = type;
                Value = defaultValue;
            }
        }

        public string FourCC { get; private set; }
        public BindingList<Property> Properties;

        public MapEntityObject(string fourCC)
        {
            FourCC = fourCC;
            Properties = new BindingList<Property>();
        }

        public Property this[string val]
        {
            get 
            {
                Property prop = null;
                for (int i = 0; i < Properties.Count; i++)
                {
                    if (string.Compare(val, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        prop = Properties[i];
                        break;
                    }
                }

                return prop;
            }
            set
            {
                Property prop = null;
                for (int i = 0; i < Properties.Count; i++)
                {
                    if (string.Compare(val, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        prop = Properties[i];
                        break;
                    }
                }

                if (prop != null)
                    prop.Value = value;
                else
                    Console.WriteLine("Unsupported property {0}", val);
            }
        }

    }
}
