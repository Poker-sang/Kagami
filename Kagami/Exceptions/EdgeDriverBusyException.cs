using System;

namespace Kagami.Exceptions;

public class EdgeDriverBusyException : Exception
{
    public EdgeDriverBusyException(string message) : base(message)
    {

    }
}