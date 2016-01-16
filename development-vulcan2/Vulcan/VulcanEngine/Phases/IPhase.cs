using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using VulcanEngine.IR;
using VulcanEngine.Kernel;

using VulcanEngine.Common;

namespace VulcanEngine.Phases
{
    public interface IPhase
    {
        string        Name          { get; }
        string        WorkflowUniqueName { get; }
        Type          InputIRType     { get; }

        // Any IR that is passed to Execute may be changed in place.
        // Often Execute will change the passed IR instance and return the same instance back.
        IIR Execute(List<IIR> predecessorIRList);
        IIR Execute(IIR predecessorIR);
    }
}
