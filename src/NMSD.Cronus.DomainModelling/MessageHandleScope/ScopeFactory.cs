using System;

namespace NMSD.Cronus.Messaging.MessageHandleScope
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

        public Context CurrentContext { get; set; }

        public bool UseBatchScope(Func<IBatchScope, bool> action)
        {
            CurrentContext = new Context();
            return UseScope<IBatchScope>(CreateBatchScope, (scope) => CurrentContext.BatchScopeContext = scope.Context, action);
        }

        public bool UseHandlerScope(Func<IHandlerScope, bool> action)
        {
            return UseScope<IHandlerScope>(CreateHandlerScope, (scope) => CurrentContext.HandlerScopeContext = scope.Context, action);
        }

        public bool UseMessageScope(Func<IMessageScope, bool> action)
        {
            return UseScope<IMessageScope>(CreateMessageScope, (scope) => CurrentContext.MessageScopeContext = scope.Context, action);
        }

        private bool UseScope<T>(Func<T> scopeFactory, Action<T> contextBuilder, Func<T, bool> action) where T : IScope
        {
            bool result = false;
            T scope = scopeFactory();
            if (scope.Context == null)
                scope.Context = new ScopeContext();
            scope.Begin();
            contextBuilder(scope);
            result = action(scope);
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