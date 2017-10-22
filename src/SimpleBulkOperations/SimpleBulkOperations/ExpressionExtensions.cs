using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleBulkOperations
{
    public static class ExpressionExtensions
    {
        public static string GetMemberName(this Expression expression)
        {
            if (expression is MemberExpression)
            {
                var memberExpression = (MemberExpression)expression;
                return memberExpression.Member.Name;
            }

            if (expression is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)expression;
                return GetMemberName(unaryExpression);
            }

            throw new ArgumentException("Invalid expression.");
        }

        public static List<string> GetMemberNames(this Expression expression)
        {
            if (expression is NewExpression)
            {
                var newExpression = expression as NewExpression;
                return newExpression.Arguments.Select(x => GetMemberName(x)).ToList();
            }

            throw new ArgumentException("Invalid expression.");
        }

        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression)unaryExpression.Operand;
                return methodExpression.Method.Name;
            }

            return ((MemberExpression)unaryExpression.Operand).Member.Name;
        }
    }
}
