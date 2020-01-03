using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions
{
    public static class ExpressionExtensions
    {
        public static string GetMemberName(this Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            if (expression is UnaryExpression unaryExpression)
            {
                return GetMemberName(unaryExpression);
            }

            return null;
        }

        public static List<string> GetMemberNames(this Expression expression)
        {
            if (expression is NewExpression newExpression)
            {
                return newExpression.Arguments.Select(x => GetMemberName(x)).ToList();
            }

            return new List<string>();
        }

        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression methodExpression)
            {
                return methodExpression.Method.Name;
            }

            return ((MemberExpression)unaryExpression.Operand).Member.Name;
        }
    }
}
