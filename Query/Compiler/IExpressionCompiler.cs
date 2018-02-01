using Query.Expressions;

namespace Query.Compiler
{
    public interface IExpressionCompiler<T>
    {
        T Compile(QueryExpression query);
    }

}