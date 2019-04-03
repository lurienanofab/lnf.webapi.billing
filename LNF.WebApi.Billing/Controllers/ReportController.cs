using LNF.Billing;
using LNF.Models.Billing;
using LNF.Models.Billing.Reports;
using LNF.Models.Billing.Reports.ServiceUnitBilling;
using LNF.Repository;
using LNF.Repository.Billing;
using LNF.Repository.Data;
using LNF.WebApi.Billing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    /// <summary>
    /// Provides endpoints for generating billing reports
    /// </summary>
    public class ReportController : ApiController
    {
        protected IApportionmentManager ApportionmentManager => ServiceProvider.Current.Billing.ApportionmentManager;
        protected IBillingTypeManager BillingTypeManager => ServiceProvider.Current.BillingTypeManager;

        /// <summary>
        /// Send the monthly User Apportionment reminder via email. The return value is the number of emails sent
        /// </summary>
        /// <param name="options">Options used for creating and sending the report</param>
        /// <returns>The number of emails sent</returns>
        [HttpPost, Route("report/user-apportionment")]
        public SendMonthlyApportionmentEmailsProcessResult SendUserApportionmentReport([FromBody] UserApportionmentReportOptions options)
        {
            using (DA.StartUnitOfWork())
                return ApportionmentManager.SendMonthlyApportionmentEmails(options);
        }

        [HttpGet, Route("report/user-apportionment/view")]
        public IEnumerable<ReportEmail> ViewUserApportionmentReport(DateTime period, string message = null, bool noEmail = false)
        {
            using (DA.StartUnitOfWork())
            {
                var options = new UserApportionmentReportOptions
                {
                    Period = period,
                    Message = message,
                    NoEmail = noEmail
                };

                return ApportionmentManager.GetMonthlyApportionmentEmails(options);
            }
        }

        [HttpPost, Route("report/user-apportionment/view")]
        public IEnumerable<ReportEmail> ViewUserApportionmentReport([FromBody] UserApportionmentReportOptions options)
        {
            using (DA.StartUnitOfWork())
                return ApportionmentManager.GetMonthlyApportionmentEmails(options);
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
                return FinancialManagerUtility.SendMonthlyUserUsageEmails(options);
        }

        [HttpGet, Route("report/financial-manager/view")]
        public IEnumerable<ReportEmail> ViewFinancialManagerReport(DateTime period, int clientId = 0, int managerOrgId = 0, string message = null, bool includeManager = true)
        {
            using (DA.StartUnitOfWork())
            {
                var options = new FinancialManagerReportOptions
                {
                    Period = period,
                    ClientID = clientId,
                    ManagerOrgID = managerOrgId,
                    Message = message,
                    IncludeManager = includeManager
                };

                return FinancialManagerUtility.GetMonthlyUserUsageEmails(options);
            }
        }

        [HttpPost, Route("report/financial-manager/view")]
        public IEnumerable<ReportEmail> ViewFinancialManagerReport([FromBody] FinancialManagerReportOptions options)
        {
            using (DA.StartUnitOfWork())
                return FinancialManagerUtility.GetMonthlyUserUsageEmails(options);
        }

        [Route("report/billing-summary")]
        public IEnumerable<BillingSummaryItem> GetBillingSummary(DateTime sd, DateTime ed, bool includeRemote = false, int clientId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                ChargeType[] chargeTypes = DA.Current.Query<ChargeType>().ToArray();

                //get all usage during the date range
                var toolUsage = DA.Current.Query<ToolBilling>().Where(x => x.Period >= sd && x.Period < ed).ToArray();
                var roomUsage = DA.Current.Query<RoomBilling>().Where(x => x.Period >= sd && x.Period < ed).ToArray();
                var storeUsage = DA.Current.Query<StoreBilling>().Where(x => x.Period >= sd && x.Period < ed).ToArray();
                var miscUsage = DA.Current.Query<MiscBillingCharge>().Where(x => x.Period >= sd && x.Period < ed).ToArray();

                var result = chargeTypes.Select(x =>
                {
                    decimal total = 0;

                    total += toolUsage.Where(u => u.ChargeTypeID == x.ChargeTypeID && (u.BillingTypeID != BillingTypeManager.Remote.BillingTypeID || includeRemote)).Sum(s => BillingTypeManager.GetLineCost(s));
                    total += roomUsage.Where(u => u.ChargeTypeID == x.ChargeTypeID && (u.BillingTypeID != BillingTypeManager.Remote.BillingTypeID || includeRemote)).Sum(s => BillingTypeManager.GetLineCost(s));
                    total += storeUsage.Where(u => u.ChargeTypeID == x.ChargeTypeID).Sum(s => s.GetLineCost());
                    total += miscUsage.Where(u => u.Account.Org.OrgType.ChargeType.ChargeTypeID == x.ChargeTypeID).Sum(s => s.GetLineCost());

                    var item = new BillingSummaryItem()
                    {
                        StartDate = sd,
                        EndDate = ed,
                        ClientID = clientId,
                        ChargeTypeID = x.ChargeTypeID,
                        ChargeTypeName = x.ChargeTypeName,
                        IncludeRemote = includeRemote
                    };

                    item.TotalCharge = total;

                    return item;
                }).ToArray();

                return result;
            }
        }

        [Route("report/tool/sub")]
        public ToolSUB GetToolSUB(DateTime sd, DateTime ed, int id = 0)
        {
            using (DA.StartUnitOfWork())
            {
                ToolSUB report = new ToolSUB() { StartPeriod = sd, EndPeriod = ed, ClientID = id };
                ToolServiceUnitBillingGenerator.Create(report).Generate();
                return report;
            }
        }

        [Route("report/room/sub")]
        public RoomSUB GetRoomSUB(DateTime sd, DateTime ed, int id = 0)
        {
            using (DA.StartUnitOfWork())
            {
                RoomSUB report = new RoomSUB() { StartPeriod = sd, EndPeriod = ed, ClientID = id };
                RoomServiceUnitBillingGenerator.Create(report).Generate();
                return report;
            }
        }

        [Route("report/store/sub")]
        public StoreSUB GetStoreSUB(DateTime sd, DateTime ed, int id = 0, string option = null)
        {
            using (DA.StartUnitOfWork())
            {
                var twoCreditAccounts = false;

                if (!string.IsNullOrEmpty(option))
                    twoCreditAccounts = option == "two-credit-accounts";

                StoreSUB report = new StoreSUB() { StartPeriod = sd, EndPeriod = ed, ClientID = id, TwoCreditAccounts = twoCreditAccounts };
                StoreServiceUnitBillingGenerator.Create(report).Generate();
                return report;
            }
        }

        [Route("report/tool/ju/{type}")]
        public ToolJU GetToolJU(DateTime sd, DateTime ed, string type, int id = 0)
        {
            using (DA.StartUnitOfWork())
            {
                ToolJU report = new ToolJU() { StartPeriod = sd, EndPeriod = ed, ClientID = id, JournalUnitType = ReportUtility.StringToEnum<JournalUnitTypes>(type) };
                ToolJournalUnitGenerator.Create(report).Generate();
                return report;
            }
        }

        [Route("report/room/ju/{type}")]
        public RoomJU GetRoomJU(DateTime sd, DateTime ed, string type, int id = 0)
        {
            using (DA.StartUnitOfWork())
            {
                RoomJU report = new RoomJU() { StartPeriod = sd, EndPeriod = ed, ClientID = id, JournalUnitType = ReportUtility.StringToEnum<JournalUnitTypes>(type) };
                RoomJournalUnitGenerator.Create(report).Generate();
                return report;
            }
        }

        [Route("report/regular-exception")]
        public IEnumerable<RegularExceptionItem> GetRegularExceptions(DateTime period, int clientId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                IQueryable<RegularException> query;

                if (clientId > 0)
                    query = DA.Current.Query<RegularException>().Where(x => x.Period == period && x.ClientID == clientId);
                else
                    query = DA.Current.Query<RegularException>().Where(x => x.Period == period);

                var result = query.CreateRegularExceptionItems();

                return result;
            }
        }
    }
}
