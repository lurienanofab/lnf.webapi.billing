using LNF.Billing;
using LNF.Models.Billing;
using LNF.Models.Billing.Reports;
using LNF.Models.Billing.Reports.ServiceUnitBilling;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    /// <summary>
    /// Provides endpoints for generating billing reports
    /// </summary>
    public class ReportController : ApiController
    {
        protected IApportionmentManager ApportionmentManager => ServiceProvider.Current.Billing.Apportionment;
        protected IBillingTypeManager BillingTypeManager => ServiceProvider.Current.Billing.BillingType;

        /// <summary>
        /// Send the monthly User Apportionment reminder via email. The return value is the number of emails sent
        /// </summary>
        /// <param name="options">Options used for creating and sending the report</param>
        /// <returns>The number of emails sent</returns>
        [HttpPost, Route("report/user-apportionment")]
        public SendMonthlyApportionmentEmailsProcessResult SendUserApportionmentReport([FromBody] UserApportionmentReportOptions options)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.SendUserApportionmentReport(options);
        }

        [HttpGet, Route("report/user-apportionment/view")]
        public IEnumerable<ReportEmail> GetUserApportionmentReportEmails(DateTime period, string message = null, bool noEmail = false)
        {
            return GetUserApportionmentReportEmails(new UserApportionmentReportOptions
            {
                Period = period,
                Message = message,
                NoEmail = noEmail
            });
        }

        [HttpPost, Route("report/user-apportionment/view")]
        public IEnumerable<ReportEmail> GetUserApportionmentReportEmails([FromBody] UserApportionmentReportOptions options)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.GetUserApportionmentReportEmails(options);
        }

        /// <summary>
        /// Send the monthly Financial Manager report via email. The return value is the number of emails sent
        /// </summary>
        /// <param name="options">Options used for creating and sending the report</param>
        /// <returns>The number of emails sent</returns>
        [HttpPost, Route("report/financial-manager")]
        public SendMonthlyUserUsageEmailsProcessResult SendFinancialManagerReport([FromBody] FinancialManagerReportOptions options)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.SendFinancialManagerReport(options);                
        }

        [HttpGet, Route("report/financial-manager/view")]
        public IEnumerable<ReportEmail> GetFinancialManagerReportEmails(DateTime period, int clientId = 0, int managerOrgId = 0, string message = null, bool includeManager = true)
        {
            return GetFinancialManagerReportEmails(new FinancialManagerReportOptions
            {
                Period = period,
                ClientID = clientId,
                ManagerOrgID = managerOrgId,
                Message = message,
                IncludeManager = includeManager
            });
        }

        [HttpPost, Route("report/financial-manager/view")]
        public IEnumerable<ReportEmail> GetFinancialManagerReportEmails([FromBody] FinancialManagerReportOptions options)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.GetFinancialManagerReportEmails(options);
        }

        [Route("report/billing-summary")]
        public IEnumerable<IBillingSummary> GetBillingSummary(DateTime sd, DateTime ed, bool includeRemote = false, int clientId = 0)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.GetBillingSummary(sd, ed, includeRemote, clientId);
        }

        [Route("report/tool/sub")]
        public ToolSUB GetToolSUB(DateTime sd, DateTime ed, int id = 0)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.GetToolSUB(sd, ed, id);
        }

        [Route("report/room/sub")]
        public RoomSUB GetRoomSUB(DateTime sd, DateTime ed, int id = 0)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.GetRoomSUB(sd, ed, id);
        }

        [Route("report/store/sub")]
        public StoreSUB GetStoreSUB(DateTime sd, DateTime ed, int id = 0, string option = null)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.GetStoreSUB(sd, ed, id, option);
        }

        [Route("report/tool/ju/{type}")]
        public ToolJU GetToolJU(DateTime sd, DateTime ed, string type, int id = 0)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.GetToolJU(sd, ed, type, id);
        }

        [Route("report/room/ju/{type}")]
        public RoomJU GetRoomJU(DateTime sd, DateTime ed, string type, int id = 0)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.GetRoomJU(sd, ed, type, id);
        }

        [Route("report/regular-exception")]
        public IEnumerable<IRegularException> GetRegularExceptions(DateTime period, int clientId = 0)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Report.GetRegularExceptions(period, clientId);
        }
    }
}
