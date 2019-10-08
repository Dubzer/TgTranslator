using System;
using System.Threading;

namespace TgTranslator.Exceptions
{
    public class UnsupportedMenuItem : Exception
    {
        public UnsupportedMenuItem(string name) : base($@"Menu {name} is not supported")
        {
            
        }
    }
}