using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Query;

namespace TestQuery
{
    [TestClass]
    public class UnitTest1
    {
        const string selectAllFromExams = @"SELECT * FROM EXAMES";
        const string selectIdsFromExams = @"SELECT EXAMES.EXAMES_ID FROM EXAMES";
        const string selectIdsFromExamsWhereCrazyWhere = "SELECT * FROM EXAMES WHERE (EXAMES.EXAMES_ID = 10) AND (((EXAMES.EXAMES_ID = 5) AND (EXAMES.DESCRIPTION = \"x\")) AND (EXAMES.EXAMES_ID = 10))";
        [TestMethod]
        public void QueryProviderOfExameMetadata_ShouldReturnASelectForExamesTable()
        {
            var sql = new QueryProvider<ExameMetadata>().Select().AsSql();
            Assert.AreEqual(selectAllFromExams, sql);
        }
        [TestMethod]
        public void QueryProviderSelectingId_ShouldReturnASelectIdFromTable()
        {
            var sql = new QueryProvider<ExameMetadata>().Select(e => new { e.Id }).AsSql();
            Assert.AreEqual(selectIdsFromExams, sql);
        }
        [TestMethod]
        public void QueryProviderWhereId_ShouldReturnASelectIdEqualsTable()
        {
            var sql = new QueryProvider<ExameMetadata>()
                .Select()
                .Where(e => e.Id == 10 || (e.Id == 5 && e.Description == "x") && e.Id == 10)
                .AsSql();
            Assert.AreEqual(selectIdsFromExamsWhereCrazyWhere, sql);
        }
    }
}
