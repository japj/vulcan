using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Vulcan.Utility.Common
{
    public class CustomAttributeProvider
    {
        private static CustomAttributeProvider _customAttributeProvider;

        private CustomAttributeProvider() 
        {
            _cache = new Dictionary<MemberInfo, Dictionary<Type, Dictionary<bool, object>>>();
        }

        public static CustomAttributeProvider Global
        {
            get
            {
                if (_customAttributeProvider == null)
                {
                    _customAttributeProvider = new CustomAttributeProvider();
                }

                return _customAttributeProvider;
            }
        }

        private readonly Dictionary<MemberInfo, Dictionary<Type, Dictionary<bool, object>>> _cache;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Advanced method provided for advanced developers.")]
        public Collection<TCustomAttribute> GetAttribute<TCustomAttribute>(MemberInfo memberInfo, bool checkAncestors)
        {
            Type attributeType = typeof(TCustomAttribute);
            if (!_cache.ContainsKey(memberInfo))
            {
                _cache.Add(memberInfo, new Dictionary<Type, Dictionary<bool, object>>());
            }

            var subcache = _cache[memberInfo];
            if (!subcache.ContainsKey(attributeType))
            {
                subcache.Add(attributeType, new Dictionary<bool, object>());
            }

            var lastcache = subcache[attributeType];
            if (!lastcache.ContainsKey(checkAncestors))
            {
                var attributes = memberInfo.GetCustomAttributes(attributeType, checkAncestors) as TCustomAttribute[];
                lastcache.Add(checkAncestors, new Collection<TCustomAttribute>(attributes));
            }

            return lastcache[checkAncestors] as Collection<TCustomAttribute>;
        }
    }
}