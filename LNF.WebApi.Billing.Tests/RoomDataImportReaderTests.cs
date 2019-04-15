using System;
using System.Linq;
using LNF.CommonTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LNF.WebApi.Billing.Tests
{
    [TestClass]
    public class RoomDataImportReaderTests
    {
        [TestMethod]
        public void CanSelectRoomDataImportItems()
        {
            var sd = DateTime.Parse("2019-03-01");
            var ed = DateTime.Parse("2019-04-01");

            using (var reader = new RoomDataImportReader(sd, ed, 0, 0))
            {
                reader.SelectRoomDataImportItems();
                Assert.AreEqual(7551, reader.Items.Count());

                var ds = reader.AsDataSet();
                Assert.AreEqual(4, ds.Tables.Count);
            }
        }
    }
}
