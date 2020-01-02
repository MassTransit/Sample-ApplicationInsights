namespace Messaging.StateMachines
{
    using Automatonymous;
    using Contracts;
    using MassTransit;

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => Submitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => Processed, x => x.CorrelateById(m => m.Message.OrderId));

            Initially(
                When(Submitted)
                    .TransitionTo(PendingApproval),
                When(Processed)
                    .PublishAsync(x => x.Init<OrderAwaitingDelivery>(x.Data))
                    .TransitionTo(AwaitingDelivery));

            During(PendingApproval,
                   When(Processed)
                       .PublishAsync(x => x.Init<OrderAwaitingDelivery>(x.Data))
                       .TransitionTo(AwaitingDelivery));

            During(AwaitingDelivery,
                   Ignore(Submitted));
        }

        public Event<OrderSubmitted> Submitted { get; set; }
        public Event<OrderProcessed> Processed { get; set; }

        public State PendingApproval { get; set; }
        public State AwaitingDelivery { get; set; }
    }
}