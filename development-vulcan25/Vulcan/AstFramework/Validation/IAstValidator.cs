using System.Collections.Generic;
using AstFramework.Engine;
using Vulcan.Utility.Collections;

namespace AstFramework.Validation
{
    public interface IAstValidator
    {
       //// AstEngine AstEngine { get; set; }

        void Validate();

        void Reset();

        ObservableHashSet<VulcanValidationItem> ValidationItems { get; }

        HashSet<VulcanValidationItem> AllValidationItems { get; }
    }
}
