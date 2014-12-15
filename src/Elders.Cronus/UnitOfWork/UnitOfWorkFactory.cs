//using System;

//namespace Elders.Cronus.UnitOfWork
//{
//    public class UnitOfWorkFactory
//    {
//        public UnitOfWorkFactory()
//        {
//            NoBatchUnitofWork noBatchUnitOfWork = new NoBatchUnitofWork();
//            CreateBatchUnitOfWork = () => noBatchUnitOfWork;

//            NoMessageUnitofWork noMessageUnitOfWork = new NoMessageUnitofWork();
//            CreateMessageUnitOfWork = () => noMessageUnitOfWork;

//            NoHandlerUnitofWork noHandlerUnitOfWork = new NoHandlerUnitofWork();
//            CreateHandlerUnitOfWork = () => noHandlerUnitOfWork;
//        }

//        public Func<IBatchUnitOfWork> CreateBatchUnitOfWork { get; set; }

//        public Func<IHandlerUnitOfWork> CreateHandlerUnitOfWork { get; set; }

//        public Func<IMessageUnitOfWork> CreateMessageUnitOfWork { get; set; }

//        public bool UseBatchUnitOfWork(Context context, Func<Context, bool> action)
//        {
//            return UseUnitOfWork<IBatchUnitOfWork>(context, CreateBatchUnitOfWork, uow => context.BatchContext = uow.Context, action);
//        }

//        public bool UseHandlerUnitOfWork(Context context, Func<Context, bool> action)
//        {
//            return UseUnitOfWork<IHandlerUnitOfWork>(context, CreateHandlerUnitOfWork, uow => context.HandlerContext = uow.Context, action);
//        }

//        public bool UseMessageUnitOfWork(Context context, Func<Context, bool> action)
//        {
//            return UseUnitOfWork<IMessageUnitOfWork>(context, CreateMessageUnitOfWork, uow => context.MessageContext = uow.Context, action);
//        }

//        private bool UseUnitOfWork<T>(Context context, Func<T> unitOfWorkFactory, Action<T> contextBuilder, Func<Context, bool> action) where T : IUnitOfWork
//        {
//            bool result = false;

//            T unitOfWork = unitOfWorkFactory();
//            if (unitOfWork.Context == null)
//                unitOfWork.Context = new UnitOfWorkContext();
//            unitOfWork.Begin();
//            contextBuilder(unitOfWork);
//            result = action(context);
//            unitOfWork.End();
//            unitOfWork.Context.Clear();
//            unitOfWork = default(T);

//            return result;
//        }

//        

//        class NoBatchUnitofWork : NoUnitOfWork, IBatchUnitOfWork { }

//        class NoMessageUnitofWork : NoUnitOfWork, IMessageUnitOfWork { }

//        class NoHandlerUnitofWork : NoUnitOfWork, IHandlerUnitOfWork { }
//    }
//}