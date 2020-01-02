namespace Messaging.Activities
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit.Courier;

    public class ProcessOrderActivity :
        IExecuteActivity<ProcessOrderArguments>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<ProcessOrderArguments> context)
        {
            return context.Completed();
        }
    }
}