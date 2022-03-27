using System;

namespace Messaging.Contracts;

public interface OrderAwaitingDelivery
{
    Guid OrderId { get; }
}