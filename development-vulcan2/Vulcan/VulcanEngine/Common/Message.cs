using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using VulcanEngine.Properties;

namespace VulcanEngine.Common
{
    public class MessageEngine
    {
        private static Dictionary<string, MessageEngine> _MessageEnginesByName;
        private static MessageEngine _GlobalMessageEngine;

        private string _name;
        private bool _breakOnError;
        private Dictionary<Severity, List<VulcanMessage>> _errorDictionary;
        private bool _bStatusOn = true;
        private int _nPercent;

        static MessageEngine()
        {
            _MessageEnginesByName = new Dictionary<string, MessageEngine>();
            _GlobalMessageEngine = new MessageEngine("__GLOBAL");
            _MessageEnginesByName.Add("__GLOBAL", _GlobalMessageEngine);
        }

        public static Dictionary<string, MessageEngine> MessageEnginesByName
        {
            get { return _MessageEnginesByName; }
        }

        public static MessageEngine Global
        {
            get { return MessageEngine._GlobalMessageEngine; }
        }

        public static MessageEngine Create(string Name)
        {
            if (_MessageEnginesByName.ContainsKey(Name))
            {
                _GlobalMessageEngine.Trace(Severity.Error, Resources.ErrorMessageEngineNameDuplicate, Name);
            }
            MessageEngine CreatedMessageEngine = new MessageEngine(Name);
            _MessageEnginesByName.Add(Name, CreatedMessageEngine);

            return CreatedMessageEngine;
        }

        public static void Reset()
        {
            _MessageEnginesByName.Clear();
        }

        private MessageEngine(string name)
        {
            _name = name;
            _breakOnError = VulcanEngine.Properties.Settings.Default.BreakOnError;
            _errorDictionary = new Dictionary<Severity, List<VulcanMessage>>();
            foreach (Severity s in Enum.GetValues(typeof(Severity)))
            {
                _errorDictionary[s] = new List<VulcanMessage>();
            }
        }

        private void ClearStatus()
        {
            try
            {
                if (_bStatusOn)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                }
            }
            catch (System.Exception)
            {
                _bStatusOn = false;
            }
        }

        private void DisplayStatus()
        {
            try
            {
                if (_bStatusOn)
                {
                    ConsoleColor oldBackgroundColor = Console.BackgroundColor;

                    Console.BackgroundColor = ConsoleColor.Yellow;
                    int i = 0;
                    while (i < _nPercent * 2 / 5)
                    {
                        Console.Write(" ");
                        ++i;
                    }
                    Console.BackgroundColor = ConsoleColor.Blue;
                    while (i < 40)
                    {
                        Console.Write(" ");
                        ++i;
                    }

                    Console.BackgroundColor = oldBackgroundColor;
                    Console.Write(" {0}%", _nPercent);
                }
            }
            catch (System.Exception)
            {
                //If we get an IO exception it means we're running in an environment that doesn't support the Console.
                _bStatusOn = false;
            }
        }

        public void UpdateProgress(double percent)
        {
            try
            {
                if (_bStatusOn)
                {
                    _nPercent = (int)(percent * 100);
                    ClearStatus();
                    DisplayStatus();
                }
            }
            catch (System.Exception)
            {
                _bStatusOn = false;
            }
        }

        public void Trace(Severity severity, string message, params object[] formatParameters)
        {
            this.Trace(severity, null, message, formatParameters);
        }

        public void Trace(Severity severity, Exception exception, string message, params object[] formatParameters)
        {
            VulcanMessage ve = new VulcanMessage(severity, exception, message, formatParameters);
            this.Trace(ve);
        }

        public void Trace(VulcanMessage e)
        {
            ClearStatus();

            _errorDictionary[e.Severity].Add(e);
            if (e.Severity == Severity.Debug)
            {
                if (VulcanEngine.Properties.Settings.Default.ShowDebug)
                {
                    Console.WriteLine("{0}:{1}", Name, e.Message);
                }
            }
            else if (e.Severity == Severity.Alert)
            {
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("{0}:{1}", Name, e.Message);
                Console.ForegroundColor = color;
            }
            else if (e.Severity == Severity.Warning)
            {
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine("{0}:{1}", Name, e.Message);
                Console.ForegroundColor = color;
            }
            else if (e.Severity == Severity.Error)
            {
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("{0}:{1}", Name, e.Message);
                Console.ForegroundColor = color;
            }
            else
            {
                if (VulcanEngine.Properties.Settings.Default.ShowNotifications || e.Severity == Severity.Alert)
                {
                    Console.WriteLine("{0}:{1}", Name, e.Message);
                }
            }
            if ((e.Severity == Severity.Error) && (_breakOnError))
            {
                HandleException(e);
                ThrowException(e);
            }

            DisplayStatus();
        }

        [Conditional("DEBUG")]
        public void ThrowException(VulcanMessage e)
        {
            throw new Exception(e.Message);
        }

        public void HandleException(VulcanMessage e)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            //Console.Error.WriteLine("{0}:{1}", Name, e.Message);
            Console.ForegroundColor = color;
        }

        public string Name
        {
            get { return _name; }
        }
        public Dictionary<Severity, List<VulcanMessage>> Messages
        {
            get { return _errorDictionary; }
        }
        public int ErrorCount
        {
            get { return _errorDictionary[Severity.Error].Count; }
        }
        public int WarningCount
        {
            get { return _errorDictionary[Severity.Warning].Count; }
        }

        public static int AllEnginesErrorCount
        {
            get
            {
                int Count = 0;
                foreach (MessageEngine Engine in _MessageEnginesByName.Values)
                {
                    Count += Engine.ErrorCount;
                }
                return Count;
            }
        }

        public static int AllEnginesWarningCount
        {
            get
            {
                int Count = 0;
                foreach (MessageEngine Engine in _MessageEnginesByName.Values)
                {
                    Count += Engine.ErrorCount;
                }
                return Count;
            }
        }
    }

    public sealed class VulcanMessage
    {
        private string _message;
        private Exception _innerException;
        private Severity _severity;

        public VulcanMessage(Severity severity, Exception innerException, string message, params object[] formatParmeters)
        {
            _innerException = innerException;
            _severity = severity;

            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame[] stackFrames = st.GetFrames();
            MethodBase method = null;
            foreach (System.Diagnostics.StackFrame sf in stackFrames)
            {
                method = sf.GetMethod();

                if (method.ReflectedType.Equals(typeof(VulcanEngine.Common.MessageEngine)) || method.ReflectedType.Equals(typeof(VulcanEngine.Common.VulcanMessage)))
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            this._message = String.Format("{0}: {1}: {2}", severity, method.ReflectedType.Name, String.Format(message, formatParmeters));
        }

        public string Message
        {
            get { return this._message; }
        }

        public Exception InnerException
        {
            get { return this._innerException; }
        }

        public Severity Severity
        {
            get { return this._severity; }
        }

        public override string ToString()
        {
            return _message;
        }
    }
}
