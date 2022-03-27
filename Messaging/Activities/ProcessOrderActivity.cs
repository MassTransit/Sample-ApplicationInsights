using System.Threading.Tasks;
using MassTransit;
using Messaging.Contracts;

namespace Messaging.Activities;

public class ProcessOrderActivity :
    IExecuteActivity<ProcessOrderArguments>
{
    public async Task<ExecutionResult> Execute(ExecuteContext<ProcessOrderArguments> context)
    {
        return context.Completed();
    }
}