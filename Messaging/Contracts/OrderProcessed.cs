namespace Messaging.Contracts
{
    using System;

    public interface OrderProcessed
    {
        Guid OrderId { get; }
    }
}