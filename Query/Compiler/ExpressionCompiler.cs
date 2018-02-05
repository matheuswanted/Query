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

        private string GetColumnName(MemberExpression e)
            => e.Member.GetAttribute<TableColumnAttribute>().ColumnName;
        private string GetTableName(MemberExpression e)
            => e.Member.DeclaringType.GetAttribute<TableNameAttribute>().TableName;
        private string GetTableName(Type type)
            => type.GetAttribute<TableNameAttribute>().TableName;
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
                    _builder.Append(",");
                CompileMember(query.Arguments[i], query.Members[i]);
            }
            _builder.Append(" ");
        }
        public void CompileMember(Expression exp, MemberInfo alias = null)
        {
            switch (exp)
            {
                case MemberExpression e:
                    if (alias != null)
                        _builder.AppendFormat(" {0}.{1} AS \"{2}\"", GetTableName(e), GetColumnName(e), alias.Name);
                    else
                        _builder.AppendFormat("{0}.{1}", GetTableName(e), GetColumnName(e));
                    break;
                case ConstantExpression e:
                    if (alias != null)
                        _builder.AppendFormat(" {0} AS \"{1}\"", e.Value, alias.Name);
                    else if (e.Type == typeof(string))
                        _builder.AppendFormat("'{0}'", e.Value);
                    else
                        _builder.Append(e.Value);
                    break;
                default: throw new NotSupportedException($"Expression type {exp.GetType().Name} is not supported!");
            }
        }

        public void CompileProjection(MemberInitExpression query) { }
        public void Compile(FromExpression query)
        {
            CheckMalformed(query);
            Compile(query.Select);
            CompileFrom(query);
            Compile(query.Joins);
            Compile(query.Where);
        }
        public void CompileFrom(FromExpression from)
            => _builder.Append(FromExpression.FROM).AppendFormat(" {0} ", GetTableName(from.Table));


        public void Compile(WhereExpression query)
        {
            if (query != null)
            {
                _builder.Append(WhereExpression.WHERE).Append(" ");
                CompileFilter(query.Filter as BinaryExpression);
            }
        }

        public void Compile(IEnumerable<JoinExpression> query) { }
        public void Compile(JoinExpression query) { }
        public void CompileFilter(BinaryExpression exp)
        {
            _builder.Append("(");
            switch (exp.NodeType)
            {
                case ExpressionType.OrElse:
                case ExpressionType.AndAlso:
                    CompilePredicate(exp);
                    break;
                default:
                    CompileEquality(exp);
                    break;
            }
            _builder.Append(")");
        }
        public void CompileEquality(BinaryExpression query)
        {
            CompileMember(query.Left);
            _builder.Append(GetOperator(query.NodeType));
            CompileMember(query.Right);
        }
        public void CompilePredicate(BinaryExpression query)
        {
            CompileFilter(query.Left as BinaryExpression);
            _builder.Append(GetOperator(query.NodeType));
            CompileFilter(query.Right as BinaryExpression);
        }
        private string GetOperator(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.AndAlso: return " AND ";
                case ExpressionType.OrElse: return " OR ";
                case ExpressionType.Equal: return " = ";
                case ExpressionType.NotEqual: return " <> ";
                case ExpressionType.GreaterThan: return " > ";
                case ExpressionType.LessThan: return " < ";
                case ExpressionType.GreaterThanOrEqual: return " >= ";
                case ExpressionType.LessThanOrEqual: return " <= ";
                default: throw new NotImplementedException("Operator is not implemented.");
            }
        }

        public string Compiled()
        {
            var result = _builder.ToString();
            _builder.Clear();
            return result;
        }
    }

}