using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Elders.Cronus.Messaging.MessageHandleScope
{
    public class ScopeFactory
    {
        public ScopeFactory()
        {
            EmptyBatchScope emptyBatchScope = new EmptyBatchScope();
            CreateBatchScope = () => emptyBatchScope;

            EmptyMessageScope emptyMessageScope = new EmptyMessageScope();
            CreateMessageScope = () => emptyMessageScope;

            EmptyHandlerScope emptyHandlerScope = new EmptyHandlerScope();
            CreateHandlerScope = () => emptyHandlerScope;
        }

        public Func<IBatchScope> CreateBatchScope { get; set; }

        public Func<IHandlerScope> CreateHandlerScope { get; set; }

        public Func<IMessageScope> CreateMessageScope { get; set; }

        public bool UseBatchScope(Context context, Func<Context, bool> action)
        {
            return UseScope<IBatchScope>(context, CreateBatchScope, scope => context.BatchScopeContext = scope.Context, action);
        }

        public bool UseHandlerScope(Context context, Func<Context, bool> action)
        {
            return UseScope<IHandlerScope>(context, CreateHandlerScope, scope => context.HandlerScopeContext = scope.Context, action);
        }

        public bool UseMessageScope(Context context, Func<Context, bool> action)
        {
            return UseScope<IMessageScope>(context, CreateMessageScope, scope => context.MessageScopeContext = scope.Context, action);
        }

        private bool UseScope<T>(Context context, Func<T> scopeFactory, Action<T> contextBuilder, Func<Context, bool> action) where T : IScope
        {
            bool result = false;

            T scope = scopeFactory();
            if (scope.Context == null)
                scope.Context = new ScopeContext();
            scope.Begin();
            contextBuilder(scope);
            result = action(context);
            scope.End();
            scope.Context.Clear();
            scope = default(T);

            return result;
        }

        class EmptyScope : IScope
        {
            public void Begin() { }

            public void End() { }

            public IScopeContext Context { get; set; }
        }

        class EmptyBatchScope : EmptyScope, IBatchScope
        {
            public int Size
            {
                get { return 1; }
                set { }
            }
        }

        class EmptyMessageScope : EmptyScope, IMessageScope { }

        class EmptyHandlerScope : EmptyScope, IHandlerScope { }
    }
}