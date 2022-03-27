using System;

namespace Messaging.Contracts;

public interface ProcessOrderArguments
{
    Guid OrderId { get; }
}