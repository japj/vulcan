using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.Common
{
    public static class PropertyManager
    {
        private static Dictionary<string, string> propertyDictionary = new Dictionary<string, string>();

        public static IDictionary<string, string> Properties
        {
            get
            {
                return propertyDictionary;
            }
        }
    }
}
