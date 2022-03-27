using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier.Contracts;
using Messaging.Activities;
using Messaging.Contracts;
using Messaging.StateMachines;

namespace Messaging.Consumers;

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private IEndpointNameFormatter _endpointNameFormatter;

    public SubmitOrderConsumer(ISendEndpointProvider sendEndpointProvider, IEndpointNameFormatter endpointNameFormatter)
    {
        _sendEndpointProvider = sendEndpointProvider;
        _endpointNameFormatter = endpointNameFormatter;
    }

    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        LogContext.Info?.Log("Submitting Order: {OrderId}", context.Message.OrderId);

        var builder = new RoutingSlipBuilder(NewId.NextGuid());

        var processEndpoint =
            new Uri($"queue:{_endpointNameFormatter.ExecuteActivity<ProcessOrderActivity, ProcessOrderArguments>()}");
        builder.AddActivity("Process", processEndpoint);

        var eventAddress =
            new Uri($"queue:{_endpointNameFormatter.Saga<OrderState>()}");
        builder.AddActivity("Process", processEndpoint);

        await builder.AddSubscription(eventAddress, RoutingSlipEvents.Completed, endpoint =>
            endpoint.Send<OrderProcessed>(context.Message));

        await context.Execute(builder.Build());

        await context.Publish<OrderSubmitted>(context.Message, x => x.ResponseAddress = context.ResponseAddress);
    }
}