using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elders.Cronus.Queries
{
    public class QueryResult<TResult> where TResult : class
    {
        public QueryResult()
        {
            Errors = new List<string>();
        }

        public TResult Result { get; set; }

        public bool Success => Errors.Count() == 0;

        public IEnumerable<string> Errors { get; set; }

        public string ErrorsAsString => string.Join('\n', Errors);
    }
}
