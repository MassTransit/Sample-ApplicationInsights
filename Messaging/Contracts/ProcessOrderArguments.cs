namespace Messaging.Contracts
{
    using System;

    public interface ProcessOrderArguments
    {
        Guid OrderId { get; }
    }
}