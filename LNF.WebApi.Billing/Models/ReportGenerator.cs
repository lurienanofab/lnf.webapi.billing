using LNF.Billing;
using LNF.Models.Billing;
using LNF.Models.Billing.Reports.ServiceUnitBilling;
using LNF.Repository;
using LNF.Repository.Data;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LNF.WebApi.Billing.Models
{
    public abstract class ReportGenerator<T> where T : ReportBase
    {
        private DataTable dtManagers;

        protected List<DataTable> _ReportTables = new List<DataTable>();
        protected List<DataView> _ReportViews = new List<DataView>();
        protected string _CreditAccount;
        protected string _CreditAccountShortCode;
        protected DataTable ClientAccountData { get; set; }

        protected T Report { get; private set; }

        protected DataTable ManagersData
        {
            get
            {
                if (dtManagers == null)
                    dtManagers = DataAccess.ClientAccountSelect(new { Action = "AllWithManagerName", sDate = Report.StartPeriod, eDate = Report.EndPeriod });
                return dtManagers;
            }
        }

        public string CreditAccount { get { return _CreditAccount; } }
        public string CreditAccountShortCode { get { return _CreditAccountShortCode; } }

        public ReportGenerator(T report)
        {
            Report = report;

            var gc = DA.Current.Query<GlobalCost>().First();

            _CreditAccountShortCode = gc.LabCreditAccount.ShortCode;

            switch (Report.ReportType)
            {
                case ReportTypes.JU:
                    _CreditAccount = gc.SubsidyCreditAccount.Number;
                    break;
                case ReportTypes.SUB:
                    _CreditAccount = gc.LabCreditAccount.Number;
                    break;
            }
        }

        protected abstract void LoadReportItems(DataView dv);
        protected abstract void ProcessTable(DataTable dtBilling);
        protected abstract DataTable InitTable();
        protected abstract void GenerateDataTables();

        public void Generate()
        {
            GenerateDataTables();
            GenerateViews();
        }

        protected virtual void GenerateViews()
        {
            foreach (DataTable dt in _ReportTables)
            {
                DataView dv = dt.DefaultView;
                dv.Sort = "ItemDescription";
                _ReportViews.Add(dv);
                LoadReportItems(dv);
            }
        }

        protected string DataRowFilter(DataRow dr)
        {
            return string.Format("Period = '{0}' AND ClientID = {1} AND AccountID = {2}", dr["Period"], dr["ClientID"], dr["AccountID"]);
        }

        protected void ApplyFilter()
        {
            ReportUtility.ApplyFilter(ClientAccountData, Report.BillingCategory);
        }

        protected void ApplyFormula(DataTable dt)
        {
            switch (Report.BillingCategory)
            {
                case BillingCategory.Tool:
                    LineCostUtility.CalculateToolLineCost(dt);
                    //ReportUtility.ApplyToolFormula(dt, Report.StartPeriod, Report.EndPeriod);
                    break;
                case BillingCategory.Room:
                    LineCostUtility.CalculateRoomLineCost(dt);
                    //ReportUtility.ApplyRoomFormula(dt);
                    break;
                case BillingCategory.Store:
                    //do nothing
                    break;
            }
        }

        protected void ApplyMiscCharge(DataTable dt, int id)
        {
            ReportUtility.ApplyMiscCharge(dt, ClientAccountData, Report.StartPeriod, Report.EndPeriod, Report.BillingCategory, id);
        }

        protected string ManagerName(DataRow dr)
        {
            string key = string.Empty;
            string notFoundText = string.Empty;
            switch (Report.ReportType)
            {
                case ReportTypes.JU:
                    key = "ManagerUniqueName";
                    notFoundText = "Not Found";
                    break;
                case ReportTypes.SUB:
                    key = "ManagerName";
                    notFoundText = "Manager Not Found";
                    break;
            }

            string result = string.Empty;

            DataRow[] drManagers = ManagersData.Select(string.Format("AccountID = {0}", dr["AccountID"]));

            if (!string.IsNullOrEmpty(key) && ManagersData.Columns.Contains(key))
            {
                if (drManagers.Length > 0)
                    result = RepositoryUtility.ConvertTo(drManagers[0][key], "[unknown]");
                else
                    result = notFoundText;
            }
            else
            {
                result = "column does not exist: " + key;
            }

            return result;
        }
    }
}