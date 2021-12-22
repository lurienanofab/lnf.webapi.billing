using LNF.Billing;
using LNF.Billing.Process;
using LNF.Impl.Billing;
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
            var controller = new ProcessController(Provider);

            var model = new DataCommand
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

        [TestMethod]
        public void CanWriteRoomDataProcess()
        {
            using (var conn = NewConnection())
            {
                var period = DateTime.Parse("2020-12-01");
                var clientId = 216;
                var record = 0;

                var process = new WriteRoomDataProcess(new WriteRoomDataConfig { Connection = conn, Context = "ProcessControllerTests.CanWriteRoomDataProcess", Period = period, ClientID = clientId, RoomID = record });
                var result = process.Start();
            }
        }
    }
}
