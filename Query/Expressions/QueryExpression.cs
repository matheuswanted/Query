using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Query.Expressions
{
    public class SelectExpression : Expression
    {
        internal const string SELECT = "SELECT";
        public Expression Projection { get; internal set; }
        public void Project(Expression exp)
        {
            Projection = exp;
        }
    }
    public class AllExpression : Expression
    {
        internal const string ALL = " * ";
    }
    public class FromExpression : Expression
    {
        internal const string FROM = "FROM";

        public FromExpression(Type t)
        {
            Table = t;
        }
        public Type Table { get; internal set; }
        public SelectExpression Select { get; internal set; }
        public IEnumerable<JoinExpression> Joins { get; internal set; }
        public WhereExpression Where { get; internal set; }
    }
    public class JoinExpression : Expression
    {

    }
    public class GroupExpression : Expression
    {

    }
    public class OrderExpression : Expression
    {

    }
    public class WhereExpression : Expression
    {
        internal const string WHERE = "WHERE";

        public WhereExpression(Expression filter)
            => Filter = filter;
        public Expression Filter { get; internal set; }
    }
    public class SubQueryExpression : Expression
    {
        public FromExpression QueryRoot { get; internal set; }
    }
    public class QueryExpression : SubQueryExpression
    {

        public GroupExpression Group { get; internal set; }
        public OrderExpression Order { get; internal set; }
    }
}
