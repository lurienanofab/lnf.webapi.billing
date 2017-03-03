using LNF.Models.Billing;
using LNF.Models.Billing.Reports.ServiceUnitBilling;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Data;

namespace LNF.WebApi.Billing.Models
{
    public abstract class JournalUnitGenerator<T> : ReportGenerator<T> where T : JournalUnitReport, new()
    {
        protected JournalUnitGenerator(T report) : base(report) { }

        protected override void LoadReportItems(DataView dv)
        {
            var items = new List<JournalUnitReportItem>();

            foreach (DataRowView drv in dv)
            {
                var i = CreateJournalUnitReportItem(drv);
                items.Add(i);
            }

            Report.Items = items.ToArray();
        }

        //this should be called by the inheriting class in the GenerateDataTablesForSUB override
        protected override void ProcessTable(DataTable dtBilling)
        {
            DataTable dtReport = InitTable();

            double ChargeAmount = 0;
            string JournalLineRef = string.Empty;
            double SubsidyDiscount = 0;
            double Total = 0;

            //for loop each record in clientID and AccountID aggregate
            foreach (DataRow cadr in ClientAccountData.Rows)
            {
                if (cadr.RowState != DataRowState.Deleted)
                {
                    ChargeAmount = Math.Round(Convert.ToDouble(dtBilling.Compute("SUM(LineCost)", DataRowFilter(cadr))), 2);
                    if (Math.Abs(ChargeAmount) > 0.01)
                    {
                        SubsidyDiscount = RepositoryUtility.ConvertTo(dtBilling.Compute("SUM(SubsidyDiscount)", DataRowFilter(cadr)), 0D);
                        if (ChargeAmount != 0 && SubsidyDiscount != 0)
                        {
                            DataRow[] billingrows = dtBilling.Select(DataRowFilter(cadr));
                            DataRow drBilling = billingrows[0];
                            string DebitAccount = RepositoryUtility.ConvertTo(drBilling["Number"], string.Empty);
                            AccountInfo dai = new AccountInfo(DebitAccount);

                            //get manager's name
                            JournalLineRef = ReportUtility.ClipText(ManagerName(drBilling), 10);

                            switch (Report.JournalUnitType)
                            {
                                case JournalUnitTypes.A:
                                    ProcessJUA(dtReport, drBilling, dai, JournalLineRef, SubsidyDiscount, ref Total);
                                    break;
                                case JournalUnitTypes.B:
                                    ProcessJUB(dtReport, drBilling, dai, JournalLineRef, SubsidyDiscount, ref Total);
                                    break;
                                case JournalUnitTypes.C:
                                    ProcessJUC(dtReport, drBilling, dai, JournalLineRef, SubsidyDiscount, ref Total);
                                    break;
                                default:
                                    throw new ArgumentException("Invalid JournalUnitType. Allowed values: A, B, C");
                            }
                        }
                    }
                }
            }

            _ReportTables.Add(dtReport);

            //Summary row
            AccountInfo cai = new AccountInfo(CreditAccount);
            Report.CreditEntry = new CreditEntry();
            Report.CreditEntry.Account = cai.Account;
            Report.CreditEntry.FundCode = cai.FundCode;
            Report.CreditEntry.DeptID = cai.DeptID;
            Report.CreditEntry.ProgramCode = cai.ProgramCode;
            Report.CreditEntry.ClassName = cai.Class;
            Report.CreditEntry.ProjectGrant = cai.ProjectGrant;
            Report.CreditEntry.DepartmentalReferenceNumber = JournalLineRef;
            Report.CreditEntry.ItemDescription = "doscar";
            Report.CreditEntry.MerchandiseAmount = Math.Round(-Total, 2);
            Report.CreditEntry.DepartmentalReferenceNumber = string.Empty;
            Report.CreditEntry.CreditAccount = CreditAccount;

            DataRow totalrow = dtReport.NewRow();
            totalrow["ReportType"] = ReportUtility.EnumToString(Report.ReportType);
            totalrow["ChargeType"] = ReportUtility.EnumToString(Report.BillingCategory);
            totalrow["JournalUnitType"] = Report.JournalUnitType;
            totalrow["Period"] = Report.EndPeriod.AddMonths(-1);
            totalrow["Account"] = (Report.JournalUnitType == JournalUnitTypes.C) ? "613280" : cai.Account;
            totalrow["FundCode"] = cai.FundCode;
            totalrow["DeptID"] = cai.DeptID;
            totalrow["ProgramCode"] = cai.ProgramCode;
            totalrow["Class"] = cai.Class;
            totalrow["ProjectGrant"] = cai.ProjectGrant;
            totalrow["DepartmentalReferenceNumber"] = string.Empty;
            totalrow["ItemDescription"] = "zzdoscar";
            totalrow["MerchandiseAmount"] = Math.Round(-Total, 2).ToString("0.00");
            dtReport.Rows.Add(totalrow);
        }

