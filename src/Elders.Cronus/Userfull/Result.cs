using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Userfull
{
    public struct Result<T>
    {
        List<Exception> errors;

        public Result(T value)
        {
            this.Value = value;
            this.errors = null;
        }

        private Result(Exception error, T defaultValue)
        {
            this.Value = defaultValue;
            this.errors = new List<Exception>(1) { error };
        }

        private Result(IEnumerable<Exception> errors, T defaultValue)
        {
            this.Value = defaultValue;
            this.errors = new List<Exception>(errors) { };
        }

        public T Value { get; }

        public bool IsSuccessful => errors is null || errors.Any() == false;

        public bool IsNotSuccessful => IsSuccessful == false;

        public IEnumerable<Exception> Errors
        {
            get
            {
                return errors is null ? Enumerable.Empty<Exception>() : errors;
            }
        }

        public Result<T> WithError(Exception error)
        {
            var newErrors = errors is null ? new List<Exception>() : new List<Exception>(errors);
            newErrors.Add(error);
            var newResult = new Result<T>();
            newResult.errors = newErrors;
            return newResult;
        }

        public Result<T> WithError(string errorMessage)
        {
            var newErrors = errors is null ? new List<Exception>() : new List<Exception>(errors);
            newErrors.Add(new Exception(errorMessage));
            var newResult = new Result<T>();
            newResult.errors = newErrors;
            return newResult;
        }

        public Result<T> WithError(IEnumerable<Exception> errors)
        {
            var newErrors = this.errors is null ? new List<Exception>() : new List<Exception>(this.errors);
            newErrors.AddRange(errors);
            var newResult = new Result<T>();
            newResult.errors = newErrors;
            return newResult;
        }

        public static Result<T> FromError(string errorMessage, T defaultValue = default)
        {
            return FromError(new Exception(errorMessage), defaultValue);
        }

        public static Result<T> FromError(Exception exception, T defaultValue = default)
        {
            return new Result<T>(exception, defaultValue);
        }

        public static Result<T> FromError(IEnumerable<Exception> exceptions, T defaultValue = default)
        {
            return new Result<T>(exceptions, defaultValue);
        }
    }

    public static class Result
    {
        public static Result<bool> Success = new Result<bool>(true);

        public static Result<bool> Error(Exception error)
        {
            return Result<bool>.FromError(error, false);
        }

        public static Result<bool> Error(string errorMessage)
        {
            return Error(new Exception(errorMessage));
        }

        public static Result<bool> Error(IEnumerable<Exception> errors)
        {
            return Result<bool>.FromError(errors, false);
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
