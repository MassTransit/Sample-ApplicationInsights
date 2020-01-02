namespace Messaging.Contracts
{
    using System;

    public interface OrderObserved
    {
        Guid OrderId { get; }
    }
}