using System;

namespace TgTranslator.Exceptions
{
    public class InvalidSettingException : Exception
    {
        public InvalidSettingException()
        {
        }

        public InvalidSettingException(string message) : base(message)
        {
        }

        public InvalidSettingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}