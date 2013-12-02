using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Multithreading.Work;
using NSMD.Cronus.RabbitMQ;
using Protoreg;

namespace NMSD.Cronus.Core.Commanding
{
    public class RabbitConsumer : RabbitCommandBus, IConsumer<ICommandHandler>
    {
        string commandsQueueName;

        HashSet<string> handlerQueues = new HashSet<string>();

        private Dictionary<Type, List<Func<ICommand, bool>>> handlers = new Dictionary<Type, List<Func<ICommand, bool>>>();

        IDictionary headers;

        private readonly ProtoregSerializer serialiser;

        public RabbitConsumer(ProtoregSerializer serialiser)
            : base(serialiser)
        {
            this.serialiser = serialiser;
            headers = new Dictionary<string, string>();
            headers.Add("x-match", "any");
        }

        public void RegisterHandler(Type eventType, Type eventHandlerType, Func<Type, ICommandHandler> handlerFactory)
        {
            if (!handlers.ContainsKey(eventType))
                handlers[eventType] = new List<Func<ICommand, bool>>();

            handlers[eventType].Add(x => Handle(x, eventHandlerType, handlerFactory));

            AddMessageContractToQueueHeaders(eventType, eventHandlerType);
        }
        WorkPool pool;
        public void Start(int numberOfWorkers)
        {
            Plumber plumber = new Plumber();

            //  Think about this
            Endpoint endpoint = plumber.GetEndpoint(commandsQueueName, headers);
            var pipe = plumber.GetPipeline(pipeName);
            pipe.AttachEndpoint(endpoint);
            endpoint.Dispose();
            //  Think about this

            pool = new WorkPool("defaultPool", numberOfWorkers);
            for (int i = 0; i < numberOfWorkers; i++)
            {
                pool.AddWork(new ConsumerWork(this, plumber.GetEndpoint(commandsQueueName, headers)));
            }


            pool.StartCrawlers();
        }

        public void Stop()
        {
            pool.Stop();
        }

        string pipeName;
        void AddMessageContractToQueueHeaders(Type messageType, Type messageHandlerType)
        {
            pipeName = MessagingHelper.GetPipelineName(messageHandlerType);

            const string queueSuffix = "Commands";
            commandsQueueName = String.Format("{0}.{1}", MessagingHelper.GetQueuePrefix(messageHandlerType), queueSuffix);
            headers.Add(MessagingHelper.GetMessageId(messageType), String.Empty);
        }

        bool Handle(ICommand message)
        {
            List<Func<ICommand, bool>> availableHandlers;
            if (handlers.TryGetValue(message.GetType(), out availableHandlers))
            {
                foreach (var handleMethod in availableHandlers)
                {
                    var result = handleMethod(message);
                    if (result == false)
                        return result;
                }
            }
            return true;
        }

        bool Handle(ICommand message, Type eventHandlerType, Func<Type, ICommandHandler> handlerFactory)
        {
            dynamic handler = null;
            try
            {
                handler = handlerFactory(eventHandlerType);
                handler.Handle((dynamic)message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        class ConsumerWork : IWork, IDisposable
        {
            private RabbitConsumer consumer;
            private Endpoint endpoint;

            public ConsumerWork(RabbitConsumer consumer, Endpoint endpoint)
            {
                this.endpoint = endpoint;
                this.consumer = consumer;
            }

            public DateTime ScheduledStart { get; set; }

            public void Start()
            {
                ICommand command;
                dynamic asd = endpoint.Dequeue();
                using (var stream = new MemoryStream(asd.Body))
                {
                    command = consumer.serialiser.Deserialize(stream) as ICommand;
                }
                if (consumer.Handle(command))
                    endpoint.Aknowledge(asd);
            }


            public void Dispose()
            {
                if (endpoint != null)
                {
                    endpoint.Dispose();
                    endpoint = null;
                }
                consumer = null;
            }
        }
    }

    public class RabbitCommandBus : RabbitPublisher<ICommand>
    {
        public RabbitCommandBus(ProtoregSerializer serialiser)
            : base(serialiser)
        {

        }
    }
}
