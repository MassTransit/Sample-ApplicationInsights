using System;

namespace Messaging.Contracts;

public interface OrderProcessed
{
    Guid OrderId { get; }
}