namespace Messaging.Contracts
{
    using System;

    public interface SubmitOrder
    {
        Guid OrderId { get; }
    }
}