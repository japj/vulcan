using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.Common
{
    internal class SimpleCommandLineParser
    {
        #region Private Storage
        private string _switchCharacters;
        private Dictionary<string, List<string>> _switchArgs = new Dictionary<string, List<string>>();
        private List<string> _noswitchArgs = new List<string>();
        #endregion // Private Storage

        #region Public Accessor Properties

        public IList<string> NoSwitchArguments
        {
            get { return _noswitchArgs; }
        }

        public IList<string> this[string switchStr]
        {
            get
            {
                if (_switchArgs.ContainsKey(switchStr))
                {
                    return _switchArgs[switchStr];
                }

                return null;
            }
        }

        #endregion //Public Accessor Properties

        public SimpleCommandLineParser(string switchCharacters, string[] args)
        {
            _switchCharacters = switchCharacters;

            Parse(args);
        }

        private void Parse(string[] args)
        {
            bool bSwitch = false;
            string strSwitch = null;

            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];
                if (arg.Length > 0 && _switchCharacters.Contains(arg[0]))
                {
                    bSwitch = true;
                    strSwitch = arg.Substring(1);
                    _switchArgs.Add(strSwitch, new List<string>());
                }
                else
                {
                    if (bSwitch)
                    {
                        _switchArgs[strSwitch].Add(arg);
                    }
                    else
                    {
                        _noswitchArgs.Add(arg);
                    }
                }
            }
        }
    }
}
