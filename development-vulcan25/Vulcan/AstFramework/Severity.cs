using System.ComponentModel;
using AstFramework.Markup;

namespace AstFramework
{
    [TypeConverter(typeof(EnumFriendlyValueConverter<Severity>))]
    public enum Severity
    {
        [Description("error")]
        Error,
        
        [Description("warning")]
        Warning,

        [Description("alert")]
        Alert,
        
        [Description("notification")]
        Notification,
        
        [Description("debug")]
        Debug
    }
}
