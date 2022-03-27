using System;

namespace Messaging.Contracts;

public interface OrderSubmitted
{
    Guid OrderId { get; }
}