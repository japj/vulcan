using System;
using System.Collections.Generic;
using System.Text;

using Vulcan.Data;
using System.IO;

namespace TextEmitter
{
    public class TextEmitter : Vulcan.Kernel.Emitter
    {
        public TextEmitter()
        {
            _target = new TextTarget();
        }

        public override int Emit(LogicalObject obj, Vulcan.Kernel.EmitterContext context)
        {
            if (obj is Package)
            {
                _target.Initialize(context.DetegoRootPath, Path.ChangeExtension(context.XMLFileName, null));
            }

            _target.EmitObject(obj, context);

            return 0;
        }

        public override void Close()
        {
            _target.DeInitialize();
        }

        private TextTarget _target;
    }
}
