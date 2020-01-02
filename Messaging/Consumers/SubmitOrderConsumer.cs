namespace Messaging.Consumers
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using MassTransit.Context;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;

    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public Task Consume(ConsumeContext<SubmitOrder> context)
        {
            LogContext.Info?.Log("Submitting Order: {OrderId}", context.Message.OrderId);

            var builder = new RoutingSlipBuilder(NewId.NextGuid());
            if (!EndpointConvention.TryGetDestinationAddress<ProcessOrderArguments>(out var activityAddress))
                throw new ConfigurationException("No endpoint address for activity");

            builder.AddActivity("Process", activityAddress);

            if (!EndpointConvention.TryGetDestinationAddress<OrderProcessed>(out var eventAddress))
                throw new ConfigurationException("No endpoint address for activity");

            builder.AddSubscription(eventAddress, RoutingSlipEvents.Completed, endpoint =>
                endpoint.Send<OrderProcessed>(context.Message));

            context.Execute(builder.Build());

            return context.Publish<OrderSubmitted>(context.Message, x => x.ResponseAddress = context.ResponseAddress);
        }
    }
}
