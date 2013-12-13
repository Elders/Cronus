using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NMSD.Cronus.Core;
using RabbitMQ.Client;

namespace NSMD.Cronus.RabbitMQ
{
    public static class RetryableOperation
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RetryableOperation));

        static RetryPolicy defaultExponentialRetryPolicy = RetryPolicyFactory.CreateExponentialRetryPolicy(5, new TimeSpan(0, 0, 3), new TimeSpan(0, 0, 90), new TimeSpan(0, 0, 6));

        /// <summary>
        /// Number of retries: 5;
        ///       Min Backoff: 3 seconds;
        ///       Max Backoff: 90 seconds;
        ///              Step: 6 seconds;
        /// </summary>
        public static RetryPolicy DefaultExponentialRetryPolicy { get { return defaultExponentialRetryPolicy; } }

        public static T TryExecute<T>(Func<T> operation, RetryPolicy retryPolicy)
        {
            var retry = retryPolicy();
            Exception exception;
            T operationResult = default(T);

            TimeSpan delay;
            for (int i = 0; i > -1; i++)
            {
                operationResult = InvokeTryExecuteInternal(operation, out exception);
                if (operationResult == null || operationResult.Equals(default(T)))
                {
                    if (retry(i, exception, out delay))
                    {
                        Console.WriteLine("Retry {0}", i);
                        Console.WriteLine("Sleeping for {0}", delay);
                        Thread.Sleep(delay);
                    }
                    else
                    {
                        Console.WriteLine("No more retries");
                        throw exception;
                    }
                }
                else
                {
                    Console.WriteLine("++++++++++++");
                    break;
                }
            }
            return operationResult;
        }

        private static T InvokeTryExecuteInternal<T>(Func<T> operation, out Exception exception)
        {
            exception = null;
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                exception = ex;
                return default(T);
            }
        }
    }

    public sealed class Plumber
    {
        private IConnection connection;

        private ConnectionFactory factory;

        private readonly string hostname;

        private readonly string password;

        private readonly int port;

        private readonly string username;

        private readonly string virtualHost;

        public Plumber() : this("192.168.16.53") { }

        public Plumber(string hostname, string username = ConnectionFactory.DefaultUser, string password = ConnectionFactory.DefaultPass, int port = 5672, string virtualHost = ConnectionFactory.DefaultVHost)
        {
            this.hostname = hostname;
            this.username = username;
            this.password = password;
            this.port = port;
            this.virtualHost = virtualHost;
            factory = new ConnectionFactory
            {
                HostName = hostname,
                Port = port,
                UserName = username,
                Password = password,
                VirtualHost = virtualHost
            };
        }

        public IConnection RabbitConnection
        {
            get
            {
                if (connection == null || !connection.IsOpen)
                {
                    connection = RetryableOperation.TryExecute<IConnection>(() => factory.CreateConnection(), RetryableOperation.DefaultExponentialRetryPolicy);
                }
                return connection;
            }
        }
    }
}