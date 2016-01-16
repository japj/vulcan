using System;

namespace AstFramework.Markup
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EnumFriendlyValueAttribute : Attribute
    {
        public string FriendlyValue { get; private set; }

        public EnumFriendlyValueAttribute(string friendlyValue)
            : base()
        {
            FriendlyValue = friendlyValue;
        }

        public override string ToString()
        {
            return FriendlyValue;
        }
    }
}