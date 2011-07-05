using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;

namespace GroovesharkDownloader
{
    class BassException : Exception
    {
        public BASSError ErrorCode { get; set; }
    }
}
