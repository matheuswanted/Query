using Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Query.Compiler
{
    public class MalformedExpressionException : ApplicationException
    {

    }
    public static class AttributeProviderExtensions
    {

        public static TAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider)
            => attributeProvider.GetCustomAttributes(false).OfType<TAttribute>().Single();
        public static TAttribute GetAttributeOrDefault<TAttribute>(this ICustomAttributeProvider attributeProvider)
            => attributeProvider.GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();

    }
    public class ExpressionCompiler : IExpressionCompiler<string>
    {
        private StringBuilder _builder;
        public ExpressionCompiler()
        {
            _builder = new StringBuilder();
        }

        private void CheckMalformed(Expression exp)
        {
            if (exp == null)
                throw new MalformedExpressionException();
        }
        public string Compile(QueryExpression query)
        {
            Compile(query.QueryRoot);
            Compile(query.Group);
            Compile(query.Order);
            return _builder.ToString();
        }
        public void Compile(SelectExpression query)
        {
            CheckMalformed(query);
            _builder.Append(SelectExpression.SELECT);
            if (query.Projection is AllExpression)
                _builder.Append(AllExpression.ALL);
            else if (query.Projection is NewExpression)
                CompileProjection(query.Projection as NewExpression);
            else if (query.Projection is MemberInitExpression)
                CompileProjection(query.Projection as MemberInitExpression);
        }
        public void Compile(GroupExpression query) { }
        public void Compile(OrderExpression query) { }
        public void CompileProjection(NewExpression query)
        {
            int count = query.Arguments.Count;

            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    _builder.Append(", ");
                switch (query.Arguments[0])
                {
                    case MemberExpression e:
                        _builder.AppendFormat(" {0}.{1} AS {2}", e.Member.DeclaringType.GetAttribute<TableNameAttribute>().TableName, e.Member.GetAttribute<TableColumnAttribute>().ColumnName, query.Members[0].Name);
                        break;
                }

            }
            _builder.Append(" ");
        }
        public void CompileProjection(MemberInitExpression query) { }
        public void Compile(FromExpression query)
        {
            CheckMalformed(query);
            Compile(query.Select);
            Compile(query.Joins);
            Compile(query.Where);
        }
        public void Compile(WhereExpression query) { }

        public void Compile(IEnumerable<JoinExpression> query) { }
        public void Compile(JoinExpression query) { }
        public void CompileFilter(BinaryExpression query) { }
        public void CompileEquality(BinaryExpression query) { }
        public void CompilePredicate(BinaryExpression query) { }
        public string Compiled()
        { 
            var result = _builder.ToString();
            _builder.Clear();
            return result;
        }
    }

}