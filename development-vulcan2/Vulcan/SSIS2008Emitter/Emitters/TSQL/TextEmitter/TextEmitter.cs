using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Ssis2008Emitter.Properties;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;

namespace Ssis2008Emitter.Emitters.TSQL.TextEmitter
{
    public class TextEmitter
    {
        public TextEmitter()
        {
            _target = new TextTarget();
        }

        public int Emit(LogicalObject obj)
        {
            if (obj is Package)
            {
                _target.Initialize(VulcanEngine.Common.PathManager.GetToolPath(), ((Package)obj).Name);
            }
            _target.EmitObject(obj);

            return 0;
        }

        public void Close()
        {
            _target.DeInitialize();
        }

        private TextTarget _target;
    }
}
