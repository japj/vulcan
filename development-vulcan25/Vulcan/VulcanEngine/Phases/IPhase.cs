using System;
using System.Collections.ObjectModel;
using VulcanEngine.IR;

namespace VulcanEngine.Phases
{
    public interface IPhase
    {
        string Name { get; }

        string WorkflowUniqueName { get; }

        Type InputIRType { get; }

        // Any IR that is passed to Execute may be changed in place.
        // Often Execute will change the passed IR instance and return the same instance back.
        IIR Execute(Collection<IIR> predecessorIRs);

        IIR Execute(IIR predecessorIR);
    }
}
