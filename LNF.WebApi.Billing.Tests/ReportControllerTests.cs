using System;
using System.Linq;
using LNF.Models.Billing.Reports;
using LNF.WebApi.Billing.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LNF.WebApi.Billing.Tests
{
    [TestClass]
    public class ReportControllerTests : TestBase
    {
        [TestMethod]
        public void CanViewApportionmentEmails()
        {
            var controller = new ReportController();

            var opts = new UserApportionmentReportOptions
            {
                Period = DateTime.Parse("2019-03-01"),
                Message = "Please note: Due to a system issue this email was sent two days late. Please try to apportion your room charges today if possible. If you cannot, please let Sandrine (sandrine@umich.edu) know and she can do this for you.",
                NoEmail = false
            };

            var emails = controller.GetUserApportionmentReportEmails(opts);
            Assert.AreEqual(76, emails.Count());
            Assert.AreEqual("lnf-billing@umich.edu", emails.First().FromAddress);
            Assert.AreEqual("lnf-it@umich.edu", emails.First().CcAddress.First());
            Assert.IsTrue(emails.First().Body.Contains(opts.Message));
        }
    }
}
