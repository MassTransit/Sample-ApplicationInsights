using System;

namespace Messaging.Contracts;

public interface OrderObserved
{
    Guid OrderId { get; }
}