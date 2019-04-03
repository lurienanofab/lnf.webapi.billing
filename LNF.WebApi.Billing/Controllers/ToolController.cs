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
    public class ToolController : ApiController
    {
        [Route("tool/data/clean")]
        public IEnumerable<ToolDataCleanItem> GetToolDataClean(DateTime sd, DateTime ed, int clientId = 0, int resourceId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                var query = DA.Current.Query<ToolDataClean>()
                .Where(x => (x.BeginDateTime < ed && x.EndDateTime > sd || x.ActualBeginDateTime < ed && x.ActualEndDateTime > ed)
                    && x.ClientID == (clientId > 0 ? clientId : x.ClientID)
                    && x.ResourceID == (resourceId > 0 ? resourceId : x.ResourceID));

                var result = query.CreateToolDataCleanItems();

                return result;
            }
        }

        [Route("tool/data/clean/{reservationId}")]
        public ToolDataCleanItem GetToolDataClean(int reservationId)
        {
            using (DA.StartUnitOfWork())
            {
                var query = DA.Current.Query<ToolDataClean>().Where(x => x.ReservationID == reservationId);
                var result = query.CreateToolDataCleanItems().FirstOrDefault();
                return result;
            }
        }

        [HttpGet, Route("tool/data/create")]
        public IEnumerable<ToolDataItem> CreateToolData(DateTime period, int clientId = 0, int resourceId = 0)
        {
            // Does the processing without saving anything to the database.

            using (DA.StartUnitOfWork())
            {
                var proc = new WriteToolDataProcess(period, clientId, resourceId);
                var dtExtract = proc.Extract();
                var dtTransform = proc.Transform(dtExtract);

                var result = dtTransform.AsEnumerable().Select(x => new ToolDataItem
                {
                    ToolDataID = x.Field<int>("ToolDataID"),
                    Period = x.Field<DateTime>("Period"),
                    ClientID = x.Field<int>("ClientID"),
                    ResourceID = x.Field<int>("ResourceID"),
                    RoomID = x.Field<int?>("RoomID"),
                    ActDate = x.Field<DateTime>("ActDate"),
                    AccountID = x.Field<int>("AccountID"),
                    Uses = x.Field<double>("Uses"),
                    SchedDuration = x.Field<double>("SchedDuration"),
                    ActDuration = x.Field<double>("ActDuration"),
                    OverTime = x.Field<double>("OverTime"),
                    Days = x.Field<double?>("Days"),
                    Months = x.Field<double?>("Months"),
                    IsStarted = x.Field<bool>("IsStarted"),
                    ChargeMultiplier = x.Field<double>("ChargeMultiplier"),
                    ReservationID = x.Field<int?>("ReservationID"),
                    ChargeDuration = x.Field<double>("ChargeDuration"),
                    TransferredDuration = x.Field<double>("TransferredDuration"),
                    MaxReservedDuration = x.Field<double>("MaxReservedDuration"),
                    ChargeBeginDateTime = x.Field<DateTime?>("ChargeBeginDateTime"),
                    ChargeEndDateTime = x.Field<DateTime?>("ChargeEndDateTime"),
                    IsActive = x.Field<bool>("IsActive"),
                    IsCancelledBeforeAllowedTime = x.Field<bool?>("IsCancelledBeforeAllowedTime")
                }).ToList();

                return result;
            }
        }


        [HttpGet, Route("tool/data/create/{reservationId}")]
        public IEnumerable<ToolDataItem> CreateToolData(int reservationId)
        {
            using (DA.StartUnitOfWork())
            {
                var tdc = DA.Current.Query<ToolDataClean>().FirstOrDefault(x => x.ReservationID == reservationId);
                if (tdc == null) return null;
                var period = tdc.GetChargeBeginDateTime().FirstOfMonth();
                // Doing it the lazy way for one reservation: create all for the client/tool and then return one.
                var items = CreateToolData(period, tdc.ClientID, tdc.ResourceID);
                var result = items.Where(x => x.ReservationID == reservationId);
                return result;
            }
        }

        [Route("tool/data")]
        public IEnumerable<ToolDataItem> GetToolData(DateTime period, int clientId = 0, int resourceId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                var query = DA.Current.Query<ToolData>()
                .Where(x => x.Period == period
                    && x.ClientID == (clientId > 0 ? clientId : x.ClientID)
                    && x.ResourceID == (resourceId > 0 ? resourceId : x.ResourceID));

                var result = query.CreateToolDataItems();

                return result;
            }
        }

        [Route("tool/data/{reservationId}")]
        public IEnumerable<ToolDataItem> GetToolData(int reservationId)
        {
            using (DA.StartUnitOfWork())
            {
                var query = DA.Current.Query<ToolData>().Where(x => x.ReservationID == reservationId);
                var result = query.CreateToolDataItems();
                return result;
            }
        }

        [HttpGet, Route("tool/create")]
        public IEnumerable<ToolBillingItem> CreateToolBilling(DateTime period, int clientId = 0)
        {
            // Does the same processing as BillingDataProcessStep1.PopulateToolBilling (transforms
            // a ToolData record into a ToolBilling record) without saving anything to the database.

            using (DA.StartUnitOfWork())
            {
                var temp = period == DateTime.Now.FirstOfMonth();

                IToolBilling[] source = BillingDataProcessStep1.GetToolData(period, clientId, 0, temp);

                var result = CreateToolBillingItems(source, temp);

                return result;
            }
        }

        [HttpGet, Route("tool/create/{reservationId}")]
        public IEnumerable<ToolBillingItem> CreateToolBilling(int reservationId)
        {
            // Does the same processing as BillingDataProcessStep1.PopulateToolBilling (transforms
            // a ToolData record into a ToolBilling record) without saving anything to the database.

            using (DA.StartUnitOfWork())
            {
                var td = DA.Current.Query<ToolData>().FirstOrDefault(x => x.ReservationID == reservationId);

                if (td == null) return null;

                var period = td.Period;
                var temp = period == DateTime.Now.FirstOfMonth();

                IToolBilling[] source = BillingDataProcessStep1.GetToolData(period, 0, reservationId, temp);

                var result = CreateToolBillingItems(source, temp);

                return result;
            }
        }

        [Route("tool")]
        public IEnumerable<ToolBillingItem> GetToolBilling(DateTime period, int clientId = 0, int resourceId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                var temp = period == DateTime.Now.FirstOfMonth();

                var query = GetToolBillingQuery(temp).Where(x =>
                    x.Period == period
                    && x.ClientID == (clientId > 0 ? clientId : x.ClientID)
                    && x.ResourceID == (resourceId > 0 ? resourceId : x.ResourceID));

                var result = query.CreateToolBillingItems();

                return result;
            }
        }

        [Route("tool/{reservationId}")]
        public IEnumerable<ToolBillingItem> GetToolBilling(int reservationId)
        {
            using (DA.StartUnitOfWork())
            {
                var td = DA.Current.Query<ToolData>().Where(x => x.ReservationID == reservationId).ToList();

                if (td.Count == 0) return null;

                var period = td.First().Period;

                var temp = period == DateTime.Now.FirstOfMonth();

                var query = GetToolBillingQuery(temp).Where(x => x.ReservationID == reservationId);

                var result = query.CreateToolBillingItems();

                return result;
            }
        }

        private IQueryable<IToolBilling> GetToolBillingQuery(bool temp)
        {
            if (temp)
                return DA.Current.Query<ToolBillingTemp>();
            else
                return DA.Current.Query<ToolBilling>();
        }

        private IEnumerable<ToolBillingItem> CreateToolBillingItems(IEnumerable<IToolBilling> source, bool temp)
        {
            var step1 = new BillingDataProcessStep1(DateTime.Now, ServiceProvider.Current);
            foreach (IToolBilling tb in source)
                step1.CalculateToolBillingCharges(tb);

            var result = source.AsQueryable().CreateToolBillingItems();

            return result;
        }
    }
}
