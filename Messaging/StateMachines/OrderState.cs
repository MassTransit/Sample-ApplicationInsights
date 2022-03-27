using System;
using MassTransit;

namespace Messaging.StateMachines;

public class OrderState : SagaStateMachineInstance
{
    public State CurrentState { get; set; }
    public Guid CorrelationId { get; set; }
}