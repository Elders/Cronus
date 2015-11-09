using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.Userfull
{
    public struct Result<T>
    {
        List<Exception> errors;

        public Result(T value)
        {
            Value = value;
            errors = null;
        }

        public T Value { get; }

        public bool IsSuccessful { get { return ReferenceEquals(null, errors) || errors.Count == 0; } }

        public bool IsNotSuccessful { get { return IsSuccessful == false; } }

        public IEnumerable<Exception> Errors
        {
            get
            {
                return ReferenceEquals(null, errors) ? Enumerable.Empty<Exception>() : errors.AsReadOnly();
            }
        }

        public Result<T> WithError(Exception error)
        {
            var newErrors = ReferenceEquals(null, errors) ? new List<Exception>() : new List<Exception>(errors);
            newErrors.Add(error);
            var newResult = new Result<T>();
            newResult.errors = newErrors;
            return newResult;
        }

        public Result<T> WithError(string errorMessage)
        {
            var newErrors = ReferenceEquals(null, errors) ? new List<Exception>() : new List<Exception>(errors);
            newErrors.Add(new Exception(errorMessage));
            var newResult = new Result<T>();
            newResult.errors = newErrors;
            return newResult;
        }

        public Result<T> WithError(IEnumerable<Exception> errors)
        {
            var newErrors = ReferenceEquals(null, this.errors) ? new List<Exception>() : new List<Exception>(this.errors);
            newErrors.AddRange(errors);
            var newResult = new Result<T>();
            newResult.errors = newErrors;
            return newResult;
        }
    }

    public static class Result
    {
        public static Result<bool> Success = new Result<bool>(true);

        public static Result<bool> Error(Exception error)
        {
            return new Result<bool>().WithError(error);
        }

        public static Result<bool> Error(string errorMessage)
        {
            return new Result<bool>().WithError(new Exception(errorMessage));
        }

        public static Result<bool> Error(IEnumerable<Exception> errors)
        {
            return new Result<bool>().WithError(errors);
        }

        public static Result<T> Error<T>(Exception error)
        {
            return new Result<T>().WithError(error);
        }
    }

    public static class ExceptionExtensions
    {
        public static Exception MakeJustOneException(this IEnumerable<Exception> exceptions)
        {
            if (exceptions.Count() == 1)
                return exceptions.Single();

            return new AggregateException(exceptions);
        }
    }
}
