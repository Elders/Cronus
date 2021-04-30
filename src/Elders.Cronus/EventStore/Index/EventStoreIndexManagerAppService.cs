﻿namespace Elders.Cronus.EventStore.Index
{
    public class EventStoreIndexManagerAppService : ApplicationService<EventStoreIndexManager>, ISystemAppService,
        ICommandHandler<RegisterIndex>,
        ICommandHandler<RebuildIndex>,
        ICommandHandler<FinalizeEventStoreIndexRequest>
    {
        public EventStoreIndexManagerAppService(IAggregateRepository repository) : base(repository) { }

        public void Handle(RegisterIndex command)
        {
            EventStoreIndexManager ar = null;
            ReadResult<EventStoreIndexManager> result = repository.Load<EventStoreIndexManager>(command.Id);
            if (result.IsSuccess)
            {
                ar = result.Data;
                ar.Register();
            }

            if (result.NotFound)
                ar = new EventStoreIndexManager(command.Id);

            repository.Save(ar);
        }

        public void Handle(RebuildIndex command)
        {
            EventStoreIndexManager ar = null;
            ReadResult<EventStoreIndexManager> result = repository.Load<EventStoreIndexManager>(command.Id);

            if (result.NotFound)
            {
                ar = new EventStoreIndexManager(command.Id);
            }

            if (result.IsSuccess)
            {
                ar = result.Data;
            }

            ar.Rebuild();

            repository.Save(ar);
        }

        public void Handle(FinalizeEventStoreIndexRequest command)
        {
            EventStoreIndexManager ar = null;
            ReadResult<EventStoreIndexManager> result = repository.Load<EventStoreIndexManager>(command.Id);

            if (result.NotFound)
            {
                ar = new EventStoreIndexManager(command.Id);
            }

            if (result.IsSuccess)
            {
                ar = result.Data;
            }

            ar.FinalizeRequest();

            repository.Save(ar);
        }
    }
}
