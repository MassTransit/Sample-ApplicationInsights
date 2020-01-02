namespace Messaging.Consumers
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;

    public class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
    {
        public Task Consume(ConsumeContext<OrderSubmitted> context)
        {
            return context.RespondAsync<OrderObserved>(context.Message);
        }
    }
}