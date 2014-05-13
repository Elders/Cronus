using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Elders.Cronus
{
    public delegate TResult LateBoundMethod<TResult>(object target, object[] arguments);

    public static class DelegateFactory
    {
        public static LateBoundMethod<TResult> Create<TResult>(System.Reflection.MethodInfo method)
        {
            Contract.Requires(method != null);

            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              Expression.Convert(instanceParameter, method.DeclaringType),
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<LateBoundMethod<TResult>> lambda = Expression.Lambda<LateBoundMethod<TResult>>(
              Expression.Convert(call, typeof(TResult)),
              instanceParameter,
              argumentsParameter);

            return lambda.Compile();
        }

        public static LateBoundVoidMethod Create(System.Reflection.MethodInfo method)
        {
            Contract.Requires(method != null);

            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              Expression.Convert(instanceParameter, method.DeclaringType),
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<LateBoundVoidMethod> lambda = Expression.Lambda<LateBoundVoidMethod>(
              Expression.Convert(call, typeof(void)),
              instanceParameter,
              argumentsParameter);

            return lambda.Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
              Expression.Convert(
                Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).ToArray();
        }

    }

    public delegate void LateBoundVoidMethod(object target, object[] arguments);

}
