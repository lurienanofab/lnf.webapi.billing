using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using LNF.Repository;
using LNF.CommonTools;

namespace LNF.WebApi.Billing.Models
{
    public static class DataAccess
    {
        public static DataSet ToolBillingSelect(object parameters)
        {
            return DA.Current.GetAdapter()
                .ApplyParameters(parameters)
                .FillDataSet("ToolBilling_Select");
        }

        public static DataSet RoomBillingSelect(object parameters)
        {
            return DA.Current.GetAdapter()
                .ApplyParameters(parameters)
                .FillDataSet("RoomApportionmentInDaysMonthly_Select");
        }

        public static DataSet StoreBillingSelect(object parameters)
        {
            return DA.Current.GetAdapter()
                .ApplyParameters(parameters)
                .FillDataSet("StoreBilling_Select");
        }

        public static DataTable ClientAccountSelect(object parameters)
        {
            return DA.Current.GetAdapter()
                .ApplyParameters(parameters)
                .FillDataTable("ClientAccount_Select");
        }

        public static DataTable AccountSelect(object parameters)
        {
            return DA.Current.GetAdapter()
                .ApplyParameters(parameters)
                .FillDataTable("Account_Select");
        }

        public static DataTable MiscBillingChargeSelect(object parameters)
        {
            return DA.Current.GetAdapter()
                .ApplyParameters(parameters)
                .FillDataTable("MiscBillingCharge_Select");
        }
    }
}