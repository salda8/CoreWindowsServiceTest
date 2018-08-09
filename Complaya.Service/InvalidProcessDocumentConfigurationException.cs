using System;
using System.Runtime.Serialization;

namespace Complaya.Service
{
    internal class InvalidProcessDocumentConfigurationException : Exception
    {
        public InvalidProcessDocumentConfigurationException()
        {
        }

        public InvalidProcessDocumentConfigurationException(string message) : base(message)
        {
        }

        public InvalidProcessDocumentConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidProcessDocumentConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}