using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Data.OleDb;

using DTS = Microsoft.SqlServer.Dts.Runtime;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.Emitters.Common;
using Ssis2008Emitter.Emitters.Framework;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.IR.Task;

namespace Ssis2008Emitter.Emitters.Task
{
    [PhysicalIRMapping(typeof(SequenceTask))]
    public class SsisContainer : SsisTaskEmitter, ISSISEmitter
    {
        private DTS.Sequence _SSISContainer;
        private Stack<Dictionary<string, SsisVariable>> _logStack = new Stack<Dictionary<string, SsisVariable>>();

        protected SequenceTask _logicalContainer;

        public override DTS.IDTSPropertiesProvider PropertyProvider
        {
            get { return _SSISContainer; }
        }

        protected DTS.Sequence DTSSequenceContainer
        {
            get { return _SSISContainer; }
        }

        public DTS.IDTSSequence SequenceContainer
        {
            get { return _SSISContainer; }
        }

        // TODO: Could we lose the constraint information in Container from doing this as ContainerBase?
        public SsisContainer(SequenceTask container, SSISEmitterContext context)
            : base(container, context)
        {
            this._logicalContainer = container;
            if (this.Name != null && this.ParentContainer != null)
            {
                if (this.ParentContainer.Executables.Contains(Name))
                {
                    if (this.ParentContainer.Executables[Name] is DTS.IDTSSequence)
                    {
                        _SSISContainer = (DTS.Sequence)this.ParentContainer.Executables[Name];
                    }
                }
                else
                {
                    DTS.Sequence sequence = (DTS.Sequence)context.SSISSequence.AppendExecutable("STOCK:Sequence");
                    sequence.Name = this.Name;
                    _SSISContainer = sequence;
                }
            }
            else if (container is Package)
            {
                // Do Nothing
            }
            else
            {
                MessageEngine.Global.Trace(Severity.Error, new ArgumentException("parentContainer cannot be null"), "parentContainer cannot be null");
            }
        }


        public void EmitPatterns(SSISEmitterContext context)
        {
            foreach (IR.Task.Task task in this._logicalContainer.Tasks)
            {
                // TODO: No need to preserve context?
                context.InstantiateEmitter(task, context).Emit();
            }
        }

        public virtual SSISEmitterContext Emit()
        {
            SSISEmitterContext newContext = _context.NewParentSequence(new SsisSequence(this.SequenceContainer, this._logicalContainer));
            this.DTSSequenceContainer.TransactionOption = (Microsoft.SqlServer.Dts.Runtime.DTSTransactionOption)
                    Enum.Parse(typeof(Microsoft.SqlServer.Dts.Runtime.DTSTransactionOption), this._logicalContainer.TransactionMode);

            //TODO: Hardcode the IsolationLevel to ReadCommitted. This will be changed to be customizable.
            this.DTSSequenceContainer.IsolationLevel = System.Data.IsolationLevel.ReadCommitted;

            foreach (Variable variable in _logicalContainer.VariableList)
            {
                SsisVariable s = new SsisVariable(variable, newContext);
            }

            EmitPatterns(newContext);

            return newContext;
        }

        public SsisVariable PeekStackVariable(string varName)
        {
            SsisVariable varRet = null;

            if (_logStack.Any())
            {
                Dictionary<string, SsisVariable> TopVariables = _logStack.Peek();

                if (TopVariables.ContainsKey(varName))
                {
                    varRet = TopVariables[varName];
                }
            }

            return varRet;
        }

        public virtual OleDbType GetVariableType(Variable variable)
        {
            OleDbType retType = OleDbType.Empty;

            switch (variable.Type.ToUpper(System.Globalization.CultureInfo.InvariantCulture))
            {
                case "STRING": retType = OleDbType.WChar; break;
                case "INT32": retType = OleDbType.Integer; break;
                case "INT64": retType = OleDbType.BigInt; break;
                case "BOOLEAN": retType = OleDbType.Boolean; break;
                case "Byte": retType = OleDbType.Binary; break;
                case "Char": retType = OleDbType.Char; break;
                case "DateTime": retType = OleDbType.DBTimeStamp; break;
                case "DBNull": retType = OleDbType.Empty; break;
                case "Double": retType = OleDbType.Double; break;
                case "Int16": retType = OleDbType.SmallInt; break;
                case "SByte": retType = OleDbType.Binary; break;
                case "Single": retType = OleDbType.Single; break;
                case "UInt32": retType = OleDbType.UnsignedInt; break;
                case "UInt64": retType = OleDbType.UnsignedBigInt; break;

            }
            return retType;
        }
    }
}
