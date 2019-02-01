using LNF.CommonTools;
using LNF.Models.Billing;
using LNF.Repository;
using LNF.Repository.Billing;
using LNF.Repository.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class StoreController : ApiController
    {
        [Route("store/data/clean")]
        public IEnumerable<StoreDataCleanItem> GetStoreDataClean(DateTime sd, DateTime ed, int clientId = 0, int itemId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                var query = DA.Current.Query<StoreDataClean>()
                    .Where(x => x.StatusChangeDate >= sd && x.StatusChangeDate < ed
                        && x.Client.ClientID == (clientId > 0 ? clientId : x.Client.ClientID)
                        && x.Item.ItemID == (itemId > 0 ? itemId : x.Item.ItemID));

                var result = query.CreateStoreDataCleanItems();

                return result;
            }
        }

        [HttpGet, Route("store/data/create")]
        public IEnumerable<StoreDataItem> CreateStoreData(DateTime period, int clientId = 0, int itemId = 0)
        {
            // Does the processing without saving anything to the database.

            using (DA.StartUnitOfWork())
            {
                var proc = new WriteStoreDataProcess(period, clientId, itemId);
                var dtExtract = proc.Extract();
                var dtTransform = proc.Transform(dtExtract);

                var result = dtTransform.AsEnumerable().Select(x => new StoreDataItem
                {
                    StoreDataID = x.Field<int>("StoreDataID"),
                    Period = x.Field<DateTime>("Period"),
                    ClientID = x.Field<int>("ClientID"),
                    ItemID = x.Field<int>("ItemID"),
                    OrderDate = x.Field<DateTime>("OrderDate"),
                    AccountID = x.Field<int>("AccountID"),
                    Quantity = x.Field<double>("Quantity"),
                    UnitCost = x.Field<double>("UnitCost"),
                    CategoryID = x.Field<int>("CategoryID"),
                    StatusChangeDate = x.Field<DateTime>("StatusChangeDate")
                }).ToList();

                return result;
            }
        }

        [Route("store/data")]
        public IEnumerable<StoreDataItem> GetStoreData(DateTime period, int clientId = 0, int itemId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                var query = DA.Current.Query<StoreData>()
                .Where(x => x.Period == period
                    && x.ClientID == (clientId > 0 ? clientId : x.ClientID)
                    && x.ItemID == (itemId > 0 ? itemId : x.ItemID));

                var result = query.CreateStoreDataItems();

                return result;
            }
        }

        [Route("store/create")]
        public IEnumerable<StoreBillingItem> CreateStoreBilling(DateTime period, int clientId = 0)
        {
            // Does the same processing as BillingDataProcessStep1.PopulateStoreBilling (transforms
            // a StoreData record into a StoreBilling record) without saving anything to the database.

            using (DA.StartUnitOfWork())
            {
                var dt = BillingDataProcessStep1.GetStoreData(period);

                var result = dt.AsEnumerable().Select(x => new StoreBillingItem
                {
                    StoreBillingID = x.Field<int>("StoreBillingID"),
                    Period = x.Field<DateTime>("Period"),
                    ClientID = x.Field<int>("ClientID"),
                    AccountID = x.Field<int>("AccountID"),
                    ChargeTypeID = x.Field<int>("ChargeTypeID"),
                    ItemID = x.Field<int>("ItemID"),
                    CategoryID = x.Field<int>("CategoryID"),
                    Quantity = x.Field<decimal>("Quantity"),
                    UnitCost = x.Field<decimal>("UnitCost"),
                    CostMultiplier = x.Field<decimal>("CostMultiplier"),
                    LineCost = x.Field<decimal>("LineCost"),
                    StatusChangeDate = x.Field<DateTime>("StatusChangeDate"),
                    IsTemp = x.Field<bool>("IsTemp")
                }).ToList();

                return result;
            }
        }

        [Route("store")]
        public IEnumerable<StoreBillingItem> GetStoreBilling(DateTime period, int clientId = 0, int itemId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                var temp = period == DateTime.Now.FirstOfMonth();

                var query = GetStoreBillingQuery(temp).Where(x =>
                    x.Period == period
                    && x.ClientID == (clientId > 0 ? clientId : x.ClientID)
                    && x.ItemID == (itemId > 0 ? itemId : x.ItemID));

                var result = query.CreateStoreBillingItems();

                return result;
            }
        }

        private IQueryable<IStoreBilling> GetStoreBillingQuery(bool temp)
        {
            if (temp)
                return DA.Current.Query<StoreBillingTemp>();
            else
                return DA.Current.Query<StoreBilling>();
        }
    }
}
