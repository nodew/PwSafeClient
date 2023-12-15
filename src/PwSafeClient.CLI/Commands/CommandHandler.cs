using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands
{
    public class CommandHandler : ICommandHandler
    {
        public int Invoke(InvocationContext context)
        {
            int result = 0;
            Task.Run(async () => result = await InvokeAsync(context)).Wait();
            return result;
        }

        public virtual Task<int> InvokeAsync(InvocationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
