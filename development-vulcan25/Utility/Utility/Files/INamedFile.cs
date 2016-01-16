using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulcan.Utility.Files
{
    public interface INamedFile
    {
        string Name
        {
            get;
            set;
        }

        string FilePath
        {
            get;
        }
    }
}
