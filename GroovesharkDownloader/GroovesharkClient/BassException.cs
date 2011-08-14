using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;

namespace GroovesharkPlayer
{
    class BassException : Exception
    {
        public BASSError ErrorCode { get; set; }
    }
}
