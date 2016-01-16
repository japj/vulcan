using System;

namespace AstFramework
{
    public class BimlFileTextChangedEventArgs : EventArgs
    {
        public BimlFileTextChangedSource BimlFileTextChangedSource { get; set; }

        public BimlFileTextChangedEventArgs(BimlFileTextChangedSource bimlFileTextChangedSource)
        {
            BimlFileTextChangedSource = bimlFileTextChangedSource;
        }
    }
}
