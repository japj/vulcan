using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using VulcanEngine.Properties;

namespace VulcanEngine.Common
{
    public class PluginLoader<PluginType,PluginAttributeType> where PluginAttributeType: Attribute
    {
        private Dictionary<PluginAttributeType, Type> _pluginTypesByAttribute;
        public Dictionary<PluginAttributeType, Type> PluginTypesByAttribute
        {
            get { return _pluginTypesByAttribute; }
        }

        private Dictionary<string, Type> _pluginTypesByAttributeString;
        public Dictionary<string, Type> PluginTypesByAttributeString
        {
            get { return _pluginTypesByAttributeString; }
        }

        private Dictionary<string, Type> _pluginTypesByFullName;
        public Dictionary<string, Type> PluginTypesByFullName
        {
            get { return _pluginTypesByFullName; }
        }
        protected MessageEngine _message;
        protected string _pluginFolder;

        private int _minAttributeCount;
        private int _maxAttributeCount;

        public PluginLoader(string pluginSubPath, int MinAttributeCount, int MaxAttributeCount)
        {
            this._pluginTypesByAttribute = new Dictionary<PluginAttributeType, Type>();
            this._pluginTypesByAttributeString = new Dictionary<string, Type>();
            this._pluginTypesByFullName = new Dictionary<string, Type>();

            this._minAttributeCount = MinAttributeCount;
            this._maxAttributeCount = MaxAttributeCount;

            this._message = MessageEngine.Global;

            if (pluginSubPath != null)
            {
                this._pluginFolder = PathManager.GetToolSubpath(pluginSubPath);
            }

            LoadPlugins(Assembly.GetCallingAssembly());
        }

        void LoadPlugins(Assembly callingAssembly)
        {
            List<Assembly> assemblies = new List<Assembly>();
            assemblies.Add(callingAssembly);

            if (this._pluginFolder != null && Directory.Exists(this._pluginFolder))
            {
                foreach (string s in Directory.GetFiles(this._pluginFolder, "*.dll"))
                {
                    try
                    {
                        // REVIEW: Should we use Assembly.LoadFrom instead?  Do we want these in the current context?  I think so for now, but we may want to isolate in future.
                        assemblies.Add(Assembly.LoadFile(s));
                    }
                    catch { }
                }
            }

            foreach (Assembly a in assemblies)
            {
                // REVIEW: Currently using exported types only, though we have the capability to find other plugins.  Any compelling scenarios to implement the latter?
                foreach (Type t in a.GetExportedTypes())
                {
                    // TODO: What's the best set of conditionals to use here?
                    if (typeof(PluginType).IsAssignableFrom(t) && !typeof(PluginType).Equals(t) && !t.IsAbstract)
                    {
                        object[] attributeList = t.GetCustomAttributes(typeof(PluginAttributeType), false);

                        // REVIEW: Taking first friendly name only if multiple friendly name attributes were specified on the phase.  Is this OK or should I raise and error/warning?
                        if (attributeList != null && attributeList.Length > 0)
                        {
                            if (attributeList.Length > this._maxAttributeCount || attributeList.Length < this._minAttributeCount)
                            {
                                // TODO: Patch this up with a more generic warning/error
                                _message.Trace(Severity.Warning, Resources.WarningMultiplePhaseFriendlyNames, t.AssemblyQualifiedName);
                            }

                            foreach (PluginAttributeType attribute in attributeList)
                            {
                                string attributeString = attribute.ToString();
                                if (_pluginTypesByAttribute.ContainsKey(attribute))
                                {
                                    // TODO: Patch this up with a more generic warning/error
                                    _message.Trace(Severity.Error, Resources.ErrorDuplicatePhaseFriendlyNameFound, attributeString);
                                }
                                _pluginTypesByAttribute.Add(attribute, t);

                                if (_pluginTypesByAttributeString.ContainsKey(attributeString))
                                {
                                    // Notice that this check is not a strict duplicate of the one above, since the string conversion could create dupes depending on attribute
                                    // TODO: Patch this up with a more generic warning/error
                                    _message.Trace(Severity.Error, Resources.ErrorDuplicatePhaseFriendlyNameFound, attributeString);
                                }
                                _pluginTypesByAttributeString.Add(attributeString, t);
                            }
                        }

                        // No need to check if dictionary already contains the key, since key is assembly qualified type name
                        _pluginTypesByFullName.Add(t.AssemblyQualifiedName, t);
                    }
                }
            }

        }
    }
}
