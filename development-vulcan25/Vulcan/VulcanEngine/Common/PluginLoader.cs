using System;
using AstFramework;
using Vulcan.Utility.Files;
using VulcanEngine.Properties;

namespace VulcanEngine.Common
{
    public class PluginLoader<TPlugin, TPluginAttribute> : Vulcan.Utility.Plugin.PluginLoader<TPlugin, TPluginAttribute> where TPluginAttribute : Attribute
    {
        public PluginLoader(string pluginSubpath, int minAttributeCount, int maxAttributeCount) : base(minAttributeCount, maxAttributeCount)
        {
            if (pluginSubpath != null)
            {
                PluginFolder = PathManager.GetToolSubpath(pluginSubpath);
            }
        }

        protected override void PopulatePluginTypesLists(object[] attributeList, Type type)
        {
            // REVIEW: Taking first friendly name only if multiple friendly name attributes were specified on the phase.  Is this OK or should I raise and error/warning?
            if (attributeList != null && attributeList.Length > 0)
            {
                if (attributeList.Length > MaxAttributeCount || attributeList.Length < MinAttributeCount)
                {
                    MessageEngine.Trace(Severity.Warning, Resources.WarningMultiplePhaseFriendlyNames, type.AssemblyQualifiedName);
                }

                foreach (TPluginAttribute attribute in attributeList)
                {
                    string attributeString = attribute.ToString();

                    if (PluginTypesByAttribute.ContainsKey(attribute))
                    {
                        MessageEngine.Trace(Severity.Error, Resources.ErrorDuplicatePhaseFriendlyNameFound, attributeString);
                    }

                    PluginTypesByAttribute.Add(attribute, type);

                    if (PluginTypesByAttributeString.ContainsKey(attributeString))
                    {
                        //// Notice that this check is not a strict duplicate of the one above, since the string conversion could create dupes depending on attribute
                        MessageEngine.Trace(Severity.Error, Resources.ErrorDuplicatePhaseFriendlyNameFound, attributeString);
                    }

                    PluginTypesByAttributeString.Add(attributeString, type);
                }
            }
        }
    }
}
