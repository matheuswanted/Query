using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Query
{
    public class Query<T> : IQueryable<T>
    {
        public Type ElementType => throw new NotImplementedException();

        public Expression Expression => throw new NotImplementedException();

        public IQueryProvider Provider => throw new NotImplementedException();

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public class QueryProvider<T> where T : IDbMetadata, new()
    {
        private string _tableName;
        private StringBuilder _builder;
        private const string AND = " AND ";
        private const string OR = " OR ";
        private const string WHERE = " WHERE ";
        private const string SELECT = "SELECT";
        private const string ALL = " *";
        private const string FROM = " FROM ";

        public QueryProvider()
        {
            _tableName = typeof(T).GetCustomAttributes(false).OfType<TableNameAttribute>().Single().TableName;
            _builder = new StringBuilder();
        }
        public QueryProvider<T> Select()
        {
            _builder.Append(SELECT).Append(ALL).Append(FROM).Append(_tableName);
            return this;
        }
        public QueryProvider<T> Select(Expression<Func<T, dynamic>> exp)
        {
            _builder.Append(SELECT);
            var ctor = ((exp as LambdaExpression).Body as NewExpression);
            string delimiter = " " + _tableName + ".";
            bool first = true;
            foreach (var member in ctor.Arguments)
            {
                switch (member)
                {
                    case MemberExpression e:
                        _builder.Append(delimiter).Append(GetAttribute<TableColumnAttribute>(e.Member).ColumnName);
                        break;
                }

                if (first)
                    delimiter = ", " + _tableName + ".";
                first = false;
            }
            if (!ctor.Arguments.Any())
                _builder.Append(ALL);
            _builder.Append(FROM).Append(_tableName);

            return this;
        }

        public QueryProvider<T> Where(Expression<Func<T, bool>> exp)
        {
            _builder.Append(WHERE);

            Resolve(exp.Body);

            return this;
        }
        public void Resolve(BinaryExpression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.OrElse:
                    ResolvePredicate(exp, OR);
                    break;
                case ExpressionType.AndAlso:
                    ResolvePredicate(exp, AND);
                    break;
                default:
                    ResolveEquality(exp, GetOperator(exp.NodeType));
                    break;
            }
        }

        private void ResolveEquality(BinaryExpression exp, string op)
        {
            Resolve(exp.Left);
            _builder.Append(op);
            Resolve(exp.Right);
        }

        private string GetOperator(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    return " = ";
                default:
                    throw new NotImplementedException();
            }
        }

        private void ResolvePredicate(BinaryExpression exp, string predicate)
        {
            ResovePredicateSide(exp.Left);

            _builder.Append(predicate);

            ResovePredicateSide(exp.Right);
        }

        private void ResovePredicateSide(Expression exp)
        {
            _builder.Append("(");
            Resolve(exp);
            _builder.Append(")");
        }

        private static bool PredicateExpression(BinaryExpression exp)
        {
            return exp.NodeType == ExpressionType.AndAlso
                || exp.NodeType == ExpressionType.OrElse;
        }
        public void Resolve(MemberExpression exp)
        {
            var paramAtt = GetAttributeOrDefault<TableColumnAttribute>(exp.Member);
            var tableAtt = GetAttributeOrDefault<TableNameAttribute>(exp.Member.DeclaringType);
            if (paramAtt != null)
                _builder.AppendFormat("{0}.{1}", tableAtt.TableName, paramAtt.ColumnName);
            else
                _builder.Append(exp.Member.Name.ToUpper());
        }
        public void Resolve(ConstantExpression exp)
        {
            if (exp.Type.IsPrimitive || exp.Type == typeof(string))
                _builder.Append(exp);
            else throw new NotSupportedException($"There is no matching DbType for type {exp.Type.FullName}");
        }
        public void Resolve(Expression exp)
        {
            switch (exp)
            {
                case BinaryExpression e:
                    Resolve(e);
                    break;
                case MemberExpression e:
                    Resolve(e);
                    break;
                case ConstantExpression e:
                    Resolve(e);
                    break;
                default:
                    throw new NotImplementedException("Expression type invalid");
            }
        }

        public string ResolveMemberExpressionColumName(MemberExpression exp)
        {
            return GetAttribute<TableColumnAttribute>(exp.Member).ColumnName;
        }
        public TAttribute GetAttribute<TAttribute>(ICustomAttributeProvider attributeProvider)
            => attributeProvider.GetCustomAttributes(false).OfType<TAttribute>().Single();
        public TAttribute GetAttributeOrDefault<TAttribute>(ICustomAttributeProvider attributeProvider)
            => attributeProvider.GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();

        public string AsSql()
        {
            return _builder.ToString();
        }
    }
}
