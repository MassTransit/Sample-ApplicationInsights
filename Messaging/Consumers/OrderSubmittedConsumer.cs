using System.Threading.Tasks;
using MassTransit;
using Messaging.Contracts;

namespace Messaging.Consumers;

public class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        return context.RespondAsync<OrderObserved>(context.Message);
    }
}