using System;
using LNF.Impl.Billing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LNF.WebApi.Billing.Tests
{
    [TestClass]
    public class BillingDataProcessStep1Tests : TestBase
    {
        [TestMethod]
        public void CanPopulateRoomBilling()
        {
            using (var conn = NewConnection())
            {
                conn.Open();
                var period = DateTime.Parse("2020-12-01");
                var now = DateTime.Now;
                var clientId = 2063; // Matt Shea

                var step1 = new BillingDataProcessStep1(new Step1Config { Connection = conn, Context = "BillingDataProcessStep1Tests.CanPopulateRoomBilling", Period = period, Now = now, ClientID = clientId, IsTemp = false });

                var result = step1.PopulateRoomBilling();
                conn.Close();
            }
        }
    }
}
