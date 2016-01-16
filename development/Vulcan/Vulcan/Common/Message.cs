/*
 * Microsoft Public License (Ms-PL)
 * 
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 * A "contribution" is the original software, or any additions or changes to the software.
 * A "contributor" is any person that distributes its contribution under this license.
 * "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
 */

using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Vulcan.Common
{

    public enum Severity
    {
        Error,
        Warning,
        Alert,
        Notification,
        Debug
    }

    public class MessageEngine
    {
        private string _name;
        private bool _breakOnError;
        private Dictionary<Severity, List<VulcanMessage>> _errorDictionary;
        public MessageEngine(string name)
        {
            _name = name;
            _breakOnError = Vulcan.Properties.Settings.Default.BreakOnError;
            _errorDictionary = new Dictionary<Severity, List<VulcanMessage>>();
            foreach (Severity s in Enum.GetValues(typeof(Severity)))
            {
                _errorDictionary[s] = new List<VulcanMessage>();
            }
        }

        public void Trace(VulcanMessage e)
        {
            _errorDictionary[e.Severity].Add(e);
            if (e.Severity == Severity.Debug)
            {
                if (Vulcan.Properties.Settings.Default.ShowDebug)
                {
                    Console.WriteLine("{0}:{1}",Name,e.Message);
                }
            }
            else if (e.Severity == Severity.Warning || e.Severity == Severity.Error)
            {
                Console.Error.WriteLine("{0}:{1}", Name, e.Message);
            }
            else
            {
                if (Vulcan.Properties.Settings.Default.ShowNotifications || e.Severity == Severity.Alert)
                {
                    Console.WriteLine("{0}:{1}", Name, e.Message);
                }
            }
            if ((e.Severity == Severity.Error) && (_breakOnError))
            {
                throw new Exception(e.Message);
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
        public Dictionary<Severity, List<VulcanMessage>> Messages
        {
            get
            {
                return _errorDictionary;
            }
        }
        public int ErrorCount
        {
            get
            {
                return _errorDictionary[Severity.Error].Count;
            }
        }
        public int WarningCount
        {
            get
            {
                return _errorDictionary[Severity.Warning].Count;
            }
        }
    }

    public static class Message
    {

        private static Dictionary<string, MessageEngine> engineDictionary;
        private static Stack<MessageEngine> engineStack;

        static Message()
        {
            engineStack = new Stack<MessageEngine>();
            engineStack.Push(new MessageEngine(Properties.Resources.DefaultMessageEngine));
            
            engineDictionary = new Dictionary<string, MessageEngine>();
            engineDictionary[Properties.Resources.DefaultMessageEngine] = engineStack.Peek();
        }

        public static void PushMessageEngine(string engineName)
        {
            lock (engineStack)
            {
                engineDictionary.Add(engineName, new MessageEngine(engineName));
                engineStack.Push(engineDictionary[engineName]);
            }
        }

        public static MessageEngine PopMessageEngine()
        {
            lock (engineStack)
            {
                if (engineStack.Count > 1)
                {
                    return engineStack.Pop();
                }
                else
                {
                    return engineStack.Peek();
                }
            }
        }

        public static void Trace(Severity severity, Exception exception, string message, params object[] formatParameters)
        {
            VulcanMessage ve = new VulcanMessage(severity, exception, message, formatParameters);
            lock (engineStack)
            {
                engineStack.Peek().Trace(ve);
            }
        }

        public static void Trace(Severity severity, string message, params object[] formatParameters)
        {
            Trace(severity, null, message, formatParameters);
        }

        public static int ErrorCount
        {
            get
            {
                lock (engineStack)
                {
                    return CurrentMessageEngine.ErrorCount;
                }
            }
        }

        public static int WarningCount
        {
            get
            {
                lock (engineStack)
                {
                    return CurrentMessageEngine.WarningCount;
                }
            }
        }

        public static MessageEngine CurrentMessageEngine
        {
            get
            {
                lock (engineStack)
                {
                    return engineStack.Peek();
                }
            }
        }

        public static Dictionary<string, MessageEngine> MessageEngines
        {
            get
            {
                return engineDictionary;
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

                if (method.ReflectedType.Equals(typeof(Vulcan.Common.Message)) || method.ReflectedType.Equals(typeof(Vulcan.Common.VulcanMessage)))
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
            get
            {
                return this._message;
            }
        }

        public Exception InnerException
        {
            get
            {
                return this._innerException;
            }
        }

        public Severity Severity
        {
            get
            {
                return this._severity;
            }
        }

        public override string ToString()
        {
            return _message;
        }
    }
}
