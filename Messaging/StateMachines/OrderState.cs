namespace Messaging.StateMachines
{
    using System;
    using Automatonymous;

    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public State CurrentState { get; set; }
    }
}