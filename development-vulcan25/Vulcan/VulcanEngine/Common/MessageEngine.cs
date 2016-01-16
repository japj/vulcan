using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AstFramework;
using VulcanEngine.IR.Ast;
using VulcanEngine.Properties;
using Microsoft.Build.Framework;

namespace VulcanEngine.Common
{
    public static class MessageEngine
    {
        private static readonly bool BreakOnError = Settings.Default.BreakOnError;
        private static Dictionary<Severity, List<VulcanMessage>> _errorDictionary;
        private static bool _statusOn;
        private static bool _consoleIsValid;
        private static int _percent;

        public static Microsoft.Build.Framework.ITask MSBuildTask
        {
            get;
            set;
        }

        public static int ErrorCount
        {
            get { return _errorDictionary[Severity.Error].Count; }
        }

        public static int WarningCount
        {
            get { return _errorDictionary[Severity.Warning].Count; }
        }

        public static void ClearMessages()
        {
            InitializeErrorDictionary();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Static initialization logic requires control structures and connot be done inline.")]
        static MessageEngine()
        {
            InitializeErrorDictionary();

            try
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                _consoleIsValid = true;
            }
            catch (IOException)
            {
                // Some Console properties will throw IOExceptions 
                // when Vulcan is being run from another process,
                // since the underlying handle may not be valid.
                _consoleIsValid = false;
            }
        }

        private static void InitializeErrorDictionary()
        {
            _errorDictionary = new Dictionary<Severity, List<VulcanMessage>>();
            foreach (Severity s in Enum.GetValues(typeof(Severity)))
            {
                _errorDictionary[s] = new List<VulcanMessage>();
            }
        }

        private static void ClearStatus()
        {
            if (_consoleIsValid)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                for (int i = 0; i < 50; ++i)
                {
                    Console.Write(" ");
                }

                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }

        private static void DisplayStatus()
        {
            if (_statusOn && _consoleIsValid)
            {
                ConsoleColor oldBackgroundColor = Console.BackgroundColor;

                Console.BackgroundColor = ConsoleColor.Yellow;
                int i = 0;
                while (i < _percent * 2 / 5)
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
                Console.Write(" {0}%", _percent);
            }
        }

        public static void UpdateProgress(double percent)
        {
            _statusOn = true;
            _percent = (int)(percent * 100);
            ClearStatus();
            DisplayStatus();
        }

        public static void Trace(AstNode astNode, Severity severity, string errorCode, string message, params object[] formatParameters)
        {
            if (astNode != null && astNode.BimlFile != null && astNode.BoundXObject != null && astNode.BoundXObject.XObject != null)
            {
                string filename = astNode.BimlFile.Name;
                int line = ((System.Xml.IXmlLineInfo)astNode.BoundXObject.XObject).LineNumber;
                int offset = ((System.Xml.IXmlLineInfo)astNode.BoundXObject.XObject).LinePosition;

                Trace(filename, line, offset, severity, errorCode, null, message, formatParameters);
            }
            else
            {
                Trace(null, -1, -1, severity, errorCode, null, message, formatParameters);
            }
        }

        public static void Trace(Severity severity, string message, params object[] formatParameters)
        {
            Trace(severity, null, message, formatParameters);
        }

        public static void Trace(Severity severity, Exception exception, string message, params object[] formatParameters)
        {
            Trace(null, -1, -1, severity, null, exception, message, formatParameters);
        }

        public static void Trace(string fileName, int line, int offset, Severity severity, string errorCode, Exception innerException, string message, params object[] formatParameters)
        {
            Trace(new VulcanMessage(fileName, line, offset, severity, errorCode, innerException, message, formatParameters));
        }

