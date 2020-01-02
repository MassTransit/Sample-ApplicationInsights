namespace Messaging.Contracts
{
    using System;

    public interface OrderAwaitingDelivery
    {
        Guid OrderId { get; }
    }
}