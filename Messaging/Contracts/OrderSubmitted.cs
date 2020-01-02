namespace Messaging.Contracts
{
    using System;

    public interface OrderSubmitted
    {
        Guid OrderId { get; }
    }
}