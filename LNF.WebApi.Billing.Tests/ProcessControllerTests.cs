using LNF.Models.Billing;
using LNF.Models.Billing.Process;
using LNF.WebApi.Billing.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LNF.WebApi.Billing.Tests
{
    [TestClass]
    public class ProcessControllerTests : TestBase
    {
        [TestMethod]
        public void CanProcessData()
        {
            var controller = new ProcessController();

            var model = new BillingProcessDataCommand
            {
                BillingCategory = BillingCategory.Tool | BillingCategory.Room | BillingCategory.Store,
                ClientID = 81,
                Period = DateTime.Parse("2019-02-01"),
                Record = 0
            };

            var result = controller.BillingProcessData(model);

            //WriteToolDataProcessResult
            Assert.AreEqual(23, result.WriteToolDataProcessResult.RowsDeleted);
            Assert.AreEqual(23, result.WriteToolDataProcessResult.RowsExtracted);
            Assert.AreEqual(23, result.WriteToolDataProcessResult.RowsLoaded);
            Assert.AreEqual(20402, result.WriteToolDataProcessResult.RowsAdjusted);

            //WriteRoomDataProcessResult
            Assert.AreEqual(1, result.WriteRoomDataProcessResult.DistinctClientRows);
            Assert.AreEqual(9, result.WriteRoomDataProcessResult.RowsDeleted);
            Assert.AreEqual(6, result.WriteRoomDataProcessResult.RowsExtracted);
            Assert.AreEqual(9, result.WriteRoomDataProcessResult.RowsLoaded);
            Assert.AreEqual(8035, result.WriteRoomDataProcessResult.RowsAdjusted);
            Assert.AreEqual(0, result.WriteRoomDataProcessResult.BadEntryRowsDeleted);

            //WriteStoreDataProcessResult
            Assert.AreEqual(0, result.WriteStoreDataProcessResult.RowsDeleted);
            Assert.AreEqual(0, result.WriteStoreDataProcessResult.RowsExtracted);
            Assert.AreEqual(0, result.WriteStoreDataProcessResult.RowsLoaded);
        }
    }
}
