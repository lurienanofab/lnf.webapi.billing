using LNF.Models.Billing;
using LNF.Models.Billing.Reports.ServiceUnitBilling;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LNF.WebApi.Billing.Models
{
    public abstract class ServiceUnitBillingGenerator<T> : ReportGenerator<T> where T : ServiceUnitBillingReport
    {
        protected ServiceUnitBillingGenerator(T report):base(report) { }

        protected override void LoadReportItems(DataView dv)
        {
            List<ServiceUnitBillingReportItem> items = new List<ServiceUnitBillingReportItem>();

            foreach (DataRowView drv in dv)
            {
                var i = CreateServiceUnitBillingReportItem(drv);
                items.Add(i);
            }

            AddCombinedItems(items);
            AddItems(items);
        }

        //this should be called by the inheriting class in the GenerateDataTablesForSUB override
        protected override void ProcessTable(DataTable dtBilling)
        {
            DataTable dtReport = InitTable();

            string deptRefNum = string.Empty;
            double chargeAmount = 0;
            double subsidyDiscount = 0;
            double total = 0;

            BillingUnit summary = new BillingUnit();

            //for loop each record in ClientID and AccountID aggregate
            foreach (DataRow cadr in ClientAccountData.Rows)
            {
                if (cadr.RowState != DataRowState.Deleted)
                {
                    chargeAmount = Math.Round(RepositoryUtility.ConvertTo(dtBilling.Compute("SUM(LineCost)", DataRowFilter(cadr)), 0D), 2);
                    if (dtBilling.Columns.Contains("SubsidyDiscount"))
                        subsidyDiscount = RepositoryUtility.ConvertTo(dtBilling.Compute("SUM(SubsidyDiscount)", DataRowFilter(cadr)), 0D);

                    if (chargeAmount != 0)
                    {
                        //2011-02-08 There will be some unavoidable rounding difference, so we basicaly ignore anything for 1 cent
                        if (Math.Abs(chargeAmount) > 0.01)
                        {
                            DataRow[] billingrows = dtBilling.Select(DataRowFilter(cadr));
                            if (billingrows.Length > 0)
                            {
                                DataRow dr = billingrows[0];
                                string DebitAccount = RepositoryUtility.ConvertTo(dr["Number"], string.Empty);
                                AccountInfo debit_acct = new AccountInfo(DebitAccount);

                                //get manager's name
                                deptRefNum = ManagerName(cadr);

                                DateTime p = RepositoryUtility.ConvertTo(dr["Period"], DateTime.MinValue);
                                DateTime invoice_date = (p.Equals(DateTime.MinValue)) ? Report.EndPeriod.AddMonths(-1) : p;

                                DataRow newdr = dtReport.NewRow();
                                newdr["ReportType"] = ReportUtility.EnumToString(Report.ReportType);
                                newdr["ChargeType"] = ReportUtility.EnumToString(Report.BillingCategory);
                                newdr["Period"] = dr["Period"];
                                newdr["CardType"] = 1;
                                newdr["ShortCode"] = dr["ShortCode"];
                                newdr["Account"] = debit_acct.Account;
                                newdr["FundCode"] = debit_acct.FundCode;
                                newdr["DeptID"] = debit_acct.DeptID;
                                newdr["ProgramCode"] = debit_acct.ProgramCode;
                                newdr["Class"] = debit_acct.Class;
                                newdr["ProjectGrant"] = debit_acct.ProjectGrant;
                                newdr["VendorID"] = "0000456136"; //wtf?
                                newdr["InvoiceDate"] = invoice_date.ToString("yyyy/MM/dd");
                                newdr["InvoiceID"] = GetInvoiceID();
                                newdr["Uniqname"] = dr["UserName"];
                                newdr["DepartmentalReferenceNumber"] = deptRefNum;
                                newdr["ItemDescription"] = GetItemDescription(dr);
                                newdr["QuantityVouchered"] = "1.0000";
                                newdr["CreditAccount"] = CreditAccount;
                                newdr["UsageCharge"] = Math.Round(chargeAmount, 2).ToString("0.00");
                                newdr["SubsidyDiscount"] = Math.Round(subsidyDiscount, 2).ToString("0.00");
                                newdr["BilledCharge"] = Math.Round(chargeAmount - subsidyDiscount, 2).ToString("0.00");
                                newdr["UnitOfMeasure"] = Math.Round(chargeAmount, 5).ToString("0.00000");
                                newdr["MerchandiseAmount"] = newdr["UsageCharge"];

                                //Used to calculate the total credit amount
                                total += chargeAmount;

                                //for testing purpose
                                newdr["AccountID"] = cadr["AccountID"];

                                dtReport.Rows.Add(newdr);
                            }
                        }
                    }
                }
            }

            _ReportTables.Add(dtReport);

            //Summary row
            AccountInfo credit_acct = new AccountInfo(CreditAccount);
            summary.CardType = 1;
            summary.ShortCode = CreditAccountShortCode;
            summary.Account = credit_acct.Account;
            summary.FundCode = credit_acct.FundCode;
            summary.DeptID = credit_acct.DeptID;
            summary.ProgramCode = credit_acct.ProgramCode;
            summary.ClassName = credit_acct.Class;
            summary.ProjectGrant = credit_acct.ProjectGrant;
            summary.InvoiceDate = Report.EndPeriod.AddMonths(-1).ToString("yyyy/MM/dd");
            summary.Uniqname = "doscar"; //wtf?
            summary.DepartmentalReferenceNumber = deptRefNum;
            summary.ItemDescription = "doscar"; //wtf?
            summary.MerchandiseAmount = -total;
            summary.CreditAccount = CreditAccount;
            summary.QuantityVouchered = "1.0000";
            AddSummary(summary);

            double SumUsageCharge = 0;
            double SumSubsidyDiscount = 0;
            double SumBilledCharge = 0;

            foreach (DataRow dr in dtReport.Rows)
            {
                SumUsageCharge += RepositoryUtility.ConvertTo(dr["UsageCharge"], 0D);
                SumSubsidyDiscount += RepositoryUtility.ConvertTo(dr["SubsidyDiscount"], 0D);
                SumBilledCharge += RepositoryUtility.ConvertTo(dr["BilledCharge"], 0D);
                if (Report.BillingCategory == BillingCategory.Store)
                {
                    dr["UsageCharge"] = string.Empty;
                    dr["SubsidyDiscount"] = string.Empty;
                }
            }

            DataRow totalrow = dtReport.NewRow();
            totalrow["ReportType"] = ReportUtility.EnumToString(Report.ReportType);
            totalrow["ChargeType"] = ReportUtility.EnumToString(Report.BillingCategory);
            totalrow["Period"] = Report.EndPeriod.AddMonths(-1);
            totalrow["ShortCode"] = dtReport.Rows.Count - 1;
            totalrow["UsageCharge"] = (Report.BillingCategory == BillingCategory.Store) ? string.Empty : SumUsageCharge.ToString("0.00");
            totalrow["SubsidyDiscount"] = (Report.BillingCategory == BillingCategory.Store) ? string.Empty : SumSubsidyDiscount.ToString("0.00");
            totalrow["BilledCharge"] = SumBilledCharge.ToString("0.00");
            dtReport.Rows.Add(totalrow);
        }

        protected override DataTable InitTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ReportType", typeof(string));
            dt.Columns.Add("ChargeType", typeof(string));
            dt.Columns.Add("Period", typeof(DateTime));
            dt.Columns.Add("CardType", typeof(string));
            dt.Columns.Add("ShortCode", typeof(string));
            dt.Columns.Add("Account", typeof(string));
            dt.Columns.Add("FundCode", typeof(string));
            dt.Columns.Add("DeptID", typeof(string));
            dt.Columns.Add("ProgramCode", typeof(string));
            dt.Columns.Add("Class", typeof(string));
            dt.Columns.Add("ProjectGrant", typeof(string));
            dt.Columns.Add("VendorID", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(string));
            dt.Columns.Add("InvoiceID", typeof(string));
            dt.Columns.Add("Uniqname", typeof(string));
            dt.Columns.Add("LocationCode", typeof(string));
            dt.Columns.Add("DeliverTo", typeof(string));
            dt.Columns.Add("VendorOrderNum", typeof(string));
            dt.Columns.Add("DepartmentalReferenceNumber", typeof(string));
            dt.Columns.Add("Trip/EventNumber", typeof(string));
            dt.Columns.Add("ItemID", typeof(string));
            dt.Columns.Add("ItemDescription", typeof(string));
            dt.Columns.Add("VendorItemID", typeof(string));
            dt.Columns.Add("ManufacturerName", typeof(string));
            dt.Columns.Add("ModelNum", typeof(string));
            dt.Columns.Add("SerialNum", typeof(string));
            dt.Columns.Add("UMTagNum", typeof(string));
            dt.Columns.Add("QuantityVouchered", typeof(string));
            dt.Columns.Add("UnitOfMeasure", typeof(string));
            dt.Columns.Add("UnitPrice", typeof(string));
            dt.Columns.Add("MerchandiseAmount", typeof(string));
            dt.Columns.Add("VoucherComment", typeof(string));
            dt.Columns.Add("SubsidyDiscount", typeof(string));
            dt.Columns.Add("BilledCharge", typeof(string));
            dt.Columns.Add("UsageCharge", typeof(string));
            //Not belong in the excel format, but added for other purpose
            dt.Columns.Add("CreditAccount", typeof(string));
            dt.Columns.Add("AccountID", typeof(string));
            return dt;
        }

        private void AddItems(List<ServiceUnitBillingReportItem> items)
        {
            List<ServiceUnitBillingReportItem[]> reportItems;

            if (Report.Items != null)
                reportItems = Report.Items.ToList();
            else
                reportItems = new List<ServiceUnitBillingReportItem[]>();

            reportItems.Add(items.ToArray());

            Report.Items = reportItems.ToArray();
        }

        private void AddCombinedItems(List<ServiceUnitBillingReportItem> items)
        {
            List<ServiceUnitBillingReportItem> combinedItems;

            if (Report.CombinedItems != null)
                combinedItems = Report.CombinedItems.ToList();
            else
                combinedItems = new List<ServiceUnitBillingReportItem>();

            combinedItems.AddRange(items);
            
            Report.CombinedItems = combinedItems.ToArray();
        }

        private void AddSummary(BillingUnit billingUnit)
        {
            List<BillingUnit> summaries;

            if (Report.Summaries != null)
                summaries = Report.Summaries.ToList();
            else
                summaries = new List<BillingUnit>();

            summaries.Add(billingUnit);

            Report.Summaries = summaries.ToArray();
        }

        private string GetInvoiceID()
        {
            return string.Format("SUB {0} {1} LNF {2}"
                , GetServiceUnitBillingNumber()
                , Report.EndPeriod.AddMonths(-1).ToString("MM/yy")
                , ReportUtility.EnumToString(Report.BillingCategory).ToLower()
            );
        }

        private string GetItemDescription(DataRow dr)
        {
            string result = string.Empty;
            string displayName = RepositoryUtility.ConvertTo(dr["DisplayName"], string.Empty);

            switch (Report.BillingCategory)
            {
                case BillingCategory.Room:
                case BillingCategory.Tool:
                    string billingTypeName = RepositoryUtility.ConvertTo(dr["BillingTypeName"], string.Empty);
                    result += ReportUtility.ClipText(displayName, 20);
                    result += "-";
                    result += ReportUtility.ClipText(billingTypeName, 9);
                    break;
                case BillingCategory.Store:
                    result = ReportUtility.ClipText(displayName, 30);
                    break;
            }
            return result;
        }

        private int GetServiceUnitBillingNumber()
        {
            DateTime Period = Report.EndPeriod.AddMonths(-1);
            DateTime July2010 = new DateTime(2010, 7, 1);
            int yearoff = Period.Year - July2010.Year;
            int monthoff = Period.Month - July2010.Month;

            int increment = (yearoff * 12 + monthoff) * 3;

            //263 is the starting number for room sub in July 2010
            if (Report.BillingCategory == BillingCategory.Tool)
                return 263 + increment + 1;
            else if (Report.BillingCategory == BillingCategory.Store)
                return 263 + increment + 2;
            else
                return 263 + increment;
        }

        private ServiceUnitBillingReportItem CreateServiceUnitBillingReportItem(DataRowView drv)
        {
            return new ServiceUnitBillingReportItem()
            {
                ReportType = RepositoryUtility.ConvertTo(drv["ReportType"], string.Empty),
                ChargeType = RepositoryUtility.ConvertTo(drv["ChargeType"], string.Empty),
                Period = RepositoryUtility.ConvertTo(drv["Period"], DateTime.MinValue),
                CardType = RepositoryUtility.ConvertTo(drv["CardType"], string.Empty),
                ShortCode = RepositoryUtility.ConvertTo(drv["ShortCode"], string.Empty),
                Account = RepositoryUtility.ConvertTo(drv["Account"], string.Empty),
                FundCode = RepositoryUtility.ConvertTo(drv["FundCode"], string.Empty),
                DeptID = RepositoryUtility.ConvertTo(drv["DeptID"], string.Empty),
                ProgramCode = RepositoryUtility.ConvertTo(drv["ProgramCode"], string.Empty),
                Class = RepositoryUtility.ConvertTo(drv["Class"], string.Empty),
                ProjectGrant = RepositoryUtility.ConvertTo(drv["ProjectGrant"], string.Empty),
                VendorID = RepositoryUtility.ConvertTo(drv["VendorID"], string.Empty),
                InvoiceDate = RepositoryUtility.ConvertTo(drv["InvoiceDate"], string.Empty),
                InvoiceID = RepositoryUtility.ConvertTo(drv["InvoiceID"], string.Empty),
                Uniqname = RepositoryUtility.ConvertTo(drv["Uniqname"], string.Empty),
                LocationCode = RepositoryUtility.ConvertTo(drv["LocationCode"], string.Empty),
                DeliverTo = RepositoryUtility.ConvertTo(drv["DeliverTo"], string.Empty),
                VendorOrderNum = RepositoryUtility.ConvertTo(drv["VendorOrderNum"], string.Empty),
                DepartmentalReferenceNumber = RepositoryUtility.ConvertTo(drv["DepartmentalReferenceNumber"], string.Empty),
                TripOrEventNumber = RepositoryUtility.ConvertTo(drv["Trip/EventNumber"], string.Empty),
                ItemID = RepositoryUtility.ConvertTo(drv["ItemID"], string.Empty),
                ItemDescription = RepositoryUtility.ConvertTo(drv["ItemDescription"], string.Empty),
                VendorItemID = RepositoryUtility.ConvertTo(drv["VendorItemID"], string.Empty),
                ManufacturerName = RepositoryUtility.ConvertTo(drv["ManufacturerName"], string.Empty),
                ModelNum = RepositoryUtility.ConvertTo(drv["ModelNum"], string.Empty),
                SerialNum = RepositoryUtility.ConvertTo(drv["SerialNum"], string.Empty),
                UMTagNum = RepositoryUtility.ConvertTo(drv["UMTagNum"], string.Empty),
                QuantityVouchered = RepositoryUtility.ConvertTo(drv["QuantityVouchered"], string.Empty),
                UnitOfMeasure = RepositoryUtility.ConvertTo(drv["UnitOfMeasure"], string.Empty),
                UnitPrice = RepositoryUtility.ConvertTo(drv["UnitPrice"], string.Empty),
                MerchandiseAmount = RepositoryUtility.ConvertTo(drv["MerchandiseAmount"], string.Empty),
                VoucherComment = RepositoryUtility.ConvertTo(drv["VoucherComment"], string.Empty),
                SubsidyDiscount = RepositoryUtility.ConvertTo(drv["SubsidyDiscount"], string.Empty),
                BilledCharge = RepositoryUtility.ConvertTo(drv["BilledCharge"], string.Empty),
                UsageCharge = RepositoryUtility.ConvertTo(drv["UsageCharge"], string.Empty),
                CreditAccount = RepositoryUtility.ConvertTo(drv["CreditAccount"], string.Empty),
                AccountID = RepositoryUtility.ConvertTo(drv["AccountID"], string.Empty)
            };
        }
    }
}