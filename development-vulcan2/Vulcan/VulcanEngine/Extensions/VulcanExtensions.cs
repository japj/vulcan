using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace VulcanEngine.Common
{
    /// <summary>
    /// Vulcan wrapper for List. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// </remarks>
    public class VulcanCollection<T> : List<T>
    {
        public VulcanCollection() 
            : base()
        {
        }

        public VulcanCollection(List<T> list)
            : base(list)
        {
        }

        public VulcanCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }
    }

    public class VulcanNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void VulcanOnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public interface IVulcanEditableObject : IEditableObject { }

    public class VulcanDescriptionAttribute : DescriptionAttribute
    {
        public VulcanDescriptionAttribute()
            : base()
        {
        }

        public VulcanDescriptionAttribute(string description)
            : base(description)
        {
        }
    }

    public class VulcanCategoryAttribute : CategoryAttribute
    {
        public VulcanCategoryAttribute()
            : base()
        {
        }

        public VulcanCategoryAttribute(string category)
            : base(category)
        {
        }
    }

    public class VulcanDefaultValueAttribute : DefaultValueAttribute
    {
        public VulcanDefaultValueAttribute(bool value) : base(value) { }
        public VulcanDefaultValueAttribute(byte value) : base(value) { }
        public VulcanDefaultValueAttribute(char value) : base(value) { }
        public VulcanDefaultValueAttribute(double value) : base(value) { }
        public VulcanDefaultValueAttribute(float value) : base(value) { }
        public VulcanDefaultValueAttribute(int value) : base(value) { }
        public VulcanDefaultValueAttribute(long value) : base(value) { }
        public VulcanDefaultValueAttribute(object value) : base(value) { }
        public VulcanDefaultValueAttribute(short value) : base(value) { }
        public VulcanDefaultValueAttribute(string value) : base(value) { }
        public VulcanDefaultValueAttribute(Type type, string value) : base(type, value) { }
    }
}
