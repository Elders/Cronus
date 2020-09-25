using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elders.Cronus.Queries
{
    public class QueryProvider
    {
        private readonly ISet<KeyValuePair<Type, IQueryHandlerDefinition>> _queryHandlers;

        QueryProvider()
        {
            _queryHandlers = new HashSet<KeyValuePair<Type, IQueryHandlerDefinition>>();
        }

        public QueryProvider(IEnumerable<IQueryHandlerDefinition> queryHandlers)
        {
            foreach (var item in queryHandlers)
            {
                var allImplementedInterfaces = item.GetType().GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)).ToList();

                foreach (var inter in allImplementedInterfaces)
                {
                    var pair = new KeyValuePair<Type, IQueryHandlerDefinition>(inter.GetGenericArguments()[0], item);
                    _queryHandlers.Add(pair);
                }
            }
        }

        public async Task<QueryResult<TResult>> QueryAsync<TResult>(IQuery<TResult> query)
            where TResult : class
        {
            var result = new QueryResult<TResult>();
            dynamic handler = _queryHandlers.Where(x => x.Key == query.GetType());

            if (handler is null)
            {
                result.Errors = new string[] { $"Missing 'IQueryHandler' implementation for query of type '{query.GetType().FullName}'" };
            }
            else
            {

                try
                {
                    result.Result = await handler.HandleAsync((dynamic)query);
                }
                catch (Exception ex)
                {
                    result.Errors = new string[] { ex.Message };
                }
            }

            return result;
        }
    }
}
