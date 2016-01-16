using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Vulcan.Utility.Plugin
{
    public class PluginLoader<TPlugin, TPluginAttribute> where TPluginAttribute : Attribute
    {
        public Dictionary<TPluginAttribute, Type> PluginTypesByAttribute
        {
            get; 
            private set;
        }

        public Dictionary<string, Type> PluginTypesByAttributeString
        {
            get;
            private set;
        }

        public Dictionary<string, Type> PluginTypesByFullName
        {
            get;
            private set;
        }

        protected int MinAttributeCount
        {
            get; 
            private set;
        }

        protected int MaxAttributeCount
        {
            get; 
            private set;
        }

        protected string PluginFolder
        {
            get; 
            set;
        }

        public PluginLoader(int minAttributeCount, int maxAttributeCount)
        {
            PluginTypesByAttribute = new Dictionary<TPluginAttribute, Type>();
            PluginTypesByAttributeString = new Dictionary<string, Type>();
            PluginTypesByFullName = new Dictionary<string, Type>();

            MinAttributeCount = minAttributeCount;
            MaxAttributeCount = maxAttributeCount;
        }

        /// <summary>
        /// Loads all plugins using the current assembly
        /// </summary>
        public void LoadPlugins()
        {
            LoadPlugins(Assembly.GetCallingAssembly());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom", Justification = "Required to get correct plugin loading behavior.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Plugin loader needs to be vary graceful in failure conditions and there is no risk of corrupted state.")]
        public void LoadPlugins(Assembly callingAssembly)
        {
            var assemblies = new List<Assembly> { callingAssembly };

            if (PluginFolder != null && Directory.Exists(PluginFolder))
            {
                foreach (string s in Directory.GetFiles(PluginFolder, "*.dll"))
                {
                    try
                    {
                        // REVIEW: Should we use Assembly.LoadFrom instead?  Do we want these in the current context?  I think so for now, but we may want to isolate in future.
                        Assembly assembly = Assembly.LoadFrom(s);
                        if (!assemblies.Contains(assembly))
                        {
                            assemblies.Add(assembly);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            foreach (Assembly a in assemblies)
            {
                // REVIEW: Currently using exported types only, though we have the capability to find other plugins.  Any compelling scenarios to implement the latter?
                foreach (Type t in a.GetExportedTypes())
                {
                    if (typeof(TPlugin).IsAssignableFrom(t) && !typeof(TPlugin).Equals(t) && !t.IsAbstract)
                    {
                        object[] attributeList = t.GetCustomAttributes(typeof(TPluginAttribute), false);

                        PopulatePluginTypesLists(attributeList, t);

                        // No need to check if dictionary already contains the key, since key is assembly qualified type name
                        PluginTypesByFullName.Add(t.AssemblyQualifiedName, t);
                    }
                }
            }
        }

        protected virtual void PopulatePluginTypesLists(object[] attributeList, Type type)
        {
            // REVIEW: Taking first friendly name only if multiple friendly name attributes were specified on the phase.  Is this OK or should I raise and error/warning?
            if (attributeList != null && attributeList.Length > 0)
            {
                foreach (TPluginAttribute attribute in attributeList)
                {
                    PluginTypesByAttribute.Add(attribute, type);
                }
            }
        }
    }
}
