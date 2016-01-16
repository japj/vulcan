using System.ComponentModel;

namespace Vulcan.Utility.ComponentModel
{
    public class VulcanPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public VulcanPropertyChangedEventArgs(string propertyName, object oldValue, object newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
