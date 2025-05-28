using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class ExpressionExtensions
{
    public static string GetMemberName(this Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            return GetMemberName(memberExpression);
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
            return newExpression.Arguments.Select(GetMemberName).ToList();
        }

        return new List<string>();
    }

    private static string GetMemberName(UnaryExpression unaryExpression)
    {
        if (unaryExpression.Operand is MethodCallExpression methodExpression)
        {
            return methodExpression.Method.Name;
        }

        if (unaryExpression.Operand is MemberExpression memberExpression)
        {
            return GetMemberName(memberExpression);
        }

        return null;
    }

    private static string GetMemberName(MemberExpression memberExpression)
    {
        var path = new Stack<string>();
        Expression current = memberExpression;
        while (current is MemberExpression m)
        {
            path.Push(m.Member.Name);
            current = m.Expression;
        }
        return string.Join(".", path);
    }
}
