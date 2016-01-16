using System;

namespace Vulcan.Utility.Markup
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class FriendlyNameAttribute : Attribute
    {
        public string FriendlyName { get; private set; }

        public FriendlyNameAttribute(string friendlyName)
        {
            FriendlyName = friendlyName;
        }

        public override string ToString()
        {
            return FriendlyName;
        }
    }
}