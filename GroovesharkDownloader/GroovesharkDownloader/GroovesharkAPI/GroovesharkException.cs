using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroovesharkAPI.ConnectionTypes;

namespace GroovesharkAPI
{
    class GroovesharkException : Exception
    {
        public GroovesharkException(FaultCode code)
        {
            FaultErrorCode = code;
        }

        public FaultCode FaultErrorCode { get; set; }
    }
}