        private static bool MSBuildTrace(VulcanMessage message)
        {
            if (MSBuildTask != null)
            {
                switch (message.Severity)
                {
                    case Severity.Error:
                        MSBuildTask.BuildEngine.LogErrorEvent
                            (
                            new BuildErrorEventArgs
                                (
                                String.Empty,
                                message.Code, message.FileName,
                                message.Line, message.Offset,
                                0,
                                0,
                                message.AnnotatedMessage,
                                String.Empty,
                                String.Empty
                                )
                             );

                        break;
                    case Severity.Warning:
                        MSBuildTask.BuildEngine.LogWarningEvent(
                            new BuildWarningEventArgs
                                (
                                String.Empty,
                                message.Code, message.FileName,
                                message.Line, message.Offset,
                                0,
                                0,
                                message.AnnotatedMessage,
                                String.Empty,
                                String.Empty
                                )
                            );
                        break;
                    case Severity.Notification:
                        MSBuildTask.BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message.AnnotatedMessage, String.Empty, string.Empty, MessageImportance.Low));
                        break;
                    case Severity.Alert:
                        MSBuildTask.BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message.AnnotatedMessage, String.Empty, string.Empty, MessageImportance.High));
                        break;
#if DEBUG
                    case Severity.Debug:
                        MSBuildTask.BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message.AnnotatedMessage, String.Empty, string.Empty, MessageImportance.Low));
                        break;
#endif
                    default:
                        MSBuildTask.BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message.AnnotatedMessage, String.Empty, string.Empty, MessageImportance.Normal));
                        break;
                } // end switch(message.Severity)
                return true;
            } // end #if msbuild!=null
            return false;
        }
        public static void Trace(VulcanMessage vulcanMessage)
        {
            _errorDictionary[vulcanMessage.Severity].Add(vulcanMessage);

            if (!MSBuildTrace(vulcanMessage))
            {
                ClearStatus();
                int lineWidth = _consoleIsValid ? Console.BufferWidth : Int32.MaxValue;
                if (vulcanMessage.Severity == Severity.Debug)
                {
                    if (Settings.Default.ShowDebug)
                    {
                        WriteWithWordLineBreaks(vulcanMessage.AnnotatedMessage, Console.Out, lineWidth);
                    }
                }
                else if (vulcanMessage.Severity == Severity.Alert)
                {
                    ConsoleColor color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    WriteWithWordLineBreaks(vulcanMessage.AnnotatedMessage, Console.Out, lineWidth);
                    Console.ForegroundColor = color;
                }
                else if (vulcanMessage.Severity == Severity.Warning)
                {
                    ConsoleColor color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    WriteWithWordLineBreaks(vulcanMessage.AnnotatedMessage, Console.Error, lineWidth);
                    Console.ForegroundColor = color;
                }
                else if (vulcanMessage.Severity == Severity.Error)
                {
                    ConsoleColor color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    WriteWithWordLineBreaks(vulcanMessage.AnnotatedMessage, Console.Error, lineWidth);
                    Console.ForegroundColor = color;
                }
                else
                {
                    if (Settings.Default.ShowNotifications || vulcanMessage.Severity == Severity.Alert)
                    {
                        WriteWithWordLineBreaks(vulcanMessage.AnnotatedMessage, Console.Out, lineWidth);
                    }
                }
                DisplayStatus();
            }
        }

        private static void WriteWithWordLineBreaks(string message, TextWriter textWriter, int lineWidth)
        {
            string[] words = message.Split(' ');
            var builder = new StringBuilder();
            bool firstLine = true;
            bool firstWordInFirstLine = true;
            foreach (string word in words)
            {
                if (builder.Length + word.Length < lineWidth)
                {
                    if (firstWordInFirstLine)
                    {
                        firstWordInFirstLine = false;
                    }
                    else
                    {
                        builder.Append(" ");
                    }

                    builder.Append(word);
                }
                else
                {
                    if (builder.Length > 0)
                    {
                        if (builder.Length == lineWidth)
                        {
                            textWriter.Write(builder); // Console handles the line break in this case
                        }
                        else
                        {
                            textWriter.WriteLine(builder);
                        }

                        firstLine = false;
                    }

                    builder = new StringBuilder(firstLine ? word : "     " + word);
                }
            }

            if (builder.Length > 0)
            {
                textWriter.WriteLine(builder);
            }
        }

        public static void HandleException()
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.ForegroundColor = color;
        }
    }
}
