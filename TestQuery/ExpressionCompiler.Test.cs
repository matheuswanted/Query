using Microsoft.VisualStudio.TestTools.UnitTesting;
using Query;
using Query.Compiler;
using Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestQuery
{
    [TestClass]
    public class ExpressionCompilerTest
    {
        ExpressionCompiler _compiler;
        [TestInitialize]
        public void Initialize()
        {
            _compiler = new ExpressionCompiler();
        }
        public SelectExpression SelectAll()
        {
            var select = new SelectExpression();
            var all = new AllExpression();
            select.Project(all);
            return select;
        }
        public void AssertMalformedExpressionException(Action method)
        {
            try
            {
                method.Invoke();
            }
            catch (MalformedExpressionException)
            {
                return;
            }
            Assert.Fail();
        }
        [TestMethod]
        public void Compile_ShouldThrowException_WhenQueryRootIsNull()
        {
            AssertMalformedExpressionException(() => _compiler.Compile(new QueryExpression()));
        }
        [TestMethod]
        public void Compile_ShouldThrowException_WhenFromExpressionSelectIsNull()
        {
            AssertMalformedExpressionException(() => _compiler.Compile(new FromExpression()));
        }
        [TestMethod]
        public void Compile_ShouldReturnSelectAllSql_WhenCompilingASelectAllExpression()
        {
            var select = SelectAll();
            _compiler.Compile(select);
            Assert.AreEqual("SELECT * ", _compiler.Compiled());
        }
        [TestMethod]
        public void Compile_ShouldReturnColumnsMetadata_WhenCompilingANewExpressionWithMetadata()
        {

            Expression<Func<ExameMetadata, dynamic>> f = e => new
            {
                Id = e.Id
            };
            var select = ToSelectExpression(f);
            _compiler.CompileProjection((f as LambdaExpression).Body as NewExpression);
            Assert.AreEqual(" EXAMES.EXAMES_ID AS Id ", _compiler.Compiled());
        }
        [TestMethod]
        public void TestThousandExecutionOf_Compile_ShouldReturnColumnsMetadata_WhenCompilingANewExpressionWithMetadata()
        {
            for (int i = 0; i < 1000; i++)
                Compile_ShouldReturnColumnsMetadata_WhenCompilingANewExpressionWithMetadata();
        }

        private SelectExpression ToSelectExpression(Expression<Func<ExameMetadata, dynamic>> f)
        {
            var s = new SelectExpression();
            s.Project((f as LambdaExpression).Body);
            return s;
        }
    }
}