        protected override DataTable InitTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ReportType", typeof(string));
            dt.Columns.Add("ChargeType", typeof(string));
            dt.Columns.Add("JournalUnitType", typeof(string));
            dt.Columns.Add("Period", typeof(DateTime));
            dt.Columns.Add("Account", typeof(string));
            dt.Columns.Add("FundCode", typeof(string));
            dt.Columns.Add("DeptID", typeof(string));
            dt.Columns.Add("ProgramCode", typeof(string));
            dt.Columns.Add("Class", typeof(string));
            dt.Columns.Add("ProjectGrant", typeof(string));
            dt.Columns.Add("DepartmentalReferenceNumber", typeof(string));
            dt.Columns.Add("ItemDescription", typeof(string));
            dt.Columns.Add("MerchandiseAmount", typeof(string));
            //Not belong in the excel format, but added for other purpose
            dt.Columns.Add("CreditAccount", typeof(string));
            dt.Columns.Add("AccountID", typeof(string));
            return dt;
        }

        private void ProcessJUA(DataTable dtReport, DataRow drBilling, AccountInfo debit_acct, string JournalLineRef, double SubsidyDiscount, ref double Total)
        {
            string ctabbr = ChargeTypeAbbreviation(Report.BillingCategory);

            if (debit_acct.FundCode == "20000" || debit_acct.FundCode == "25000")
            {
                DataRow newdr = dtReport.NewRow();
                newdr["ReportType"] = ReportUtility.EnumToString(Report.ReportType);
                newdr["ChargeType"] = ReportUtility.EnumToString(Report.BillingCategory);
                newdr["JournalUnitType"] = Report.JournalUnitType;
                newdr["Period"] = drBilling["Period"];
                newdr["Account"] = debit_acct.Account;
                newdr["FundCode"] = debit_acct.FundCode;
                newdr["DeptID"] = debit_acct.DeptID;
                newdr["ProgramCode"] = debit_acct.ProgramCode;
                newdr["Class"] = debit_acct.Class;
                newdr["ProjectGrant"] = debit_acct.ProjectGrant;
                newdr["DepartmentalReferenceNumber"] = JournalLineRef;
                newdr["ItemDescription"] = ReportUtility.ClipText(string.Format("LNF{0}A {1} {2}", ctabbr, drBilling["DisplayName"], drBilling["BillingTypeName"]), 30);
                newdr["MerchandiseAmount"] = (Math.Round(SubsidyDiscount, 2) * -1).ToString("0.00");

                //Used to calculate the total credit amount
                Total += RepositoryUtility.ConvertTo(newdr["MerchandiseAmount"], 0D);

                dtReport.Rows.Add(newdr);

                //2nd record of JU
                newdr = dtReport.NewRow();
                newdr["ReportType"] = ReportUtility.EnumToString(Report.ReportType);
                newdr["ChargeType"] = ReportUtility.EnumToString(Report.BillingCategory);
                newdr["JournalUnitType"] = Report.JournalUnitType;
                newdr["Period"] = RepositoryUtility.ConvertTo(drBilling["Period"], DateTime.MinValue);
                newdr["Account"] = debit_acct.Account;
                newdr["FundCode"] = "10000";
                newdr["DeptID"] = debit_acct.DeptID;
                newdr["ProgramCode"] = debit_acct.ProgramCode;
                newdr["Class"] = debit_acct.Class;
                newdr["ProjectGrant"] = debit_acct.ProjectGrant;
                newdr["DepartmentalReferenceNumber"] = JournalLineRef;
                newdr["ItemDescription"] = ReportUtility.ClipText(string.Format("LNF{0}A {1} {2}", ctabbr, drBilling["DisplayName"], drBilling["BillingTypeName"]), 30);
                newdr["MerchandiseAmount"] = Math.Round(SubsidyDiscount, 2).ToString("0.00");

                dtReport.Rows.Add(newdr);

                //3rd record of JU A
                newdr = dtReport.NewRow();
                newdr["ReportType"] = ReportUtility.EnumToString(Report.ReportType);
                newdr["ChargeType"] = ReportUtility.EnumToString(Report.BillingCategory);
                newdr["JournalUnitType"] = Report.JournalUnitType;
                newdr["Period"] = RepositoryUtility.ConvertTo(drBilling["Period"], DateTime.MinValue);
                newdr["Account"] = "450600";
                newdr["FundCode"] = "10000";
                newdr["DeptID"] = debit_acct.DeptID;
                newdr["ProgramCode"] = debit_acct.ProgramCode;
                newdr["Class"] = debit_acct.Class;
                newdr["ProjectGrant"] = debit_acct.ProjectGrant;
                newdr["DepartmentalReferenceNumber"] = JournalLineRef;
                newdr["ItemDescription"] = ReportUtility.ClipText(string.Format("LNF{0}A {1} {2}", ctabbr, drBilling["DisplayName"], drBilling["BillingTypeName"]), 30);
                newdr["MerchandiseAmount"] = (Math.Round(SubsidyDiscount, 2) * -1).ToString("0.00");

                dtReport.Rows.Add(newdr);
            }
        }

