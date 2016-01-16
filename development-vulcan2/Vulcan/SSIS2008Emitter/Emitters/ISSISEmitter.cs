using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.Emitters;

namespace Ssis2008Emitter.Emitters
{
    public interface ISSISEmitter  
    {
        SSISEmitterContext Emit();
    }
}
