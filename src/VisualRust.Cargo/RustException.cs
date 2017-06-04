using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    class RustException : ExternalException
    {
        public const int PanicCode = 0x00524323;

        private readonly string message;

        public RustException(string msg, Exception inner) : base(msg, inner)
        {
            this.message = msg;
        }

        public override int ErrorCode
        {
            get { return PanicCode; }
        }

        public override string Message
        {
            get { return message ?? InnerException.Message; }
        }
    }
}