        private void ProcessJUB(DataTable dtReport, DataRow drBilling, AccountInfo debit_acct, string JournalLineRef, double SubsidyDiscount, ref double Total)
        {
            string ctabbr = ChargeTypeAbbreviation(Report.BillingCategory);

            if (debit_acct.FundCode == "10000" && debit_acct.ProgramCode == "CSTSH")
            {
                DataRow newdr = dtReport.NewRow();
                newdr["ReportType"] = ReportUtility.EnumToString(Report.ReportType);
                newdr["ChargeType"] = ReportUtility.EnumToString(Report.BillingCategory);
                newdr["JournalUnitType"] = Report.JournalUnitType;
                newdr["Period"] = drBilling["Period"];
                newdr["Account"] = "450600";
                newdr["FundCode"] = debit_acct.FundCode;
                newdr["DeptID"] = debit_acct.DeptID;
                newdr["ProgramCode"] = debit_acct.ProgramCode;
                newdr["Class"] = debit_acct.Class;
                newdr["ProjectGrant"] = debit_acct.ProjectGrant;
                newdr["DepartmentalReferenceNumber"] = JournalLineRef;
                newdr["ItemDescription"] = ReportUtility.ClipText(string.Format("LNF{0}B {1} {2}", ctabbr, drBilling["DisplayName"], drBilling["BillingTypeName"]), 30);
                newdr["MerchandiseAmount"] = (Math.Round(SubsidyDiscount, 2) * -1).ToString("0.00");

                //Used to calculate the total credit amount
                Total += RepositoryUtility.ConvertTo(newdr["MerchandiseAmount"], 0D);

                dtReport.Rows.Add(newdr);
            }
        }

