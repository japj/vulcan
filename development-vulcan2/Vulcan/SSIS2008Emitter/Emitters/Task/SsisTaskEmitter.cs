using System;
using System.Collections.Generic;
using System.Text;

using DTS = Microsoft.SqlServer.Dts.Runtime;

using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.Emitters.Task
{
    public abstract class SsisTaskEmitter
    {
        #region Private Storage
        protected string _name;
        private string _description;
        protected SSISEmitterContext _context;
        #endregion  // Private Storage

        public abstract DTS.IDTSPropertiesProvider PropertyProvider { get; }

        public virtual string Name
        {
            get { return _name; }
        }

        public string Description
        {
            get { return _description; }
        }

        public DTS.IDTSSequence ParentContainer
        {
            get { return _context.ParentContainer; }
        }

        public SSISEmitterContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public virtual SsisExecutable SSISExecutable
        {
            get { return null; }
        }

        protected SsisTaskEmitter(LogicalObject obj, SSISEmitterContext context)
        {
            _name = obj.Name;
            _description = obj.Name;
            _context = context;
        }

        public virtual void SetExpression(string name, string expression)
        {
            PropertyProvider.SetExpression(name, expression);
        }

        public virtual void SetProperty(string name, object value)
        {
            if (PropertyProvider.Properties.Contains(name))
            {
                PropertyProvider.Properties[name].SetValue(PropertyProvider, value);
            }
        }

        public virtual void Execute()
        {
            SsisExecutable executable = SSISExecutable;

            if (executable != null)
            {
                executable.Execute();
            }
        }
    }
}
