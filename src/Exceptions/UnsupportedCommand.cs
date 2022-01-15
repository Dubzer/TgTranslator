using System;

namespace TgTranslator.Exceptions;

public class UnsupportedCommand : Exception
{
    public UnsupportedCommand(string request) : base($@"Command {request} is not supported")
    {
    }
}