        private void ProcessJUC(DataTable dtReport, DataRow drBilling, AccountInfo debit_acct, string JournalLineRef, double SubsidyDiscount, ref double Total)
        {
            string ctabbr = ChargeTypeAbbreviation(Report.BillingCategory);

            if (debit_acct.FundCode != "20000" && debit_acct.FundCode != "25000" && !(debit_acct.FundCode == "10000" && debit_acct.ProgramCode == "CSTSH"))
            {
                DataRow newdr = dtReport.NewRow();
                newdr["ReportType"] = ReportUtility.EnumToString(Report.ReportType);
                newdr["ChargeType"] = ReportUtility.EnumToString(Report.BillingCategory);
                newdr["JournalUnitType"] = Report.JournalUnitType;
                newdr["Period"] = drBilling["Period"];
                newdr["Account"] = debit_acct.Account;
                newdr["FundCode"] = debit_acct.FundCode;
                newdr["DeptID"] = debit_acct.DeptID;
                newdr["ProgramCode"] = debit_acct.ProgramCode;
                newdr["Class"] = debit_acct.Class;
                newdr["ProjectGrant"] = debit_acct.ProjectGrant;
                newdr["DepartmentalReferenceNumber"] = JournalLineRef;
                newdr["ItemDescription"] = ReportUtility.ClipText(string.Format("LNF{0}C {1} {2}", ctabbr, drBilling["DisplayName"], drBilling["BillingTypeName"]), 30);
                newdr["MerchandiseAmount"] = (Math.Round(SubsidyDiscount, 2) * -1).ToString("0.00");

                //Used to calculate the total credit amount
                Total += RepositoryUtility.ConvertTo(newdr["MerchandiseAmount"], 0D);

                dtReport.Rows.Add(newdr);
            }
        }

        private static readonly Dictionary<BillingCategory, string> ChargeTypeAbbreviationLookup = new Dictionary<BillingCategory, string>()
        {
            { BillingCategory.Room | BillingCategory.Tool | BillingCategory.Store, "all" },
            { BillingCategory.Room, "rm" },
            { BillingCategory.Tool, "tl" },
            { BillingCategory.Store, "st" }
        };

        private string ChargeTypeAbbreviation(BillingCategory billingCategory)
        {
            return ChargeTypeAbbreviationLookup[billingCategory];
        }

        private JournalUnitReportItem CreateJournalUnitReportItem(DataRowView drv)
        {
            return new JournalUnitReportItem()
            {
                ReportType = RepositoryUtility.ConvertTo(drv["ReportType"], string.Empty),
                ChargeType = RepositoryUtility.ConvertTo(drv["ChargeType"], string.Empty),
                JournalUnitType = RepositoryUtility.ConvertTo(drv["JournalUnitType"], string.Empty),
                Period = RepositoryUtility.ConvertTo(drv["Period"], DateTime.MinValue),
                Account = RepositoryUtility.ConvertTo(drv["Account"], string.Empty),
                FundCode = RepositoryUtility.ConvertTo(drv["FundCode"], string.Empty),
                DeptID = RepositoryUtility.ConvertTo(drv["DeptID"], string.Empty),
                ProgramCode = RepositoryUtility.ConvertTo(drv["ProgramCode"], string.Empty),
                Class = RepositoryUtility.ConvertTo(drv["Class"], string.Empty),
                ProjectGrant = RepositoryUtility.ConvertTo(drv["ProjectGrant"], string.Empty),
                DepartmentalReferenceNumber = RepositoryUtility.ConvertTo(drv["DepartmentalReferenceNumber"], string.Empty),
                ItemDescription = RepositoryUtility.ConvertTo(drv["ItemDescription"], string.Empty),
                MerchandiseAmount = RepositoryUtility.ConvertTo(drv["MerchandiseAmount"], string.Empty),
                CreditAccount = RepositoryUtility.ConvertTo(drv["CreditAccount"], string.Empty),
                AccountID = RepositoryUtility.ConvertTo(drv["AccountID"], string.Empty)
            };
        }
    }
}