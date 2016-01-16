using System;
using System.Collections.Generic;
using System.Reflection;
using Vulcan.Utility.Collections;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;

namespace AstLowerer
{
    public static class Utility
    {
        public static void Replace(AstTransformationNode item, IEnumerable<AstTransformationNode> replacements)
        {
            var transformations = GetParentTransformationCollection(item);
            if (transformations != null)
            {
                transformations.Replace(item, replacements);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Static enforcement of AstTransformationNode ensures that the Parent node will have a Transformations collection.")]
        public static VulcanCollection<AstTransformationNode> GetParentTransformationCollection(AstTransformationNode item)
        {
            var parent = item.ParentItem;
            if (parent != null)
            {
                PropertyInfo propertyInfo = parent.GetType().GetProperty("Transformations");
                if (propertyInfo != null)
                {
                    return propertyInfo.GetValue(parent, null) as VulcanCollection<AstTransformationNode>;
                }
            }

            return null;
        }

        public static string NameCleaner(string name)
        {
            return name.Replace(".", "_").Replace(@"\", "_");
        }
        
        // BUGBUG: Why do we have two name cleaner and uniqifiers?
        public static string NameCleanerAndUniqifier(string name)
        {
             return NameCleaner(name) + "_" + Guid.NewGuid().ToString("N");
        }
    }
}
