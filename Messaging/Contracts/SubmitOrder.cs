using System;

namespace Messaging.Contracts;

public interface SubmitOrder
{
    Guid OrderId { get; }
}