using System;
using System.Xml.Linq;

namespace Vulcan.Utility.Xml
{
    public class XObjectMapping
    {
        public XObject XObject { get; set; }

        public string PropertyName { get; set; }

        public bool IsXAttribute
        {
            get { return XObject is XAttribute; }
        }

        public bool IsXElement
        {
            get { return XObject is XElement; }
        }

        public XObjectMapping(XObject xmlObject, string propertyName)
        {
            XObject = xmlObject;
            PropertyName = propertyName;
        }
    }
}
