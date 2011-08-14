using System;
using GroovesharkAPI.ConnectionTypes;

namespace GroovesharkAPI
{
    [Serializable]
    class GroovesharkException : Exception
    {
        public GroovesharkException(FaultCode code, string message = null, Exception inner = null) : base(message,inner)
        {
            ErrorCode = code;
        }

        public FaultCode ErrorCode { get; set; }
    }
}
