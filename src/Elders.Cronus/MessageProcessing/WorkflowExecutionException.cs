using System;

namespace Elders.Cronus.MessageProcessing
{
    public class WorkflowExecutionException : Exception
    {
        public WorkflowExecutionException() { }

        public WorkflowExecutionException(string message, ErrorContext context)
            : base(message)
        {
            Context = context;
        }

        public WorkflowExecutionException(string message, ErrorContext context, Exception inner)
            : base(message, inner)
        {
            Context = context;
        }

        public ErrorContext Context { get; private set; }
    }
}


